using MimeKit;

namespace CompEd.Nm.Net;

internal static partial class MimeEntityExtensions
{
    internal static MimeMessage? NestedMessage(this MimeEntity body, string filename)
    {
        switch (body)
        {
            case MessagePart message:
                if (message.ContentDisposition.FileName == filename)
                    return message.Message;
                else
                    return message.Message.Body.NestedMessage(filename);
            case Multipart multipart:
                foreach (var part in multipart)
                {
                    var nested = part.NestedMessage(filename);
                    if (nested != null)
                        return nested;
                }
                break;
        }
        return null;
    }
}
