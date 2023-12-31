﻿using MailKit.Net.Imap;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CompEd.Nm.Net.Job;

[DisallowConcurrentExecution]
internal class MailboxJobWatch : MailboxJob
{
    public MailboxJobWatch(ImapClient imap, ILogger<MailboxJobWatch>? log) : base(imap, log)
    {
    }

    protected override async Task ExecuteInternal(IJobExecutionContext ctx)
    {
        while (!ctx.CancellationToken.IsCancellationRequested)
        {
            // ensure connected
            await ReconnectAsync(imap, ctx.CancellationToken).ConfigureAwait(false);

            // trigger mailbox syncronizing job (MailboxJobCheck)
            await Monitor!.TriggerCheck(ctx.CancellationToken).ConfigureAwait(false);

            // wait for mailbox changing event, or timeout (5 minutes for IDLE, 1 minute for NOOP)
            if (imap.Capabilities.HasFlag(ImapCapabilities.Idle))
                await Idle(imap, ctx.CancellationToken).ConfigureAwait(false);
            else
                await Noop(imap, ctx.CancellationToken).ConfigureAwait(false);
        }
    }

    private async Task Idle(ImapClient imap, CancellationToken ct)
    {
        using var time = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        using var done = CancellationTokenSource.CreateLinkedTokenSource(time.Token, ct);

        void callback(object? sender, EventArgs args) => time.Cancel();

        log?.LogTrace("{mailbox}: +IDLE", Monitor.Mailbox.Name);
        imap.Inbox.CountChanged += callback;
        await imap.IdleAsync(done.Token).ConfigureAwait(false);
        imap.Inbox.CountChanged -= callback;
        log?.LogTrace("{mailbox}: -IDLE", Monitor.Mailbox.Name);
    }
    private async Task Noop(ImapClient imap, CancellationToken ct)
    {
        log?.LogTrace("{mailbox}: +NOOP", Monitor.Mailbox.Name);
        await imap.NoOpAsync(ct).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromMinutes(1), ct).ConfigureAwait(false);
        log?.LogTrace("{mailbox}: -NOOP", Monitor.Mailbox.Name);
    }
}
