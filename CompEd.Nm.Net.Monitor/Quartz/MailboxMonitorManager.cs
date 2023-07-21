using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CompEd.Nm.Net;

public class MailboxMonitorManager : BackgroundService
{
    private readonly IServiceProvider sp;
    private readonly ISchedulerFactory sf;
    private readonly ILogger? log;
    private readonly Dictionary<string, MailboxMonitor> monitors = new();

    public MailboxMonitorManager(IServiceProvider sp, ISchedulerFactory sf, ILogger<MailboxMonitorManager>? log)
    {
        this.sp = sp;
        this.sf = sf;
        this.log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await using var scope = sp.CreateAsyncScope();
        using var db = scope.ServiceProvider.GetRequiredService<Db.MainContext>();
        await db.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);

        foreach (var mailbox in db.Mailboxes)
        {
            log?.LogInformation("create scheduler jobs for '{mailbox}'", mailbox.Name);
            monitors[mailbox.Name] = await MailboxMonitor.Create(sf, mailbox, ct);
        }
    }
}
