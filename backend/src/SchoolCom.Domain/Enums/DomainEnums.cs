namespace SchoolCom.Domain.Enums;

public enum UserRole
{
    SuperAdmin = 1,
    SchoolAdmin = 2,
    Teacher = 3,
    Parent = 4
}

public enum ModerationStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    ChangesRequested = 4
}

public enum MessageType
{
    Text = 1,
    Image = 2,
    Video = 3,
    Document = 4,
    Voice = 5,
    Announcement = 6,
    System = 7
}

public enum OtpChannel
{
    Sms = 1,
    Email = 2
}

public enum ModerationAction
{
    Approve = 1,
    Reject = 2,
    RequestChanges = 3
}

public enum MessageDeliveryStatus
{
    Sent = 1,
    Delivered = 2,
    Read = 3
}

public enum NotificationType
{
    MessageApproved = 1,
    MessageRejected = 2,
    NewMessage = 3,
    MessagePendingApproval = 4
}
