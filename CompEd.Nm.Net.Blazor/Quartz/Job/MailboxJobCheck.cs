using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using CompEd.Nm.Net.Db;
using CompEd.Nm.Net.Db.Model;
using Quartz;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Xml.XPath;

namespace CompEd.Nm.Net.Job;

[DisallowConcurrentExecution]
internal partial class MailboxJobCheck : MailboxJob
{
    private readonly Settings settings;
    private readonly ContextFactory cf;

    public MailboxJobCheck(IOptions<Settings> settings, ContextFactory cf, ImapClient imap, ILogger<MailboxJobCheck> log) : base(imap, log)
    {
        this.settings = settings.Value;
        this.cf = cf;
    }

    protected override async Task ExecuteInternal(IJobExecutionContext ctx)
    {
        // 0. ensure connected
        await ReconnectAsync(imap, ctx.CancellationToken).ConfigureAwait(false);

        // 1. get mailbox folder
        var folder = GetMailboxFolder();

        // 2. open database context
        using var cache = await cf.CreateCacheContext(Monitor.Mailbox, ctx.CancellationToken).ConfigureAwait(false);

        // 3. delete outdated UIDs from local cache (with unactual UidValidity)
        await cache.Mails
            .Where(x => x.UidValidity != imap.Inbox.UidValidity)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.UidValidity, new uint?()).SetProperty(x => x.Uid, new uint?()), ctx.CancellationToken)
            .ConfigureAwait(false);
        await cache.SaveChangesAsync(ctx.CancellationToken).ConfigureAwait(false);

        // 4. get local UIDs
        var uidsLocal = new UniqueIdSet(cache.Mails.Where(x => x.Uid.HasValue && x.UidValidity.HasValue).Select(x => x.UniqueId).AsEnumerable().OfType<UniqueId>());

        // 5. get remote UIDs
        var uidsRemote = new UniqueIdSet(await imap.Inbox.SearchAsync(SearchQuery.All, ctx.CancellationToken).ConfigureAwait(false));

        // 6. get UIDs to delete from local cache (ones that present in local cache but not on remote server)
        var uidsToDel = uidsLocal.Except(uidsRemote);

        // 7.1 notify about deleted mails
        foreach (var mail in cache.Mails.Where(x => x.Uid.HasValue && uidsToDel.Select(x => x.Id).Contains(x.Uid.Value)))
            log?.LogInformation("{mailbox}: '{sbj}' marked as deleted", Monitor.Mailbox.Name, mail.Subject);

        // 7.2 delete UIDs from local cache
        await cache.Mails
            .Where(x => x.Uid.HasValue && uidsToDel.Select(x => x.Id).Contains(x.Uid.Value))
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.UidValidity, new uint?()).SetProperty(x => x.Uid, new uint?()), ctx.CancellationToken)
            .ConfigureAwait(false);
        await cache.SaveChangesAsync(ctx.CancellationToken).ConfigureAwait(false);

        // 8. get UIDs to add to local cache (ones that present on remote server but not in local cache)
        var uidsToAdd = uidsRemote.Except(uidsLocal);

        // 9. check incoming emails (by 100 items):
        foreach (var uidsBatch in uidsToAdd.Chunk(100))
        {
            // 9.1 fetch email headers
            var summaries = await imap.Inbox.FetchAsync(uidsBatch, MessageSummaryItems.Envelope|MessageSummaryItems.Headers|MessageSummaryItems.InternalDate|MessageSummaryItems.BodyStructure, ctx.CancellationToken).ConfigureAwait(false);

            // 9.2 prepare Mail records to insert
            var toInsert = new List<Mail>();
            foreach (var summary in summaries)
            {
                var mail = cache.Mails.FirstOrDefault(x => x.MessageId == summary.Envelope.MessageId);
                if (mail != null)
                {
                    // if there is message with such MessageId, just update UID
                    mail.UniqueId = summary.UniqueId;
                    cache.Mails.Update(mail);
                }
                else
                    toInsert.Add(FromSummary(summary));
                log?.LogInformation("{mailbox}: '{sbj}' checked", Monitor.Mailbox.Name, summary.NormalizedSubject);
            }

            // 9.3 Download all SdI notifications
            foreach (var mail in toInsert.Where(x => x.SdiType != null))
            {
                await DownloadSdi(folder, mail, ctx.CancellationToken).ConfigureAwait(false);
                log?.LogInformation("{mailbox}: '{sbj}' downloaded. [{sdiId}:{sdiType}]", Monitor.Mailbox.Name, mail.Subject, mail.SdiId, mail.SdiType);
            }

            // 9.4 Download PEC notifications only for outgoing mails that presents in cache (i.e. for sent EC mail)
            foreach (var mail in toInsert.Where(x => cache.Mails.Any(m => m.MessageId == x.PecId)))
            {
                await DownloadPec(folder, mail, ctx.CancellationToken).ConfigureAwait(false);
                log?.LogInformation("{mailbox}: '{sbj}' downloaded. [{pecId}:{pecType}]", Monitor.Mailbox.Name, mail.Subject, mail.PecId, mail.PecType);
            }

            // 9.5 insert Mail records batch
            await cache.Mails.AddRangeAsync(toInsert, ctx.CancellationToken).ConfigureAwait(false);
            await cache.SaveChangesAsync(ctx.CancellationToken).ConfigureAwait(false);
        }
    }

    private string GetMailboxFolder()
    {
        var folder = Monitor.Mailbox.Folder ?? Path.Combine(settings.RootFolder);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return folder;
    }

    [GeneratedRegex(@"([a-z][a-z][0-9a-z]{2,28}_[0-9a-z]{1,5})_(AT|DT|EC|MC|MT|NE|NR|NS|RC|SE)_[0-9a-z]{1,3}\.(xml|zip)", RegexOptions.IgnoreCase)]
    private static partial Regex RxSdiMsg();

    private static Mail FromSummary(IMessageSummary summary)
    {
        var mail = new Mail();
        mail.UniqueId = summary.UniqueId;
        mail.MessageId = summary.Envelope.MessageId;
        mail.Subject = summary.NormalizedSubject;
        mail.From = summary.Envelope.From.ToString();
        mail.Date = summary.InternalDate?.UtcDateTime;

        var ricevuta = summary.Headers["X-Ricevuta"];
        if (ricevuta != null)
        {
            mail.PecType = ricevuta;
            mail.PecId = summary.Headers["X-Riferimento-Message-ID"].TrimStart('<').TrimEnd('>');
        }

        var trasporto = summary.Headers["X-Trasporto"];
        if (trasporto != null)
        {
            mail.PecType = trasporto;
            if (summary.BodyParts.FirstOrDefault(x => x.FileName == "postacert.eml") is BodyPartMessage nested)
            {
                var match = (nested.Body as BodyPartMultipart)?
                    .BodyParts?
                    .OfType<BodyPartBasic>()
                    .Where(x => x.IsAttachment)
                    .Select(x => RxSdiMsg().Match(x.FileName))
                    .Where(x => x.Success)
                    .FirstOrDefault();
                if (match != null)
                {
                    mail.SdiId = match.Value;
                    mail.SdiType = match.Groups[2].Value;
                }
            }
        }

        return mail;
    }

    private async Task<string> DownloadPec(string folder, Mail mail, CancellationToken ct)
    {
        var date = mail.Date ?? DateTime.Now;
        var path = Path.Combine(folder, $"{date.Year:D4}", $"{date.Month:D2}", $"{date.Day:D2}");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        path = Path.Combine(path, $"{mail.MessageId}.eml");
        using var stream = await imap.Inbox.GetStreamAsync(mail.UniqueId!.Value, "", ct).ConfigureAwait(false);
        using var fs = File.Create(path);
        await stream.CopyToAsync(fs, ct).ConfigureAwait(false);
        return path;
    }
    private async Task<string> DownloadSdi(string folder, Mail mail, CancellationToken ct = default)
    {
        var path = await DownloadPec(folder, mail, ct).ConfigureAwait(false);
        using var fs = File.OpenRead(path);
        using var mime = await MimeMessage.LoadAsync(fs, ct).ConfigureAwait(false);
        var nested = mime.Body.NestedMessage("postacert.eml");
        if (nested != null)
        {
            // Get attachment with SdI notification xml (MT|DT|SE)
            var sdiAtt = nested.Attachments.OfType<MimePart>().FirstOrDefault(x => x.FileName == mail.SdiId);
            if (sdiAtt != null)
            {
                // Get SdI notification stream
                await using var sdiStm = new MemoryStream();
                await sdiAtt.Content.DecodeToAsync(sdiStm, ct).ConfigureAwait(false);
                // Load XPathDocument from SdI notification stream
                sdiStm.Seek(0, SeekOrigin.Begin);
                var sdiDoc = new XPathDocument(sdiStm);
                var sdiNav = sdiDoc.CreateNavigator();
                // Get SdI notification data
                mail.SdiId = sdiNav.SelectSingleNode("/*/IdentificativoSdI")?.Value;
            }
        }
        return path;
    }
}
