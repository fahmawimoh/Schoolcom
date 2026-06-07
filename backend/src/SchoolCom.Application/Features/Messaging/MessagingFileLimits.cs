using SchoolCom.Domain.Enums;

namespace SchoolCom.Application.Features.Messaging;

public static class MessagingFileLimits
{
    private static readonly HashSet<string> AllowedImageMimes =
        ["image/jpeg", "image/jpg", "image/png", "image/webp"];

    private static readonly HashSet<string> AllowedVideoMimes =
        ["video/mp4", "video/quicktime", "video/webm"];

    private static readonly HashSet<string> AllowedDocumentMimes =
    [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    ];
    public const long ImageMaxBytes = 2 * 1024 * 1024;
    public const long VideoMaxBytes = 20 * 1024 * 1024;
    public const long DocumentMaxBytes = 3 * 1024 * 1024;

    public static long GetMaxBytes(MessageType type) => type switch
    {
        MessageType.Image => ImageMaxBytes,
        MessageType.Video => VideoMaxBytes,
        MessageType.Document => DocumentMaxBytes,
        _ => DocumentMaxBytes
    };

    public static string GetLimitLabel(MessageType type) => type switch
    {
        MessageType.Image => "2 MB",
        MessageType.Video => "20 MB",
        MessageType.Document => "3 MB",
        _ => "3 MB"
    };

    public static MessageType ResolveMessageType(string mimeType)
    {
        if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return MessageType.Image;
        if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return MessageType.Video;
        return MessageType.Document;
    }

    public static void ValidateSize(MessageType type, long sizeBytes)
    {
        var max = GetMaxBytes(type);
        if (sizeBytes > max)
            throw new InvalidOperationException(
                $"File exceeds maximum size of {GetLimitLabel(type)} for this file type.");
    }

    public static void ValidateMimeType(string mimeType)
    {
        var type = ResolveMessageType(mimeType);
        var allowed = type switch
        {
            MessageType.Image => AllowedImageMimes,
            MessageType.Video => AllowedVideoMimes,
            MessageType.Document => AllowedDocumentMimes,
            _ => AllowedDocumentMimes
        };

        if (!allowed.Contains(mimeType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"File type '{mimeType}' is not allowed for {type} attachments.");
    }
}
