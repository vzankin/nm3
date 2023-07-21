using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddNmServices(builder.Configuration);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorise(options => options.Immediate = true);
builder.Services.AddBootstrapProviders();
builder.Services.AddFontAwesomeIcons();
builder.Services.AddLocalization(opt => opt.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(opt =>
{
    opt.SupportedCultures!.Clear();
    opt.AddSupportedCultures("en", "it");
    opt.SupportedUICultures!.Clear();
    opt.AddSupportedUICultures("en", "it");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseRequestLocalization();
app.UseStaticFiles();
app.UseRouting();

app.MapGet("/culture", CompEd.Nm.Net.Api.Culture.OnGet);
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
