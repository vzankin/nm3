using CompEd.Nm.Net;
using CompEd.Nm.Net.Db;
using CompEd.Nm.Net.Imap;
using Microsoft.Extensions.Logging.EventLog;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 1. configure logging
builder.Services.AddLogging(log =>
{
    log.ClearProviders();
    log.AddSimpleConsole();
});
// 2. Configure event log settings (will be used if app run as service)
builder.Services.Configure<EventLogSettings>(opt => { opt.LogName = "CompEd"; opt.SourceName = "CompEd.Nm.Net"; });
// 3. Add Quartz services
builder.Services.AddQuartz(opt =>
{
    opt.SchedulerName = "Notification Monitor 2.0 Quartz Scheduler";
    opt.InterruptJobsOnShutdownWithWait = true;
    opt.UseMicrosoftDependencyInjectionJobFactory(); // IJob instances will be created by DI to able inject dependencies.
});
// 4. Add QuartzHostedService (IHostedService implementation)
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
// 5. Add MailboxMonitorManager AFTER Quartz hosted service.
builder.Services.AddHostedService<MailboxMonitorManager>();
// 6. Add MailKit.IProtocolLogger implementation, so it can use configured ILogger from DI
builder.Services.AddTransient<MailKit.IProtocolLogger, ImapLogger>();
// 7. Add ImapClient, so it will take IProtocolLogger implementation from DI
builder.Services.AddTransient<MailKit.Net.Imap.ImapClient>();
// 8. Add main DB context (one with global parameters, like mailbox list, etc.)
builder.Services.AddDbContext<MainContext>();
// 9. Add Nm.Net.Settings from 'appsettings.js'
builder.Services.Configure<Settings>(builder.Configuration.GetSection(Settings.Section));
// 10. Add ContextFactory for contexts which have different connection string for different mailbox.
builder.Services.AddScoped<ContextFactory>();


var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
