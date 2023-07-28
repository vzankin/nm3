using Radzen;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(log =>
{
    log.ClearProviders();
    log.AddSimpleConsole();
});
builder.Services.AddNmServices(builder.Configuration);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddLocalization(opt => opt.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(opt =>
{
    opt.SetDefaultCulture("en");
    opt.SupportedCultures!.Clear();
    opt.AddSupportedCultures("en", "it");
    opt.SupportedUICultures!.Clear();
    opt.AddSupportedUICultures("en", "it");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseRequestLocalization();
app.UseStaticFiles();
app.UseRouting();

app.MapGet("/culture", CompEd.Nm.Net.Api.Culture.OnGet);
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
