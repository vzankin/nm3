using MailKit.Net.Imap;
using Quartz;

namespace CompEd.Nm.Net.Job;

[DisallowConcurrentExecution]
internal class MailboxJobClean : MailboxJob
{
    public MailboxJobClean(ImapClient imap, ILogger<MailboxJobClean> log) : base(imap, log)
    {
    }

    protected override async Task ExecuteInternal(IJobExecutionContext ctx)
    {
        // emulate clean job working
        await Task.Delay(TimeSpan.FromSeconds(10), ctx.CancellationToken);
    }
}
