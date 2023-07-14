namespace CompEd.Nm.Net;

internal static class ExceptionExtensions
{
    internal static string InnerMessage(this Exception e) =>
        e.InnerException?.InnerMessage() ?? e.Message;
}
