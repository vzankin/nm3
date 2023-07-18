using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddLocalization(opt => opt.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(builder.Configuration.GetSection("Localization"));

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