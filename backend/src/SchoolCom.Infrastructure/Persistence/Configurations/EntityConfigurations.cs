using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolCom.Domain.Entities;
using SchoolCom.Domain.Enums;

namespace SchoolCom.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Phone).HasMaxLength(32);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PreferredLanguage).IsRequired().HasMaxLength(5);
        builder.Property(u => u.MfaSecret).HasMaxLength(256);

        builder.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
        builder.HasIndex(u => new { u.TenantId, u.Phone }).IsUnique();
        builder.HasIndex(u => u.TenantId);
        builder.HasIndex(u => u.Phone);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.Name).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.DisplayName).IsRequired().HasMaxLength(100);
        builder.HasIndex(r => r.Name).IsUnique();
    }
}

public class UserRoleAssignmentConfiguration : IEntityTypeConfiguration<UserRoleAssignment>
{
    public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
    {
        builder.HasKey(x => new { x.UserId, x.RoleId });
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasOne(x => x.User)
            .WithMany(u => u.RoleAssignments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(r => r.UserAssignments)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ParentStudentConfiguration : IEntityTypeConfiguration<ParentStudent>
{
    public void Configure(EntityTypeBuilder<ParentStudent> builder)
    {
        builder.HasKey(x => new { x.ParentId, x.StudentId });
        builder.Property(x => x.Relationship).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Parent)
            .WithMany(u => u.ParentStudents)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Student)
            .WithMany(s => s.ParentStudents)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(m => m.Content).IsRequired().HasMaxLength(4000);
        builder.Property(m => m.MessageType).HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.ModerationStatus).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(m => new { m.TenantId, m.ModerationStatus });
        builder.HasIndex(m => new { m.ConversationId, m.SentAt });
        builder.HasIndex(m => m.SenderId);
    }
}

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(120);
        builder.Property(t => t.CustomDomain).HasMaxLength(255);
        builder.Property(t => t.BrandingJson).HasColumnType("jsonb");

        builder.HasIndex(t => t.Slug).IsUnique();
    }
}

public class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> builder)
    {
        builder.HasIndex(x => x.TenantId).IsUnique();
        builder.Property(x => x.PreferredLanguage).IsRequired().HasMaxLength(5);
        builder.HasOne(x => x.Tenant)
            .WithOne(x => x.Settings)
            .HasForeignKey<TenantSettings>(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(512);
        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(512);
        builder.HasIndex(x => new { x.UserId, x.TokenHash }).IsUnique();
        builder.HasIndex(x => x.ExpiresAt);
    }
}

public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.Property(x => x.Identifier).IsRequired().HasMaxLength(256);
        builder.Property(x => x.CodeHash).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(16);
        builder.HasIndex(x => new { x.Identifier, x.Channel, x.ExpiresAt });
    }
}

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.Property(x => x.StudentNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.FirstNameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FirstNameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastNameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastNameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ParentName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ParentPhone).IsRequired().HasMaxLength(32);
        builder.Property(x => x.ParentEmail).HasMaxLength(256);
        builder.HasIndex(x => new { x.TenantId, x.StudentNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.SchoolClassId });
        builder.HasIndex(x => new { x.TenantId, x.SectionId });
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
        builder.HasOne(x => x.SchoolClass)
            .WithMany(x => x.Students)
            .HasForeignKey(x => x.SchoolClassId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Section)
            .WithMany(x => x.Students)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Parent)
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class SchoolBranchConfiguration : IEntityTypeConfiguration<SchoolBranch>
{
    public void Configure(EntityTypeBuilder<SchoolBranch> builder)
    {
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
    }
}

