using CompEd.Nm.Net;
using CompEd.Nm.Net.Db;
using CompEd.Nm.Net.Imap;
using Microsoft.Extensions.Configuration;
using Quartz;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static IServiceCollection AddNmServices(this IServiceCollection svc, ConfigurationManager cfg)
    {
        cfg
        // Add C:\ProgramData\CompEd\Nm.Net\settings.json
        .AddJsonFile(SettingsProvider.CfgPath, true, false)
        ;

        svc
        // Add Nm.Net.Settings from 'settings.js'
        .Configure<Settings>(cfg.GetSection(Settings.Section))
        // Add SettingsProvider
        .AddSingleton<SettingsProvider>()
        // Add Settings provided by SettingsProvider
        .AddTransient<Settings>(sp => sp.GetRequiredService<SettingsProvider>().Settings)
        // Add Quartz services
        .AddQuartz(opt =>
        {
            opt.SchedulerName = "Notification Monitor 2.0 Quartz Scheduler";
            opt.InterruptJobsOnShutdownWithWait = true;
            opt.UseMicrosoftDependencyInjectionJobFactory(); // IJob instances will be created by DI to able inject dependencies.
        })
        // Add MailKit.IProtocolLogger implementation, so it can use configured ILogger from DI
        .AddTransient<MailKit.IProtocolLogger, ImapLogger>()
        // Add ImapClient, so it will take IProtocolLogger implementation from DI
        .AddTransient<MailKit.Net.Imap.ImapClient>()
        // Add main DB context (one with global parameters, like mailbox list, etc.)
        .AddDbContext<MainContext>()
        // Add ContextFactory for contexts which have different connection string for different mailbox.
        .AddScoped<CacheContextFactory>()
        // Add QuartzHostedService (IHostedService implementation)
        .AddQuartzHostedService(q => q.WaitForJobsToComplete = true)
        // Add MailboxMonitorManager AFTER Quartz hosted service.
        .AddHostedService<MailboxMonitorManager>()
        ;

        return svc;
    }
}
