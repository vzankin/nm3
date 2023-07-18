using CompEd.Nm.Net.Job;
using Quartz;

namespace CompEd.Nm.Net;

internal class MailboxMonitor : IAsyncDisposable
{
    private readonly IScheduler scheduler;

    internal MailboxMonitor(IScheduler scheduler, Db.Model.Mailbox mailbox)
    {
        this.scheduler = scheduler;
        Mailbox = mailbox;
    }

    public async ValueTask DisposeAsync()
    {
        await Interrupt(default).ConfigureAwait(false);
        await DeleteJobs(default).ConfigureAwait(false);
    }

    internal Db.Model.Mailbox Mailbox { get; private set; }

    private static async Task AddJob<TMailboxJob>(IScheduler scheduler, string group, string name, CancellationToken ct)
        where TMailboxJob : MailboxJob
    {
        var key = JobKey.Create(name, group);
        var job = JobBuilder.Create<TMailboxJob>()
            .WithIdentity(key)
            .StoreDurably() // job with this key will remain registered even if there is no active triggers.
            .Build();
        await scheduler.AddJob(job, true, ct).ConfigureAwait(false);
    }
    internal static async Task<MailboxMonitor> Create(ISchedulerFactory sf, Db.Model.Mailbox mailbox, CancellationToken ct)
    {
        var scheduler = await sf.GetScheduler(ct).ConfigureAwait(false);
        await AddJob<MailboxJobWatch>(scheduler, mailbox.Name, "watch", ct).ConfigureAwait(false);
        await AddJob<MailboxJobCheck>(scheduler, mailbox.Name, "check", ct).ConfigureAwait(false);
        await AddJob<MailboxJobClean>(scheduler, mailbox.Name, "clean", ct).ConfigureAwait(false);
        var monitor = new MailboxMonitor(scheduler, mailbox);
        await monitor.Trigger(ct).ConfigureAwait(false);
        return monitor;
    }

    private Task DeleteJob(string name, CancellationToken ct)
    {
        var key = JobKey.Create(name, Mailbox.Name);
        return scheduler.DeleteJob(key, ct);
    }
    private async Task DeleteJobs(CancellationToken ct)
    {
        var tasks = new List<Task>
        {
            DeleteJob("watch", ct),
            DeleteJob("clean", ct),
            DeleteJob("check", ct)
        };
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private Task Trigger(string name, CancellationToken ct)
    {
        var key = JobKey.Create(name, Mailbox!.Name);
        var map = new JobDataMap { ["Monitor"] = this };
        return scheduler.TriggerJob(key, map, ct);
    }
    internal async Task Trigger(CancellationToken ct)
    {
        var tasks = new List<Task>();
        if (Mailbox.IsWatchEnabled == true)
            tasks.Add(TriggerWatch(ct));
        if (Mailbox.IsCleanEnabled == true)
            tasks.Add(TriggerClean(ct));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private Task Interrupt(string name, CancellationToken ct)
    {
        var key = JobKey.Create(name, Mailbox.Name);
        return scheduler.Interrupt(key, ct);
    }
    internal async Task Interrupt(CancellationToken ct)
    {
        var tasks = new List<Task>
        {
            Interrupt("watch", ct),
            Interrupt("clean", ct),
            Interrupt("check", ct)
        };
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    internal Task TriggerWatch(CancellationToken ct) =>
        Trigger("watch", ct);
    internal Task TriggerClean(CancellationToken ct) =>
        Trigger("clean", ct);
    internal Task TriggerCheck(CancellationToken ct) =>
        Trigger("check", ct);
}
