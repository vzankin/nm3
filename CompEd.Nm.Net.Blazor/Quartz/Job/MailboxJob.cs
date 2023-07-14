using MailKit.Net.Imap;
using MailKit;
using Quartz;

namespace CompEd.Nm.Net.Job;

internal abstract class MailboxJob : IJob, IDisposable
{
    protected readonly ImapClient imap;
    protected readonly ILogger? log;

    protected MailboxJob(ImapClient imap, ILogger? log)
    {
        this.imap = imap;
        this.log = log;
    }

    public void Dispose() =>
        this.imap.Dispose();

    public MailboxMonitor Monitor { get; set; } = default!;

    protected async Task ReconnectAsync(ImapClient imap, CancellationToken ct)
    {
        if (!imap.IsConnected)
            await imap.ConnectAsync(Monitor.Mailbox!.ImapHost, Monitor.Mailbox.ImapPort ?? 993, true, ct).ConfigureAwait(false);
        if (imap.Capabilities.HasFlag(ImapCapabilities.Compress))
            await imap.CompressAsync(ct).ConfigureAwait(false);
        if (!imap.IsAuthenticated)
            await imap.AuthenticateAsync(Monitor.Mailbox!.ImapUsername, Monitor.Mailbox!.ImapPassword, ct).ConfigureAwait(false);
        if (!imap.Inbox.IsOpen)
            await imap.Inbox.OpenAsync(FolderAccess.ReadOnly, ct).ConfigureAwait(false);
    }

    public async Task Execute(IJobExecutionContext ctx)
    {
        log?.LogDebug("{key} job has started.", ctx.JobDetail.Key);
        try
        {
            await ExecuteInternal(ctx).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            log?.LogDebug("{key} job has cancelled.", ctx.JobDetail.Key);
        }
        catch (Exception e)
        {
            log?.LogWarning(e, "{mailbox}: {msg}. restarting...", Monitor.Mailbox.Name, e.InnerMessage());
            await Task.Delay(TimeSpan.FromSeconds(10), ctx.CancellationToken).ConfigureAwait(false);
            throw new JobExecutionException(e.Message, cause: e, refireImmediately: true); // restart job
        }
        finally
        {
            log?.LogDebug("{key} job has finished.", ctx.JobDetail.Key);
        }
    }

    protected abstract Task ExecuteInternal(IJobExecutionContext ctx);
}
