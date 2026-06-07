using SchoolCom.Domain.Common;
using SchoolCom.Domain.Enums;

namespace SchoolCom.Domain.Entities;

public class Conversation : TenantEntity
{
    public Guid StudentId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    public Student Student { get; set; } = null!;
    public ICollection<ConversationParticipant> Participants { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}

public class ConversationParticipant : IAuditableTimestamps
{
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Message : TenantEntity
{
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType MessageType { get; set; } = MessageType.Text;
    public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Pending;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public ICollection<MessageAttachment> Attachments { get; set; } = [];
    public ICollection<MessageModeration> ModerationHistory { get; set; } = [];
    public ICollection<MessageReceipt> Receipts { get; set; } = [];
    public MessageApprovalRequest? ApprovalRequest { get; set; }
}

public class MessageAttachment : BaseEntity
{
    public Guid MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Pending;

    public Message Message { get; set; } = null!;
}

public class MessageModeration : BaseEntity
{
    public Guid MessageId { get; set; }
    public Guid ReviewerId { get; set; }
    public ModerationAction Action { get; set; }
    public string? Notes { get; set; }
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

    public Message Message { get; set; } = null!;
    public User Reviewer { get; set; } = null!;
}

public class MessageReceipt : BaseEntity
{
    public Guid MessageId { get; set; }
    public Guid RecipientUserId { get; set; }
    public MessageDeliveryStatus Status { get; set; } = MessageDeliveryStatus.Sent;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public Message Message { get; set; } = null!;
    public User RecipientUser { get; set; } = null!;
}

public class MessageApprovalRequest : TenantEntity
{
    public Guid MessageId { get; set; }
    public Guid SubmittedByUserId { get; set; }
    public ModerationStatus Status { get; set; } = ModerationStatus.Pending;
    public Guid? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public Message Message { get; set; } = null!;
    public User SubmittedByUser { get; set; } = null!;
    public User? ReviewedByAdmin { get; set; }
}

public class Notification : TenantEntity
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public bool IsRead { get; set; }

    public User User { get; set; } = null!;
}

public class WhatsAppSession : TenantEntity
{
    public Guid AdminUserId { get; set; }
    public string PhoneNumberId { get; set; } = string.Empty;
    public string AccessTokenEncrypted { get; set; } = string.Empty;
    public string? ConfigJson { get; set; }
    public bool IsActive { get; set; }

    public User AdminUser { get; set; } = null!;
}

public class WhatsAppMessage : TenantEntity
{
    public Guid SessionId { get; set; }
    public string RecipientPhone { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Direction { get; set; } = "Outbound";
    public string? ExternalMessageId { get; set; }
    public string Status { get; set; } = "Sent";

    public WhatsAppSession Session { get; set; } = null!;
}

public class AuditLog : TenantEntity
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? MetadataJson { get; set; }
    public string? IpAddress { get; set; }

    public User? User { get; set; }
}
