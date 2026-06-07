using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;
using SchoolCom.Domain.Enums;

namespace SchoolCom.Application.Features.Messaging.Commands.SendMessage;

public record AttachmentInput(string FileName, string MimeType, Stream Content);

public record SendMessageCommand(
    Guid ConversationId,
    string Content,
    MessageType MessageType,
    IReadOnlyList<AttachmentInput>? Attachments = null) : IRequest<SendMessageResult>;

public class SendMessageCommandHandler(
    IMessagingRepository repository,
    IMessagingAccessService access,
    ICurrentUserService currentUser,
    IFileStorage fileStorage,
    IAuditService audit,
    IChatNotifier chatNotifier) : IRequestHandler<SendMessageCommand, SendMessageResult>
{
    public async Task<SendMessageResult> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        await access.EnsureCanAccessConversationAsync(request.ConversationId, cancellationToken);

        var tenantId = access.RequireTenantId();
        var userId = access.RequireUserId();
        var isParent = MessagingAuthorization.IsParent(currentUser.Roles);

        if (request.MessageType == MessageType.Text &&
            string.IsNullOrWhiteSpace(request.Content) &&
            (request.Attachments is null || request.Attachments.Count == 0))
            throw new InvalidOperationException("Message content is required.");

        var moderationStatus = isParent
            ? ModerationStatus.Pending
            : ModerationStatus.Approved;

        var message = new Message
        {
            TenantId = tenantId,
            ConversationId = request.ConversationId,
            SenderId = userId,
            Content = request.Content?.Trim() ?? string.Empty,
            MessageType = request.MessageType,
            ModerationStatus = moderationStatus,
            SentAt = DateTime.UtcNow
        };

        await repository.AddMessageAsync(message, cancellationToken);

        if (request.Attachments is not null)
        {
            foreach (var file in request.Attachments)
            {
                MessagingFileLimits.ValidateMimeType(file.MimeType);
                var type = MessagingFileLimits.ResolveMessageType(file.MimeType);
                MessagingFileLimits.ValidateSize(type, file.Content.Length);

                var stored = await fileStorage.SaveAsync(
                    file.Content, file.FileName, file.MimeType, tenantId, cancellationToken);

                await repository.AddAttachmentAsync(new MessageAttachment
                {
                    MessageId = message.Id,
                    FileName = file.FileName,
                    StorageKey = stored.StorageKey,
                    MimeType = file.MimeType,
                    SizeBytes = stored.SizeBytes,
                    ModerationStatus = moderationStatus
                }, cancellationToken);
            }
        }

        if (isParent)
        {
            await repository.AddApprovalRequestAsync(new MessageApprovalRequest
            {
                TenantId = tenantId,
                MessageId = message.Id,
                SubmittedByUserId = userId,
                Status = ModerationStatus.Pending
            }, cancellationToken);

            var adminIds = await repository.GetAdminUserIdsAsync(tenantId, cancellationToken);
            foreach (var adminId in adminIds)
            {
                await repository.AddNotificationAsync(new Notification
                {
                    TenantId = tenantId,
                    UserId = adminId,
                    Type = NotificationType.MessagePendingApproval,
                    Title = "Message pending approval",
                    Body = "A parent message is waiting for moderation.",
                    ReferenceId = message.Id,
                    IsRead = false
                }, cancellationToken);
            }
        }

        var conversation = await repository.GetConversationAsync(tenantId, request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException("Conversation not found");

        var receipts = conversation.Participants
            .Where(p => p.UserId != userId)
            .Select(p => new MessageReceipt
            {
                MessageId = message.Id,
                RecipientUserId = p.UserId,
                Status = MessageDeliveryStatus.Sent
            });
        await repository.AddReceiptsAsync(receipts, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        await audit.LogAsync(
            "Send", "Message", message.Id, new { request.ConversationId, moderationStatus }, cancellationToken);

        var sender = await repository.GetUserAsync(userId, cancellationToken);
        var attachments = message.Attachments.Select(a => new MessageAttachmentDto(
            a.Id, a.FileName, a.MimeType, a.SizeBytes,
            $"/v1/messaging/attachments/{a.Id}/download")).ToList();

        var dto = new MessageDto(
            message.Id,
            message.ConversationId,
            message.SenderId,
            sender is null ? "" : $"{sender.FirstName} {sender.LastName}",
            message.Content,
            message.MessageType.ToString(),
            message.ModerationStatus.ToString(),
            MessageDeliveryStatus.Sent.ToString(),
            message.SentAt,
            attachments,
            true);

        if (moderationStatus == ModerationStatus.Approved)
            await chatNotifier.NotifyMessageAsync(request.ConversationId, dto, cancellationToken);

        return new SendMessageResult(dto, moderationStatus == ModerationStatus.Pending);
    }
}