public class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.ToTable("school_classes");
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.DescriptionEn).HasMaxLength(500);
        builder.Property(x => x.DescriptionAr).HasMaxLength(500);
        builder.HasIndex(x => new { x.TenantId, x.SortOrder });
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
    }
}

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("sections");
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.DescriptionEn).HasMaxLength(500);
        builder.Property(x => x.DescriptionAr).HasMaxLength(500);
        builder.HasIndex(x => new { x.TenantId, x.SchoolClassId, x.NameEn })
            .IsUnique()
            .HasDatabaseName("UQ_class_sections_school_class_id_section_name");
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
        builder.HasIndex(x => x.SchoolClassId);
        builder.HasOne(x => x.SchoolClass)
            .WithMany(x => x.Sections)
            .HasForeignKey(x => x.SchoolClassId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.HomeroomTeacherUser)
            .WithMany()
            .HasForeignKey(x => x.HomeroomTeacherUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).HasMaxLength(32);
        builder.HasIndex(x => new { x.TenantId, x.Code });
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
    }
}

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms");
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).HasMaxLength(32);
        builder.HasIndex(x => new { x.TenantId, x.Code });
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
    }
}

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("teachers");
        builder.Property(x => x.FirstNameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FirstNameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastNameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastNameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Phone).IsRequired().HasMaxLength(32);
        builder.Property(x => x.EmployeeNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SubjectEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SubjectAr).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Email });
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class TeacherAssignmentConfiguration : IEntityTypeConfiguration<TeacherAssignment>
{
    public void Configure(EntityTypeBuilder<TeacherAssignment> builder)
    {
        builder.ToTable("teacher_assignments");
        builder.HasIndex(x => new { x.TeacherId, x.SchoolClassId, x.SectionId, x.SubjectId }).IsUnique();
        builder.HasIndex(x => x.SchoolClassId);
        builder.HasIndex(x => x.SectionId);
        builder.HasIndex(x => x.SubjectId);
        builder.HasOne(x => x.Teacher)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.SchoolClass)
            .WithMany(x => x.TeacherAssignments)
            .HasForeignKey(x => x.SchoolClassId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Section)
            .WithMany(x => x.TeacherAssignments)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Subject)
            .WithMany(x => x.TeacherAssignments)
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TimetableEntryConfiguration : IEntityTypeConfiguration<TimetableEntry>
{
    public void Configure(EntityTypeBuilder<TimetableEntry> builder)
    {
        builder.ToTable("timetable_entries");
        builder.HasIndex(x => new { x.TenantId, x.SectionId, x.DayOfWeek, x.PeriodNumber }).IsUnique();
        builder.HasIndex(x => x.SchoolClassId);
        builder.HasIndex(x => x.TeacherId);
        builder.HasIndex(x => x.SubjectId);
        builder.HasIndex(x => x.RoomId);
        builder.HasOne(x => x.SchoolClass)
            .WithMany(x => x.TimetableEntries)
            .HasForeignKey(x => x.SchoolClassId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Section)
            .WithMany(x => x.TimetableEntries)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Teacher)
            .WithMany(x => x.TimetableEntries)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Subject)
            .WithMany(x => x.TimetableEntries)
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Room)
            .WithMany(x => x.TimetableEntries)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.Property(x => x.Subject).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(32);
        builder.HasIndex(x => new { x.TenantId, x.StudentId });
    }
}

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.HasKey(x => new { x.ConversationId, x.UserId });
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();
        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(EntityTypeBuilder<MessageAttachment> builder)
    {
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.StorageKey).IsRequired().HasMaxLength(1024);
        builder.Property(x => x.MimeType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ModerationStatus).HasConversion<string>().HasMaxLength(32);
        builder.ToTable(t => t.HasCheckConstraint("CK_MessageAttachment_SizeBytes_Positive", "\"SizeBytes\" > 0"));
        builder.HasIndex(x => x.MessageId);
    }
}

public class MessageModerationConfiguration : IEntityTypeConfiguration<MessageModeration>
{
    public void Configure(EntityTypeBuilder<MessageModeration> builder)
    {
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => new { x.MessageId, x.ReviewedAt });
    }
}

public class MessageReceiptConfiguration : IEntityTypeConfiguration<MessageReceipt>
{
    public void Configure(EntityTypeBuilder<MessageReceipt> builder)
    {
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(x => new { x.MessageId, x.RecipientUserId }).IsUnique();
        builder.HasIndex(x => x.RecipientUserId);
    }
}

public class MessageApprovalRequestConfiguration : IEntityTypeConfiguration<MessageApprovalRequest>
{
    public void Configure(EntityTypeBuilder<MessageApprovalRequest> builder)
    {
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ReviewNotes).HasMaxLength(1000);
        builder.HasIndex(x => x.MessageId).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status });
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Body).IsRequired().HasMaxLength(2000);
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.IsRead });
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}

public class WhatsAppSessionConfiguration : IEntityTypeConfiguration<WhatsAppSession>
{
    public void Configure(EntityTypeBuilder<WhatsAppSession> builder)
    {
        builder.Property(x => x.PhoneNumberId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.AccessTokenEncrypted).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.ConfigJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TenantId, x.AdminUserId }).IsUnique();
    }
}

public class WhatsAppMessageConfiguration : IEntityTypeConfiguration<WhatsAppMessage>
{
    public void Configure(EntityTypeBuilder<WhatsAppMessage> builder)
    {
        builder.Property(x => x.RecipientPhone).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.Direction).IsRequired().HasMaxLength(16);
        builder.Property(x => x.ExternalMessageId).HasMaxLength(128);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(32);
        builder.HasIndex(x => new { x.TenantId, x.SessionId, x.CreatedAt });
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(x => x.Action).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.MetadataJson).HasColumnType("jsonb");
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.HasIndex(x => new { x.TenantId, x.CreatedAt });
    }
}

