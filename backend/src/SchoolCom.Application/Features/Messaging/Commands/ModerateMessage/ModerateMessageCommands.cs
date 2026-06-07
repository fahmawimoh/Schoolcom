using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Messaging.Commands.ModerateMessage;

public record ApproveMessageCommand(Guid MessageId, string? Notes) : IRequest;

public record RejectMessageCommand(Guid MessageId, string? Notes) : IRequest;

public class ApproveMessageCommandHandler(
    IMessagingRepository repository,
    ICurrentUserService currentUser,
    IAuditService audit,
    IChatNotifier chatNotifier) : IRequestHandler<ApproveMessageCommand>
{
    public async Task Handle(ApproveMessageCommand request, CancellationToken cancellationToken)
    {
        if (!MessagingAuthorization.IsAdmin(currentUser.Roles))
            throw new UnauthorizedAccessException("Admin access required.");

        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");
        var adminId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not found");

        var message = await repository.GetMessageAsync(tenantId, request.MessageId, cancellationToken)
            ?? throw new KeyNotFoundException("Message not found");

        await repository.ApproveMessageAsync(tenantId, request.MessageId, adminId, request.Notes, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await audit.LogAsync("Approve", "Message", request.MessageId, null, cancellationToken);

        var notifications = await repository.GetNotificationsAsync(tenantId, message.SenderId, cancellationToken);
        var latest = notifications.FirstOrDefault();
        if (latest is not null)
            await chatNotifier.NotifyNotificationAsync(message.SenderId, latest, cancellationToken);

        await chatNotifier.NotifyModerationUpdateAsync(
            message.ConversationId, message.Id, "Approved", cancellationToken);

        var refreshed = await repository.GetMessagesAsync(
            tenantId,
            message.ConversationId,
            message.SenderId,
            true,
            false,
            null,
            5,
            null,
            cancellationToken);
        var dto = refreshed.Items.FirstOrDefault(m => m.Id == message.Id);
        if (dto is not null)
            await chatNotifier.NotifyMessageAsync(message.ConversationId, dto, cancellationToken);
    }
}

public class RejectMessageCommandHandler(
    IMessagingRepository repository,
    ICurrentUserService currentUser,
    IAuditService audit,
    IChatNotifier chatNotifier) : IRequestHandler<RejectMessageCommand>
{
    public async Task Handle(RejectMessageCommand request, CancellationToken cancellationToken)
    {
        if (!MessagingAuthorization.IsAdmin(currentUser.Roles))
            throw new UnauthorizedAccessException("Admin access required.");

        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");
        var adminId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not found");

        var message = await repository.GetMessageAsync(tenantId, request.MessageId, cancellationToken)
            ?? throw new KeyNotFoundException("Message not found");

        await repository.RejectMessageAsync(tenantId, request.MessageId, adminId, request.Notes, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await audit.LogAsync("Reject", "Message", request.MessageId, null, cancellationToken);

        var notifications = await repository.GetNotificationsAsync(tenantId, message.SenderId, cancellationToken);
        var latest = notifications.FirstOrDefault();
        if (latest is not null)
            await chatNotifier.NotifyNotificationAsync(message.SenderId, latest, cancellationToken);

        await chatNotifier.NotifyModerationUpdateAsync(
            message.ConversationId, message.Id, "Rejected", cancellationToken);
    }
}
