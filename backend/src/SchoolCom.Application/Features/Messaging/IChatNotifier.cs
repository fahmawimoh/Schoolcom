namespace SchoolCom.Application.Features.Messaging;

public interface IChatNotifier
{
    Task NotifyMessageAsync(Guid conversationId, MessageDto message, CancellationToken ct = default);

    Task NotifyModerationUpdateAsync(
        Guid conversationId,
        Guid messageId,
        string moderationStatus,
        CancellationToken ct = default);

    Task NotifyNotificationAsync(Guid userId, NotificationDto notification, CancellationToken ct = default);
}
