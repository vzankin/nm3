namespace CompEd.Nm.Net;

public static class ExceptionExtensions
{
    public static string InnerMessage(this Exception e) =>
        e.InnerException?.InnerMessage() ?? e.Message;
}
