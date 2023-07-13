namespace CompEd.Nm.Net;

public record Settings
{
    public const string Section = "NotificationMonitor";

    public string RootFolder { get; init; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "CompEd", "Nm.Net");
}
