using CompEd.Nm.Net;
using CompEd.Nm.Net.Db;
using CompEd.Nm.Net.Imap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Quartz;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, sc) =>
    {
        // 1. Add logging
        sc.AddLogging(log =>
        {
            log.ClearProviders();
            log.AddSimpleConsole();
        });

        // 2. Configure event log settings (will be used if app run as service)
        sc.Configure<EventLogSettings>(opt => { opt.LogName = "CompEd"; opt.SourceName = "CompEd.Nm.Net"; });

        // 3. Add Quartz services
        sc.AddQuartz(opt =>
        {
            opt.SchedulerName = "Notification Monitor 2.0 Quartz Scheduler";
            opt.InterruptJobsOnShutdownWithWait = true;
            opt.UseMicrosoftDependencyInjectionJobFactory(); // IJob instances will be created by DI to able inject dependencies.
        });

        // 4. Add QuartzHostedService (IHostedService implementation)
        sc.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // 5. Add MailboxMonitorManager AFTER Quartz hosted service.
        sc.AddHostedService<MailboxMonitorManager>();

        // 6. Add MailKit.IProtocolLogger implementation, so it can use configured ILogger from DI
        sc.AddTransient<MailKit.IProtocolLogger, ImapLogger>();

        // 7. Add ImapClient, so it will take IProtocolLogger implementation from DI
        sc.AddTransient<MailKit.Net.Imap.ImapClient>();

        // 8. Add main DB context (one with global parameters, like mailbox list, etc.)
        sc.AddDbContext<MainContext>();

        // 9. Add Nm.Net.Settings from 'appsettings.js'
        sc.Configure<Settings>(ctx.Configuration.GetSection(Settings.Section));
    })
    .Build()
    .Run();