using Microsoft.Extensions.Logging.EventLog;

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
// 3. Add CompEd.Nm.Net services
builder.Services.AddNmServices(builder.Configuration);
// 4. Add IStringLocalizer<> services
builder.Services.AddLocalization(opt => opt.ResourcesPath = "Resources");


var app = builder.Build();

app.UseRequestLocalization(opt =>
{
    //opt.SetDefaultCulture("en");
    opt.AddSupportedCultures("en", "it", "ru");
    opt.AddSupportedUICultures("en", "it", "ru");
});
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
