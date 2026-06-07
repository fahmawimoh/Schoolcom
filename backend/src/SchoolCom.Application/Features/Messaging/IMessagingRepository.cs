using SchoolCom.Domain.Entities;
using SchoolCom.Domain.Enums;

namespace SchoolCom.Application.Features.Messaging;

public interface IMessagingRepository
{
    Task<bool> IsParticipantAsync(Guid tenantId, Guid conversationId, Guid userId, CancellationToken ct);

    Task<bool> IsAdminAsync(Guid userId, CancellationToken ct);

    Task<bool> IsParentOfStudentAsync(Guid tenantId, Guid parentUserId, Guid studentId, CancellationToken ct);

    Task<Guid?> GetPrimaryParentIdAsync(Guid tenantId, Guid studentId, CancellationToken ct);

    Task<Guid?> GetHomeroomTeacherIdAsync(Guid tenantId, Guid studentId, CancellationToken ct);

    Task<IReadOnlyList<ConversationListItemDto>> GetConversationsForUserAsync(
        Guid tenantId, Guid userId, bool isTeacher, string? search, CancellationToken ct);

    Task<IReadOnlyList<LinkedStudentDto>> GetLinkedStudentsForUserAsync(
        Guid tenantId, Guid userId, bool isTeacher, CancellationToken ct);

    Task<Conversation?> GetConversationAsync(Guid tenantId, Guid conversationId, CancellationToken ct);

    Task<Conversation> GetOrCreateConversationAsync(
        Guid tenantId, Guid studentId, Guid parentUserId, Guid teacherUserId, CancellationToken ct);

    Task<MessageListResult> GetMessagesAsync(
        Guid tenantId,
        Guid conversationId,
        Guid viewerUserId,
        bool viewerIsParent,
        bool viewerIsAdmin,
        string? cursor,
        int pageSize,
        string? search,
        CancellationToken ct);

    Task<Message?> GetMessageAsync(Guid tenantId, Guid messageId, CancellationToken ct);

    Task<Message> AddMessageAsync(Message message, CancellationToken ct);

    Task AddReceiptsAsync(IEnumerable<MessageReceipt> receipts, CancellationToken ct);

    Task MarkDeliveredAsync(Guid conversationId, Guid recipientUserId, CancellationToken ct);

    Task MarkReadAsync(Guid conversationId, Guid recipientUserId, CancellationToken ct);

    Task<IReadOnlyList<PendingModerationDto>> GetPendingApprovalsAsync(
        Guid tenantId,
        ModerationFilter filter,
        CancellationToken ct);

    Task<IReadOnlyList<ModerationHistoryDto>> GetModerationHistoryAsync(
        Guid tenantId,
        ModerationFilter filter,
        int limit,
        CancellationToken ct);

    Task<IReadOnlyList<Guid>> GetAdminUserIdsAsync(Guid tenantId, CancellationToken ct);

    Task<MessageApprovalRequest?> GetApprovalRequestAsync(Guid tenantId, Guid messageId, CancellationToken ct);

    Task ApproveMessageAsync(
        Guid tenantId,
        Guid messageId,
        Guid adminUserId,
        string? notes,
        CancellationToken ct);

    Task RejectMessageAsync(
        Guid tenantId,
        Guid messageId,
        Guid adminUserId,
        string? notes,
        CancellationToken ct);

    Task AddApprovalRequestAsync(MessageApprovalRequest request, CancellationToken ct);

    Task AddAttachmentAsync(MessageAttachment attachment, CancellationToken ct);

    Task<MessageAttachment?> GetAttachmentAsync(Guid tenantId, Guid attachmentId, CancellationToken ct);

    Task AddNotificationAsync(Notification notification, CancellationToken ct);

    Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(
        Guid tenantId, Guid userId, CancellationToken ct);

    Task MarkNotificationReadAsync(Guid tenantId, Guid userId, Guid notificationId, CancellationToken ct);

    Task<User?> GetUserAsync(Guid userId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
