namespace SchoolCom.Application.Features.Messaging;

public record ConversationListItemDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    string ParentName,
    string TeacherName,
    Guid? OtherParticipantId,
    string Subject,
    string? LastMessagePreview,
    DateTime? LastMessageAt,
    int UnreadCount,
    bool OtherParticipantOnline);

public record LinkedStudentDto(
    Guid Id,
    string FullName,
    string? ClassroomName,
    bool HasConversation);

public record MessageAttachmentDto(
    Guid Id,
    string FileName,
    string MimeType,
    long SizeBytes,
    string DownloadUrl);

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string SenderName,
    string Content,
    string MessageType,
    string ModerationStatus,
    string DeliveryStatus,
    DateTime SentAt,
    IReadOnlyList<MessageAttachmentDto> Attachments,
    bool IsOwnMessage,
    string? RejectionReason = null);

public record MessageListResult(
    IReadOnlyList<MessageDto> Items,
    string? NextCursor,
    bool HasMore);

public record PendingModerationDto(
    Guid ApprovalRequestId,
    Guid MessageId,
    Guid ConversationId,
    Guid StudentId,
    Guid ParentUserId,
    Guid TeacherUserId,
    string StudentName,
    string SenderName,
    string TeacherName,
    string Content,
    string MessageType,
    DateTime SentAt,
    IReadOnlyList<MessageAttachmentDto> Attachments);

public record ModerationHistoryDto(
    Guid MessageId,
    string StudentName,
    string SenderName,
    string Action,
    string? Notes,
    string ReviewerName,
    DateTime ReviewedAt);

public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    Guid? ReferenceId,
    bool IsRead,
    DateTime CreatedAt);

public record SendMessageResult(MessageDto Message, bool RequiresApproval);

public record ModerationFilter(
    Guid? StudentId = null,
    Guid? ParentUserId = null,
    Guid? TeacherUserId = null,
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null);
