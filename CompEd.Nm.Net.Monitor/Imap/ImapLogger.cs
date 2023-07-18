using MailKit;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CompEd.Nm.Net.Imap;

public sealed class ImapLogger : IProtocolLogger
{
    private readonly ILogger log;
    private IAuthenticationSecretDetector secret = default!;

    public ImapLogger(ILogger<MailKit.Net.Imap.ImapClient> log) =>
        this.log = log;
    public void Dispose()
    {
    }

    public IAuthenticationSecretDetector AuthenticationSecretDetector
    {
        get => secret;
        set => secret = value;
    }

    public void LogConnect(Uri uri) =>
        log?.LogDebug("connect: '{uri}'", uri.OriginalString);
    public void LogServer(byte[] buffer, int offset, int count) =>
        log?.LogTrace("rcv: {msg}", Encoding.ASCII.GetString(buffer.AsSpan(offset, count)).TrimEnd());
    public void LogClient(byte[] buffer, int offset, int count) =>
        log?.LogTrace("snd: {msg}", Encoding.ASCII.GetString(buffer.AsSpan(offset, count)).TrimEnd());
}