public class SeedData
{
    public static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DemoTenantSettingsId = Guid.Parse("11111111-1111-1111-1111-111111111112");
    public static readonly Guid AdminUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ParentUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid TeacherUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid DemoBranchId = Guid.Parse("55555555-5555-5555-5555-555555555551");
    public static readonly Guid Grade5Id = Guid.Parse("55555555-5555-5555-5555-555555555552");
    public static readonly Guid SectionAId = Guid.Parse("55555555-5555-5555-5555-555555555553");
    public static readonly Guid DemoClassroomId = Guid.Parse("55555555-5555-5555-5555-555555555554");
    public static readonly Guid Grade4Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid Grade6Id = Guid.Parse("55555555-5555-5555-5555-555555555556");
    public static readonly Guid SectionBId = Guid.Parse("55555555-5555-5555-5555-555555555557");
    public static readonly Guid ClassroomGrade5BId = Guid.Parse("55555555-5555-5555-5555-555555555558");
    public static readonly Guid ClassroomGrade4AId = Guid.Parse("55555555-5555-5555-5555-555555555559");
    public static readonly Guid ClassroomGrade6AId = Guid.Parse("55555555-5555-5555-5555-55555555555a");
    public static readonly Guid SectionCId = Guid.Parse("55555555-5555-5555-5555-55555555555b");
    public static readonly Guid ClassroomGrade5CId = Guid.Parse("55555555-5555-5555-5555-55555555555c");
    public static readonly Guid MathSubjectId = Guid.Parse("99999999-9999-9999-9999-999999999991");
    public static readonly Guid DemoRoomId = Guid.Parse("99999999-9999-9999-9999-999999999992");
    public static readonly Guid ScienceSubjectId = Guid.Parse("99999999-9999-9999-9999-999999999993");
    public static readonly Guid EnglishSubjectId = Guid.Parse("99999999-9999-9999-9999-999999999994");
    public static readonly Guid DemoStudentId = Guid.Parse("66666666-6666-6666-6666-666666666661");
    public static readonly Guid DemoTeacherId = Guid.Parse("88888888-8888-8888-8888-888888888881");
    public static readonly Guid DemoConversationId = Guid.Parse("77777777-7777-7777-7777-777777777771");
    public static readonly Guid DemoMessageId = Guid.Parse("77777777-7777-7777-7777-777777777772");
    public static readonly Guid SuperAdminRoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab");
    public static readonly Guid AdminRoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid ParentRoleId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid TeacherRoleId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static void Seed(ModelBuilder modelBuilder)
    {
        var adminRoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var parentRoleId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var teacherRoleId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        modelBuilder.Entity<Tenant>().HasData(new Tenant
        {
            Id = DemoTenantId,
            Name = "Demo International School",
            Slug = "demo-school",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = adminRoleId, Name = UserRole.SchoolAdmin, DisplayName = "School Admin", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = parentRoleId, Name = UserRole.Parent, DisplayName = "Parent", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = teacherRoleId, Name = UserRole.Teacher, DisplayName = "Teacher", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Password: Admin@123456 (BCrypt)
        const string passwordHash = "$2a$11$K3VqJ8xYmZ5nR7pL2wH9.uQXvN4mK6jF8hG1sT0yU3iO5pA7bC9dE";

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = AdminUserId,
                TenantId = DemoTenantId,
                Email = "admin@demo.school",
                Phone = "+966501234567",
                PasswordHash = passwordHash,
                FirstName = "School",
                LastName = "Admin",
                EmailVerified = true,
                PhoneVerified = true,
                PreferredLanguage = "en",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = ParentUserId,
                TenantId = DemoTenantId,
                Email = "parent@demo.school",
                Phone = "+966509876543",
                PasswordHash = passwordHash,
                FirstName = "Ahmed",
                LastName = "Al-Rashid",
                EmailVerified = true,
                PhoneVerified = true,
                PreferredLanguage = "ar",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = TeacherUserId,
                TenantId = DemoTenantId,
                Email = "teacher@demo.school",
                Phone = "+966507654321",
                PasswordHash = passwordHash,
                FirstName = "Sarah",
                LastName = "Johnson",
                EmailVerified = true,
                PhoneVerified = true,
                PreferredLanguage = "en",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<UserRoleAssignment>().HasData(
            new UserRoleAssignment { UserId = AdminUserId, RoleId = adminRoleId },
            new UserRoleAssignment { UserId = ParentUserId, RoleId = parentRoleId },
            new UserRoleAssignment { UserId = TeacherUserId, RoleId = teacherRoleId }
        );
    }
}
