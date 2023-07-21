using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CompEd.Nm.Net;

public class SettingsProvider
{
    public static readonly string CfgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "CompEd", "Nm.Net", "settings.json");

    public SettingsProvider(IOptions<Settings> options) =>
        Settings = options.Value;

    public Settings Settings { get; private set; }

    public void Update(Settings settings)
    {
        using var fs = File.Create(CfgPath);
        JsonSerializer.Serialize(fs, settings, new JsonSerializerOptions { WriteIndented = true });
        Settings = settings;
    }
}
