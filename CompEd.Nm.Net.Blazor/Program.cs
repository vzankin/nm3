using CompEd.Nm.Net;
using CompEd.Nm.Net.Db;
using CompEd.Nm.Net.Imap;
using Microsoft.Extensions.Logging.EventLog;
using Quartz;

#region FILL TEST MAILBOXES
// test data
//CompEd.Nm.Net.Db.Model.Mailbox[] Mailboxes = {
//    new()
//    {
//        Name = "vladimir.zankin@hotmail.com",
//        ImapHost  = "outlook.office365.com",
//        ImapPort = 993,
//        ImapUsername = "vladimir.zankin@hotmail.com",
//        ImapPassword = "ixbrfrugphendseq",
//        IsWatchEnabled = true,
//        IsCleanEnabled = false,
//        Folder = @"d:\test\nm2\vladimir.zankin@hotmail.com",
//    },
//    new()
//    {
//        Name = "test_A@actalispec.it",
//        ImapHost  = "imap.pec.actalis.it",
//        ImapPort = 993,
//        ImapUsername = "test_A@actalispec.it",
//        ImapPassword = "Pas$1234",
//        IsWatchEnabled = true,
//        IsCleanEnabled = false,
//        Folder = @"d:\test\nm2\test_A@actalispec.it",
//    },
//    new()
//    {
//        Name = "test_B@actalispec.it",
//        ImapHost  = "imap.pec.actalis.it",
//        ImapPort = 993,
//        ImapUsername = "test_B@actalispec.it",
//        ImapPassword = "Pas$1234",
//        IsWatchEnabled = true,
//        IsCleanEnabled = false,
//        Folder = @"d:\test\nm2\test_B@actalispec.it",
//    },
//};

//// perpare connection string
//var root = @"c:\ProgramData\CompEd\Nm.Net";

//// create folder
//if (!Directory.Exists(root))
//    Directory.CreateDirectory(root);

//// fill test data
//var ob = new DbContextOptionsBuilder<MainContext>();
//using (var db = new MainContext(new Microsoft.Extensions.Options.OptionsWrapper<Settings>(new Settings()), ob.Options))
//{
//    await db.Database.EnsureDeletedAsync();
//    await db.Database.EnsureCreatedAsync();
//    await db.Mailboxes.AddRangeAsync(Mailboxes);
//    await db.SaveChangesAsync();
//}
#endregion FILL TEST MAILBOXES

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


var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
