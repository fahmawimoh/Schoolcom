using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Features.Messaging;
using SchoolCom.Domain.Entities;
using SchoolCom.Domain.Enums;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class MessagingRepository(ApplicationDbContext db) : IMessagingRepository
{
    public Task<bool> IsParticipantAsync(Guid tenantId, Guid conversationId, Guid userId, CancellationToken ct) =>
        db.ConversationParticipants.AnyAsync(
            p => p.ConversationId == conversationId &&
                 p.Conversation.TenantId == tenantId &&
                 p.UserId == userId,
            ct);

    public Task<bool> IsAdminAsync(Guid userId, CancellationToken ct) =>
        db.UserRoleAssignments
            .Include(x => x.Role)
            .AnyAsync(
                x => x.UserId == userId &&
                     (x.Role.Name == UserRole.SchoolAdmin ||
                      x.Role.Name == UserRole.SuperAdmin),
                ct);

    public Task<bool> IsParentOfStudentAsync(
        Guid tenantId, Guid parentUserId, Guid studentId, CancellationToken ct) =>
        db.ParentStudents.AnyAsync(
            ps => ps.ParentId == parentUserId && ps.StudentId == studentId &&
                  ps.Student.TenantId == tenantId,
            ct);

    public Task<Guid?> GetPrimaryParentIdAsync(Guid tenantId, Guid studentId, CancellationToken ct) =>
        db.ParentStudents
            .Where(ps => ps.StudentId == studentId && ps.Student.TenantId == tenantId)
            .OrderBy(ps => ps.CreatedAt)
            .Select(ps => (Guid?)ps.ParentId)
            .FirstOrDefaultAsync(ct);

    public Task<Guid?> GetHomeroomTeacherIdAsync(Guid tenantId, Guid studentId, CancellationToken ct) =>
        db.Students
            .Where(s => s.TenantId == tenantId && s.Id == studentId)
            .Select(s => s.Section.HomeroomTeacherUserId)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ConversationListItemDto>> GetConversationsForUserAsync(
        Guid tenantId, Guid userId, bool isTeacher, string? search, CancellationToken ct)
    {
        var query = db.Conversations
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId &&
                        c.Participants.Any(p => p.UserId == userId));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                (c.Student.FirstNameEn + " " + c.Student.LastNameEn).ToLower().Contains(term) ||
                (c.Student.FirstNameAr + " " + c.Student.LastNameAr).Contains(term) ||
                c.Subject.ToLower().Contains(term) ||
                c.Participants.Any(p =>
                    (p.User.FirstName + " " + p.User.LastName).ToLower().Contains(term)) ||
                c.Messages.Any(m => m.Content.ToLower().Contains(term)));
        }

        var conversations = await query
            .Select(c => new
            {
                c.Id,
                c.StudentId,
                StudentName = c.Student.FirstNameEn + " " + c.Student.LastNameEn,
                ParentName = c.Participants
                    .Where(p => p.User.RoleAssignments.Any(r => r.Role.Name == UserRole.Parent))
                    .Select(p => p.User.FirstName + " " + p.User.LastName)
                    .FirstOrDefault() ?? "",
                TeacherName = c.Participants
                    .Where(p => p.User.RoleAssignments.Any(r => r.Role.Name == UserRole.Teacher))
                    .Select(p => p.User.FirstName + " " + p.User.LastName)
                    .FirstOrDefault() ?? "",
                OtherParticipantId = c.Participants
                    .Where(p => p.UserId != userId)
                    .Select(p => (Guid?)p.UserId)
                    .FirstOrDefault(),
                c.Subject,
                LastMessage = c.Messages
                    .Where(m => isTeacher
                        ? m.ModerationStatus == ModerationStatus.Approved || m.SenderId == userId
                        : m.ModerationStatus == ModerationStatus.Approved ||
                          m.SenderId == userId ||
                          m.ModerationStatus == ModerationStatus.Pending ||
                          m.ModerationStatus == ModerationStatus.Rejected)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => new { m.Content, m.SentAt, m.SenderId })
                    .FirstOrDefault(),
                Unread = c.Messages.Count(m =>
                    m.SenderId != userId &&
                    m.ModerationStatus == ModerationStatus.Approved &&
                    !m.Receipts.Any(r =>
                        r.RecipientUserId == userId && r.Status == MessageDeliveryStatus.Read))
            })
            .OrderByDescending(c => c.LastMessage != null ? c.LastMessage.SentAt : DateTime.MinValue)
            .ToListAsync(ct);

        return conversations.Select(c => new ConversationListItemDto(
            c.Id,
            c.StudentId,
            c.StudentName,
            c.ParentName,
            c.TeacherName,
            c.OtherParticipantId,
            c.Subject,
            c.LastMessage?.Content.Length > 80
                ? c.LastMessage.Content[..80] + "…"
                : c.LastMessage?.Content,
            c.LastMessage?.SentAt,
            c.Unread,
            false)).ToList();
    }

    public async Task<IReadOnlyList<LinkedStudentDto>> GetLinkedStudentsForUserAsync(
        Guid tenantId, Guid userId, bool isTeacher, CancellationToken ct)
    {
        if (isTeacher)
        {
            return await db.Students
                .AsNoTracking()
                .Where(s =>
                    s.TenantId == tenantId &&
                    (s.Section.HomeroomTeacherUserId == userId ||
                     db.TeacherAssignments.Any(ta =>
                         ta.SectionId == s.SectionId &&
                         ta.Teacher.UserId == userId)))
                .OrderBy(s => s.FirstNameEn)
                .ThenBy(s => s.LastNameEn)
                .Select(s => new LinkedStudentDto(
                    s.Id,
                    s.FirstNameEn + " " + s.LastNameEn,
                    s.SchoolClass.NameEn + " - " + s.Section.NameEn,
                    db.Conversations.Any(c =>
                        c.TenantId == tenantId &&
                        c.StudentId == s.Id &&
                        c.Participants.Any(p => p.UserId == userId))))
                .ToListAsync(ct);
        }

        return await db.ParentStudents
            .AsNoTracking()
            .Where(ps => ps.ParentId == userId && ps.Student.TenantId == tenantId)
            .OrderBy(ps => ps.Student.FirstNameEn)
            .ThenBy(ps => ps.Student.LastNameEn)
            .Select(ps => new LinkedStudentDto(
                ps.StudentId,
                ps.Student.FirstNameEn + " " + ps.Student.LastNameEn,
                ps.Student.SchoolClass.NameEn + " - " + ps.Student.Section.NameEn,
                db.Conversations.Any(c =>
                    c.TenantId == tenantId &&
                    c.StudentId == ps.StudentId &&
                    c.Participants.Any(p => p.UserId == userId))))
            .ToListAsync(ct);
    }

    public Task<Conversation?> GetConversationAsync(Guid tenantId, Guid conversationId, CancellationToken ct) =>
        db.Conversations
            .Include(c => c.Student)
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == conversationId, ct);

    public async Task<Conversation> GetOrCreateConversationAsync(
        Guid tenantId, Guid studentId, Guid parentUserId, Guid teacherUserId, CancellationToken ct)
    {
        var existing = await db.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(
                c => c.TenantId == tenantId && c.StudentId == studentId &&
                     c.Participants.Any(p => p.UserId == parentUserId) &&
                     c.Participants.Any(p => p.UserId == teacherUserId),
                ct);

        if (existing is not null) return existing;

        var student = await db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == studentId, ct)
            ?? throw new InvalidOperationException("Student not found");

        var conversation = new Conversation
        {
            TenantId = tenantId,
            StudentId = studentId,
            Subject = $"{student.FirstNameEn} {student.LastNameEn} — Parent-Teacher",
            Status = "Active"
        };

        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(ct);

        db.ConversationParticipants.AddRange(
            new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = parentUserId,
                JoinedAt = DateTime.UtcNow
            },
            new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = teacherUserId,
                JoinedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync(ct);
        return conversation;
    }

    public async Task<MessageListResult> GetMessagesAsync(
        Guid tenantId,
        Guid conversationId,
        Guid viewerUserId,
        bool viewerIsParent,
        bool viewerIsAdmin,
        string? cursor,
        int pageSize,
        string? search,
        CancellationToken ct)
    {
        DateTime? cursorSentAt = null;
        Guid? cursorId = null;
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            var parts = cursor.Split('|');
            if (parts.Length == 2 && DateTime.TryParse(parts[0], out var sentAt))
            {
                cursorSentAt = sentAt.ToUniversalTime();
                cursorId = Guid.Parse(parts[1]);
            }
        }

        var query = db.Messages
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.ConversationId == conversationId);

        if (!viewerIsAdmin)
        {
            query = query.Where(m => m.ModerationStatus == ModerationStatus.Approved);
        }

        if (cursorSentAt.HasValue && cursorId.HasValue)
        {
            query = query.Where(m =>
                m.SentAt < cursorSentAt.Value ||
                (m.SentAt == cursorSentAt.Value && m.Id.CompareTo(cursorId.Value) < 0));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(m => m.Content.ToLower().Contains(term));
        }

        var messages = await query
            .OrderByDescending(m => m.SentAt)
            .ThenByDescending(m => m.Id)
            .Take(pageSize + 1)
            .Select(m => new
            {
                m.Id,
                m.ConversationId,
                m.SenderId,
                SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
                m.Content,
                m.MessageType,
                m.ModerationStatus,
                m.SentAt,
                RejectionReason = m.ApprovalRequest != null && m.ModerationStatus == ModerationStatus.Rejected
                    ? m.ApprovalRequest.ReviewNotes
                    : null,
                Attachments = m.Attachments.Select(a => new MessageAttachmentDto(
                    a.Id, a.FileName, a.MimeType, a.SizeBytes,
                    $"/v1/messaging/attachments/{a.Id}/download")).ToList(),
                Receipts = m.Receipts.Where(r => r.RecipientUserId != m.SenderId).ToList()
            })
            .ToListAsync(ct);

        var hasMore = messages.Count > pageSize;
        var page = hasMore ? messages.Take(pageSize).ToList() : messages;

        var items = page.Select(m =>
        {
            var delivery = ResolveDeliveryStatus(m.Receipts, m.SenderId == viewerUserId);
            return new MessageDto(
                m.Id,
                m.ConversationId,
                m.SenderId,
                m.SenderName,
                m.Content,
                m.MessageType.ToString(),
                m.ModerationStatus.ToString(),
                delivery,
                m.SentAt,
                m.Attachments,
                m.SenderId == viewerUserId,
                m.RejectionReason);
        }).Reverse().ToList();

        string? nextCursor = null;
        if (hasMore && page.Count > 0)
        {
            var last = page[^1];
            nextCursor = $"{last.SentAt:O}|{last.Id}";
        }

        return new MessageListResult(items, nextCursor, hasMore);
    }

    public Task<Message?> GetMessageAsync(Guid tenantId, Guid messageId, CancellationToken ct) =>
        db.Messages
            .Include(m => m.Attachments)
            .Include(m => m.Sender)
            .Include(m => m.ApprovalRequest)
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == messageId, ct);

    public async Task<Message> AddMessageAsync(Message message, CancellationToken ct)
    {
        await db.Messages.AddAsync(message, ct);
        return message;
    }

    public async Task AddReceiptsAsync(IEnumerable<MessageReceipt> receipts, CancellationToken ct) =>
        await db.MessageReceipts.AddRangeAsync(receipts, ct);

    public async Task MarkDeliveredAsync(Guid conversationId, Guid recipientUserId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var receipts = await db.MessageReceipts
            .Where(r =>
                r.RecipientUserId == recipientUserId &&
                r.Message.ConversationId == conversationId &&
                r.Status == MessageDeliveryStatus.Sent)
            .ToListAsync(ct);

        foreach (var receipt in receipts)
        {
            receipt.Status = MessageDeliveryStatus.Delivered;
            receipt.DeliveredAt = now;
        }
    }

    public async Task MarkReadAsync(Guid conversationId, Guid recipientUserId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var receipts = await db.MessageReceipts
            .Where(r =>
                r.RecipientUserId == recipientUserId &&
                r.Message.ConversationId == conversationId &&
                r.Status != MessageDeliveryStatus.Read)
            .ToListAsync(ct);

        foreach (var receipt in receipts)
        {
            receipt.Status = MessageDeliveryStatus.Read;
            receipt.ReadAt = now;
            if (receipt.DeliveredAt is null) receipt.DeliveredAt = now;
        }
    }

    public async Task<IReadOnlyList<PendingModerationDto>> GetPendingApprovalsAsync(
        Guid tenantId,
        ModerationFilter filter,
        CancellationToken ct)
    {
        var query = db.MessageApprovalRequests
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.Status == ModerationStatus.Pending);

        query = ApplyModerationFilters(query, filter, pendingOnly: true);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new PendingModerationDto(
                r.Id,
                r.MessageId,
                r.Message.ConversationId,
                r.Message.Conversation.StudentId,
                r.SubmittedByUserId,
                r.Message.Conversation.Participants
                    .Where(p => p.User.RoleAssignments.Any(x => x.Role.Name == UserRole.Teacher))
                    .Select(p => p.UserId)
                    .First(),
                r.Message.Conversation.Student.FirstNameEn + " " + r.Message.Conversation.Student.LastNameEn,
                r.SubmittedByUser.FirstName + " " + r.SubmittedByUser.LastName,
                r.Message.Conversation.Participants
                    .Where(p => p.User.RoleAssignments.Any(x => x.Role.Name == UserRole.Teacher))
                    .Select(p => p.User.FirstName + " " + p.User.LastName)
                    .FirstOrDefault() ?? "",
                r.Message.Content,
                r.Message.MessageType.ToString(),
                r.Message.SentAt,
                r.Message.Attachments.Select(a => new MessageAttachmentDto(
                    a.Id, a.FileName, a.MimeType, a.SizeBytes,
                    $"/v1/messaging/attachments/{a.Id}/download")).ToList()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ModerationHistoryDto>> GetModerationHistoryAsync(
        Guid tenantId,
        ModerationFilter filter,
        int limit,
        CancellationToken ct)
    {
        var query = db.MessageModerations
            .AsNoTracking()
            .Where(m => m.Message.TenantId == tenantId);

        if (filter.StudentId.HasValue)
            query = query.Where(m => m.Message.Conversation.StudentId == filter.StudentId.Value);
        if (filter.ParentUserId.HasValue)
            query = query.Where(m => m.Message.SenderId == filter.ParentUserId.Value);
        if (filter.TeacherUserId.HasValue)
            query = query.Where(m =>
                m.Message.Conversation.Participants.Any(p =>
                    p.UserId == filter.TeacherUserId.Value));
        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<ModerationAction>(filter.Status, true, out var action))
            query = query.Where(m => m.Action == action);
        if (filter.FromDate.HasValue)
            query = query.Where(m => m.ReviewedAt >= filter.FromDate.Value.ToUniversalTime());
        if (filter.ToDate.HasValue)
            query = query.Where(m => m.ReviewedAt <= filter.ToDate.Value.ToUniversalTime());

        return await query
            .OrderByDescending(m => m.ReviewedAt)
            .Take(limit)
            .Select(m => new ModerationHistoryDto(
                m.MessageId,
                m.Message.Conversation.Student.FirstNameEn + " " + m.Message.Conversation.Student.LastNameEn,
                m.Message.Sender.FirstName + " " + m.Message.Sender.LastName,
                m.Action.ToString(),
                m.Notes,
                m.Reviewer.FirstName + " " + m.Reviewer.LastName,
                m.ReviewedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetAdminUserIdsAsync(Guid tenantId, CancellationToken ct) =>
        await db.UserRoleAssignments
            .AsNoTracking()
            .Where(ura =>
                ura.User.TenantId == tenantId &&
                (ura.Role.Name == UserRole.SchoolAdmin || ura.Role.Name == UserRole.SuperAdmin))
            .Select(ura => ura.UserId)
            .Distinct()
            .ToListAsync(ct);

    private static IQueryable<MessageApprovalRequest> ApplyModerationFilters(
        IQueryable<MessageApprovalRequest> query,
        ModerationFilter filter,
        bool pendingOnly)
    {
        if (filter.StudentId.HasValue)
            query = query.Where(r => r.Message.Conversation.StudentId == filter.StudentId.Value);
        if (filter.ParentUserId.HasValue)
            query = query.Where(r => r.SubmittedByUserId == filter.ParentUserId.Value);
        if (filter.TeacherUserId.HasValue)
            query = query.Where(r =>
                r.Message.Conversation.Participants.Any(p =>
                    p.UserId == filter.TeacherUserId.Value));
        if (filter.FromDate.HasValue)
            query = query.Where(r => r.Message.SentAt >= filter.FromDate.Value.ToUniversalTime());
        if (filter.ToDate.HasValue)
            query = query.Where(r => r.Message.SentAt <= filter.ToDate.Value.ToUniversalTime());
        if (!pendingOnly &&
            !string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<ModerationStatus>(filter.Status, true, out var status))
            query = query.Where(r => r.Status == status);

        return query;
    }

    public Task<MessageApprovalRequest?> GetApprovalRequestAsync(
        Guid tenantId, Guid messageId, CancellationToken ct) =>
        db.MessageApprovalRequests.FirstOrDefaultAsync(
            r => r.TenantId == tenantId && r.MessageId == messageId, ct);

    public async Task ApproveMessageAsync(
        Guid tenantId, Guid messageId, Guid adminUserId, string? notes, CancellationToken ct)
    {
        var message = await db.Messages
            .Include(m => m.ApprovalRequest)
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == messageId, ct)
            ?? throw new KeyNotFoundException("Message not found");

        message.ModerationStatus = ModerationStatus.Approved;
        foreach (var attachment in message.Attachments)
            attachment.ModerationStatus = ModerationStatus.Approved;

        if (message.ApprovalRequest is not null)
        {
            message.ApprovalRequest.Status = ModerationStatus.Approved;
            message.ApprovalRequest.ReviewedByAdminId = adminUserId;
            message.ApprovalRequest.ReviewedAt = DateTime.UtcNow;
            message.ApprovalRequest.ReviewNotes = notes;
        }

        db.MessageModerations.Add(new MessageModeration
        {
            MessageId = messageId,
            ReviewerId = adminUserId,
            Action = ModerationAction.Approve,
            Notes = notes,
            ReviewedAt = DateTime.UtcNow
        });

        await AddNotificationAsync(new Notification
        {
            TenantId = tenantId,
            UserId = message.SenderId,
            Type = NotificationType.MessageApproved,
            Title = "Message approved",
            Body = "Your message was approved and is now visible to the teacher.",
            ReferenceId = messageId,
            IsRead = false
        }, ct);
    }

    public async Task RejectMessageAsync(
        Guid tenantId, Guid messageId, Guid adminUserId, string? notes, CancellationToken ct)
    {
        var message = await db.Messages
            .Include(m => m.ApprovalRequest)
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == messageId, ct)
            ?? throw new KeyNotFoundException("Message not found");

        message.ModerationStatus = ModerationStatus.Rejected;
        foreach (var attachment in message.Attachments)
            attachment.ModerationStatus = ModerationStatus.Rejected;

        if (message.ApprovalRequest is not null)
        {
            message.ApprovalRequest.Status = ModerationStatus.Rejected;
            message.ApprovalRequest.ReviewedByAdminId = adminUserId;
            message.ApprovalRequest.ReviewedAt = DateTime.UtcNow;
            message.ApprovalRequest.ReviewNotes = notes;
        }

        db.MessageModerations.Add(new MessageModeration
        {
            MessageId = messageId,
            ReviewerId = adminUserId,
            Action = ModerationAction.Reject,
            Notes = notes,
            ReviewedAt = DateTime.UtcNow
        });

        await AddNotificationAsync(new Notification
        {
            TenantId = tenantId,
            UserId = message.SenderId,
            Type = NotificationType.MessageRejected,
            Title = "Message rejected",
            Body = notes ?? "Your message was rejected by an administrator.",
            ReferenceId = messageId,
            IsRead = false
        }, ct);
    }

    public async Task AddApprovalRequestAsync(MessageApprovalRequest request, CancellationToken ct) =>
        await db.MessageApprovalRequests.AddAsync(request, ct);

    public async Task AddAttachmentAsync(MessageAttachment attachment, CancellationToken ct) =>
        await db.MessageAttachments.AddAsync(attachment, ct);

    public Task<MessageAttachment?> GetAttachmentAsync(Guid tenantId, Guid attachmentId, CancellationToken ct) =>
        db.MessageAttachments
            .Include(a => a.Message)
            .FirstOrDefaultAsync(
                a => a.Id == attachmentId && a.Message.TenantId == tenantId, ct);

    public async Task AddNotificationAsync(Notification notification, CancellationToken ct) =>
        await db.Notifications.AddAsync(notification, ct);

    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(
        Guid tenantId, Guid userId, CancellationToken ct) =>
        await db.Notifications
            .AsNoTracking()
            .Where(n => n.TenantId == tenantId && n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(
                n.Id, n.Type.ToString(), n.Title, n.Body, n.ReferenceId, n.IsRead, n.CreatedAt))
            .ToListAsync(ct);

    public Task MarkNotificationReadAsync(
        Guid tenantId, Guid userId, Guid notificationId, CancellationToken ct) =>
        db.Notifications
            .Where(n => n.TenantId == tenantId && n.UserId == userId && n.Id == notificationId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);

    public Task<User?> GetUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private static string ResolveDeliveryStatus(
        List<MessageReceipt> receipts,
        bool isOwnMessage)
    {
        if (!isOwnMessage || receipts.Count == 0) return MessageDeliveryStatus.Sent.ToString();
        if (receipts.All(r => r.Status == MessageDeliveryStatus.Read))
            return MessageDeliveryStatus.Read.ToString();
        if (receipts.Any(r => r.Status >= MessageDeliveryStatus.Delivered))
            return MessageDeliveryStatus.Delivered.ToString();
        return MessageDeliveryStatus.Sent.ToString();
    }
}
