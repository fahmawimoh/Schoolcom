using Microsoft.AspNetCore.SignalR;
using SchoolCom.Api.Hubs;
using SchoolCom.Application.Features.Messaging;

namespace SchoolCom.Api.Services;

public class SignalRChatNotifier(IHubContext<ChatHub> hub) : IChatNotifier
{
    public Task NotifyMessageAsync(Guid conversationId, MessageDto message, CancellationToken ct = default) =>
        hub.Clients
            .Group(ChatHub.ConversationGroup(conversationId.ToString()))
            .SendAsync("messageReceived", message, ct);

    public Task NotifyModerationUpdateAsync(
        Guid conversationId,
        Guid messageId,
        string moderationStatus,
        CancellationToken ct = default) =>
        hub.Clients
            .Group(ChatHub.ConversationGroup(conversationId.ToString()))
            .SendAsync("moderationUpdated", new { messageId, moderationStatus }, ct);

    public Task NotifyNotificationAsync(
        Guid userId,
        NotificationDto notification,
        CancellationToken ct = default) =>
        hub.Clients
            .User(userId.ToString())
            .SendAsync("notificationReceived", notification, ct);
}
