using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;
using SchoolCom.Domain.Enums;
using SchoolCom.Infrastructure.Persistence.Configurations;

namespace SchoolCom.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, IServiceProvider services)
    {
        var hasher = services.GetRequiredService<IPasswordHasher>();
        var passwordHash = hasher.Hash("Admin@123456");

        await SeedTenantAsync(db);
        await SeedRolesAsync(db);
        await SeedUsersAsync(db, passwordHash);
        await SeedTenantSettingsAsync(db);
        await SeedAcademicStructureAsync(db);
        await ConsolidateLegacySectionsAsync(db);
        await EnsureTeachersSeededAsync(db);
        await EnsureStudentsSeededAsync(db);
        await EnsureTimetableSeededAsync(db);
        await SeedSampleConversationAsync(db);
    }

    private static async Task SeedTenantAsync(ApplicationDbContext db)
    {
        if (await db.Tenants.AnyAsync()) return;

        db.Tenants.Add(new Tenant
        {
            Id = SeedData.DemoTenantId,
            Name = "Demo International School",
            Slug = "demo-school",
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(ApplicationDbContext db)
    {
        if (await db.Roles.AnyAsync()) return;

        db.Roles.AddRange(
            new Role { Id = SeedData.SuperAdminRoleId, Name = UserRole.SuperAdmin, DisplayName = "Super Admin" },
            new Role { Id = SeedData.AdminRoleId, Name = UserRole.SchoolAdmin, DisplayName = "School Admin" },
            new Role { Id = SeedData.ParentRoleId, Name = UserRole.Parent, DisplayName = "Parent" },
            new Role { Id = SeedData.TeacherRoleId, Name = UserRole.Teacher, DisplayName = "Teacher" }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(ApplicationDbContext db, string passwordHash)
    {
        if (await db.Users.AnyAsync()) return;

        var adminRole = await db.Roles.FirstAsync(r => r.Name == UserRole.SchoolAdmin);
        var parentRole = await db.Roles.FirstAsync(r => r.Name == UserRole.Parent);
        var teacherRole = await db.Roles.FirstAsync(r => r.Name == UserRole.Teacher);

        var admin = new User
        {
            Id = SeedData.AdminUserId,
            TenantId = SeedData.DemoTenantId,
            Email = "admin@demo.school",
            Phone = "+966501234567",
            PasswordHash = passwordHash,
            FirstName = "School",
            LastName = "Admin",
            EmailVerified = true,
            PhoneVerified = true,
            PreferredLanguage = "en"
        };

        var parent = new User
        {
            Id = SeedData.ParentUserId,
            TenantId = SeedData.DemoTenantId,
            Email = "parent@demo.school",
            Phone = "+966509876543",
            PasswordHash = passwordHash,
            FirstName = "Ahmed",
            LastName = "Al-Rashid",
            EmailVerified = true,
            PhoneVerified = true,
            PreferredLanguage = "ar"
        };

        var teacher = new User
        {
            Id = SeedData.TeacherUserId,
            TenantId = SeedData.DemoTenantId,
            Email = "teacher@demo.school",
            Phone = "+966507654321",
            PasswordHash = passwordHash,
            FirstName = "Sarah",
            LastName = "Johnson",
            EmailVerified = true,
            PhoneVerified = true,
            PreferredLanguage = "en"
        };

        db.Users.AddRange(admin, parent, teacher);
        await db.SaveChangesAsync();

        db.UserRoleAssignments.AddRange(
            new UserRoleAssignment { UserId = admin.Id, RoleId = adminRole.Id },
            new UserRoleAssignment { UserId = parent.Id, RoleId = parentRole.Id },
            new UserRoleAssignment { UserId = teacher.Id, RoleId = teacherRole.Id }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedTenantSettingsAsync(ApplicationDbContext db)
    {
        if (await db.TenantSettings.AnyAsync()) return;

        db.TenantSettings.Add(new TenantSettings
        {
            Id = SeedData.DemoTenantSettingsId,
            TenantId = SeedData.DemoTenantId,
            RequireTeacherMessageModeration = true,
            EnableWhatsAppIntegration = true,
            PreferredLanguage = "en",
            MaxImageSizeMb = 2,
            MaxVideoSizeMb = 10,
            MaxDocumentSizeMb = 3
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedAcademicStructureAsync(ApplicationDbContext db)
    {
        if (!await db.SchoolBranches.AnyAsync())
        {
            db.SchoolBranches.Add(new SchoolBranch
            {
                Id = SeedData.DemoBranchId,
                TenantId = SeedData.DemoTenantId,
                Name = "Main Campus",
                Address = "123 Education Street, Riyadh"
            });
        }

        if (!await db.SchoolClasses.AnyAsync())
        {
            db.SchoolClasses.AddRange(
                new SchoolClass
                {
                    Id = SeedData.Grade4Id,
                    TenantId = SeedData.DemoTenantId,
                    NameEn = "Fourth Grade",
                    NameAr = "الصف الرابع",
                    SortOrder = 4,
                    IsActive = true
                },
                new SchoolClass
                {
                    Id = SeedData.Grade5Id,
                    TenantId = SeedData.DemoTenantId,
                    NameEn = "Ninth Grade",
                    NameAr = "الصف التاسع",
                    SortOrder = 9,
                    IsActive = true
                },
                new SchoolClass
                {
                    Id = SeedData.Grade6Id,
                    TenantId = SeedData.DemoTenantId,
                    NameEn = "Tenth Grade",
                    NameAr = "الصف العاشر",
                    SortOrder = 10,
                    IsActive = true
                }
            );
        }

        if (!await db.Subjects.AnyAsync())
        {
            db.Subjects.Add(new Subject
            {
                Id = SeedData.MathSubjectId,
                TenantId = SeedData.DemoTenantId,
                NameEn = "Mathematics",
                NameAr = "الرياضيات",
                Code = "MATH",
                IsActive = true
            });
        }

        if (!await db.Rooms.AnyAsync())
        {
            db.Rooms.Add(new Room
            {
                Id = SeedData.DemoRoomId,
                TenantId = SeedData.DemoTenantId,
                NameEn = "Room 101",
                NameAr = "غرفة 101",
                Code = "R101",
                Capacity = 30,
                IsActive = true
            });
        }

        var standaloneSections = new[]
        {
            new Section
            {
                Id = SeedData.SectionAId,
                TenantId = SeedData.DemoTenantId,
                NameEn = "A",
                NameAr = "أ",
                IsActive = true
            },
            new Section
            {
                Id = SeedData.SectionBId,
                TenantId = SeedData.DemoTenantId,
                NameEn = "B",
                NameAr = "ب",
                IsActive = true
            },
            new Section
            {
                Id = SeedData.SectionCId,
                TenantId = SeedData.DemoTenantId,
                NameEn = "C",
                NameAr = "ج",
                IsActive = true
            }
        };

        foreach (var section in standaloneSections)
        {
            if (!await db.Sections.AnyAsync(s =>
                    s.Id == section.Id ||
                    (s.TenantId == section.TenantId && s.NameEn == section.NameEn)))
                db.Sections.Add(section);
        }

        await db.SaveChangesAsync();

        var classSectionLinks = new[]
        {
            (SeedData.Grade5Id, SeedData.SectionAId),
            (SeedData.Grade5Id, SeedData.SectionBId),
            (SeedData.Grade5Id, SeedData.SectionCId),
            (SeedData.Grade4Id, SeedData.SectionAId),
            (SeedData.Grade6Id, SeedData.SectionAId)
        };

        foreach (var (schoolClassId, sectionId) in classSectionLinks)
        {
            var resolvedSectionId = await ResolveSeedSectionIdAsync(db, sectionId);
            if (resolvedSectionId is null) continue;

            if (!await db.ClassSections.AnyAsync(cs =>
                    cs.TenantId == SeedData.DemoTenantId &&
                    cs.SchoolClassId == schoolClassId &&
                    cs.SectionId == resolvedSectionId.Value))
            {
                db.ClassSections.Add(new ClassSection
                {
                    TenantId = SeedData.DemoTenantId,
                    SchoolClassId = schoolClassId,
                    SectionId = resolvedSectionId.Value,
                    IsActive = true
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task<Guid?> ResolveSeedSectionIdAsync(
        ApplicationDbContext db, Guid seedSectionId)
    {
        if (await db.Sections.AnyAsync(s => s.Id == seedSectionId))
            return seedSectionId;

        var shortName = seedSectionId switch
        {
            var id when id == SeedData.SectionAId => "A",
            var id when id == SeedData.SectionBId => "B",
            var id when id == SeedData.SectionCId => "C",
            _ => null
        };

        if (shortName is null) return null;

        return await db.Sections
            .Where(s => s.TenantId == SeedData.DemoTenantId &&
                        (s.NameEn == shortName || s.NameEn == $"Section {shortName}"))
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync();
    }

    private static async Task<Guid?> GetSectionIdAsync(
        ApplicationDbContext db, Guid schoolClassId, params string[] nameCandidates)
    {
        foreach (var nameEn in nameCandidates)
        {
            var id = await db.ClassSections
                .Where(cs => cs.SchoolClassId == schoolClassId && cs.Section.NameEn == nameEn)
                .Select(cs => (Guid?)cs.SectionId)
                .FirstOrDefaultAsync();
            if (id.HasValue) return id;
        }

        return null;
    }

    private static readonly (string Legacy, string Canonical)[] SectionNamePairs =
    [
        ("A", "Section A"),
        ("B", "Section B"),
        ("C", "Section C"),
    ];

    private static string? ToLegacySectionName(string canonical) =>
        canonical switch
        {
            "Section A" => "A",
            "Section B" => "B",
            "Section C" => "C",
            _ => null
        };

    private static Task ConsolidateLegacySectionsAsync(ApplicationDbContext db) =>
        Task.CompletedTask;

    private static async Task ReassignSectionReferencesAsync(
        ApplicationDbContext db, Guid fromSectionId, Guid toSectionId)
    {
        await db.Students
            .Where(s => s.SectionId == fromSectionId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.SectionId, toSectionId));

        await db.TeacherAssignments
            .Where(ta => ta.SectionId == fromSectionId)
            .ExecuteUpdateAsync(ta => ta.SetProperty(x => x.SectionId, toSectionId));

        await db.TimetableEntries
            .Where(te => te.SectionId == fromSectionId)
            .ExecuteUpdateAsync(te => te.SetProperty(x => x.SectionId, toSectionId));
    }

    private static async Task<bool> SectionIsInUseAsync(ApplicationDbContext db, Guid sectionId)
    {
        if (await db.Students.AnyAsync(s => s.SectionId == sectionId)) return true;
        if (await db.TeacherAssignments.AnyAsync(ta => ta.SectionId == sectionId)) return true;
        return await db.TimetableEntries.AnyAsync(te => te.SectionId == sectionId);
    }

    private static async Task<Guid> EnsureSubjectAsync(
        ApplicationDbContext db, string code, string nameEn, string nameAr, Guid fallbackId)
    {
        var existing = await db.Subjects
            .Where(s => s.TenantId == SeedData.DemoTenantId && s.Code == code)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        if (existing != Guid.Empty) return existing;

        var subject = new Subject
        {
            Id = fallbackId,
            TenantId = SeedData.DemoTenantId,
            NameEn = nameEn,
            NameAr = nameAr,
            Code = code,
            IsActive = true
        };
        db.Subjects.Add(subject);
        await db.SaveChangesAsync();
        return subject.Id;
    }

    private static async Task EnsureTeachersSeededAsync(ApplicationDbContext db)
    {
        if (!await db.Teachers.AnyAsync())
        {
            db.Teachers.Add(new Teacher
            {
                Id = SeedData.DemoTeacherId,
                TenantId = SeedData.DemoTenantId,
                FirstNameEn = "Sarah",
                FirstNameAr = "سارة",
                LastNameEn = "Johnson",
                LastNameAr = "جونson",
                Email = "teacher@demo.school",
                Phone = "+966507654321",
                EmployeeNumber = "EMP-001",
                SubjectEn = "Mathematics",
                SubjectAr = "الرياضيات",
                IsActive = true,
                UserId = SeedData.TeacherUserId
            });
            await db.SaveChangesAsync();
        }

        if (await db.TeacherAssignments.AnyAsync()) return;

        var mathSubjectId = await db.Subjects
            .Where(s => s.TenantId == SeedData.DemoTenantId && s.Code == "MATH")
            .Select(s => s.Id)
            .FirstOrDefaultAsync();
        if (mathSubjectId == Guid.Empty)
            mathSubjectId = SeedData.MathSubjectId;

        var grade5SectionA = await GetSectionIdAsync(db, SeedData.Grade5Id, "Section A", "A");
        var grade5SectionB = await GetSectionIdAsync(db, SeedData.Grade5Id, "Section B", "B");
        var grade6SectionA = await GetSectionIdAsync(db, SeedData.Grade6Id, "Section A", "A");

        var assignments = new List<TeacherAssignment>();
        if (grade5SectionA.HasValue)
        {
            assignments.Add(new TeacherAssignment
            {
                TeacherId = SeedData.DemoTeacherId,
                SchoolClassId = SeedData.Grade5Id,
                SectionId = grade5SectionA.Value,
                SubjectId = mathSubjectId
            });
        }
        if (grade5SectionB.HasValue)
        {
            assignments.Add(new TeacherAssignment
            {
                TeacherId = SeedData.DemoTeacherId,
                SchoolClassId = SeedData.Grade5Id,
                SectionId = grade5SectionB.Value,
                SubjectId = mathSubjectId
            });
        }
        if (grade6SectionA.HasValue)
        {
            assignments.Add(new TeacherAssignment
            {
                TeacherId = SeedData.DemoTeacherId,
                SchoolClassId = SeedData.Grade6Id,
                SectionId = grade6SectionA.Value,
                SubjectId = mathSubjectId
            });
        }

        if (assignments.Count > 0)
        {
            db.TeacherAssignments.AddRange(assignments);
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureStudentsSeededAsync(ApplicationDbContext db)
    {
        if (await db.Students.CountAsync() >= 10) return;

        var grade5SectionA = await GetSectionIdAsync(db, SeedData.Grade5Id, "Section A", "A");
        var grade5SectionB = await GetSectionIdAsync(db, SeedData.Grade5Id, "Section B", "B");
        var grade4SectionA = await GetSectionIdAsync(db, SeedData.Grade4Id, "Section A", "A");
        var grade6SectionA = await GetSectionIdAsync(db, SeedData.Grade6Id, "Section A", "A");

        if (!grade5SectionA.HasValue || !grade5SectionB.HasValue ||
            !grade4SectionA.HasValue || !grade6SectionA.HasValue)
            return;

        var existingNumbers = await db.Students
            .Select(s => s.StudentNumber)
            .ToListAsync();
        var existingIds = await db.Students
            .Select(s => s.Id)
            .ToListAsync();

        var samples = new[]
        {
            (Id: SeedData.DemoStudentId, Number: "STU-2026-001", FirstEn: "Omar", FirstAr: "عمر", LastEn: "Al-Rashid", LastAr: "الرashid", SchoolClass: SeedData.Grade5Id, Section: grade5SectionA.Value, Parent: "Ahmed Al-Rashid", Phone: "+966509876543", Email: "parent@demo.school", Active: true),
            (Id: Guid.Parse("66666666-6666-6666-6666-666666666662"), Number: "STU-2026-002", FirstEn: "Fatima", FirstAr: "فاطمة", LastEn: "Al-Saud", LastAr: "آل سعود", SchoolClass: SeedData.Grade5Id, Section: grade5SectionA.Value, Parent: "Khalid Al-Saud", Phone: "+966501111111", Email: "khalid@example.com", Active: true),
            (Id: Guid.Parse("66666666-6666-6666-6666-666666666663"), Number: "STU-2026-003", FirstEn: "Youssef", FirstAr: "يوسف", LastEn: "Hassan", LastAr: "حسن", SchoolClass: SeedData.Grade5Id, Section: grade5SectionB.Value, Parent: "Mona Hassan", Phone: "+966502222222", Email: (string?)null, Active: true),
            (Id: Guid.Parse("66666666-6666-6666-6666-666666666664"), Number: "STU-2026-004", FirstEn: "Layla", FirstAr: "ليلى", LastEn: "Ibrahim", LastAr: "إبراهيم", SchoolClass: SeedData.Grade5Id, Section: grade5SectionB.Value, Parent: "Samir Ibrahim", Phone: "+966503333333", Email: "samir@example.com", Active: true),
            (Id: Guid.Parse("66666666-6666-6666-6666-666666666665"), Number: "STU-2026-005", FirstEn: "Khalid", FirstAr: "خالد", LastEn: "Nasser", LastAr: "ناصر", SchoolClass: SeedData.Grade4Id, Section: grade4SectionA.Value, Parent: "Hana Nasser", Phone: "+966504444444", Email: "hana@example.com", Active: true),
            (Id: Guid.Parse("66666666-6666-6666-6666-666666666666"), Number: "STU-2026-006", FirstEn: "Noura", FirstAr: "نورة", LastEn: "Al-Farsi", LastAr: "الفارsi", SchoolClass: SeedData.Grade4Id, Section: grade4SectionA.Value, Parent: "Faisal Al-Farsi", Phone: "+966505555555", Email: (string?)null, Active: true),
            (Id: Guid.Parse("66666666-6666-6666-6666-666666666667"), Number: "STU-2026-007", FirstEn: "Abdullah", FirstAr: "عبدالله", LastEn: "Qasim", LastAr: "قاسم", SchoolClass: SeedData.Grade6Id, Section: grade6SectionA.Value, Parent: "Reem Qasim", Phone: "+966506666666", Email: "reem@example.com", Active: true),
            (Id: Guid.Parse("66666666-6666-6666-6666-666666666668"), Number: "STU-2026-008", FirstEn: "Mariam", FirstAr: "مريم", LastEn: "Saleh", LastAr: "صaleh", SchoolClass: SeedData.Grade6Id, Section: grade6SectionA.Value, Parent: "Tariq Saleh", Phone: "+966507777777", Email: "tariq@example.com", Active: false),
            (Id: Guid.Parse("66666666-6666-6666-6666-666666666669"), Number: "STU-2026-009", FirstEn: "Hamza", FirstAr: "حمزة", LastEn: "Al-Mutairi", LastAr: "المطيري", SchoolClass: SeedData.Grade5Id, Section: grade5SectionA.Value, Parent: "Nadia Al-Mutairi", Phone: "+966508888888", Email: (string?)null, Active: true),
            (Id: Guid.Parse("66666666-6666-6666-6666-66666666666a"), Number: "STU-2026-010", FirstEn: "Sara", FirstAr: "سارة", LastEn: "Al-Otaibi", LastAr: "العتيبي", SchoolClass: SeedData.Grade5Id, Section: grade5SectionB.Value, Parent: "Waleed Al-Otaibi", Phone: "+966509999999", Email: "waleed@example.com", Active: true)
        };

        foreach (var s in samples)
        {
            if (existingNumbers.Contains(s.Number) || existingIds.Contains(s.Id)) continue;

            db.Students.Add(new Student
            {
                Id = s.Id,
                TenantId = SeedData.DemoTenantId,
                StudentNumber = s.Number,
                FirstNameEn = s.FirstEn,
                FirstNameAr = s.FirstAr,
                LastNameEn = s.LastEn,
                LastNameAr = s.LastAr,
                SchoolClassId = s.SchoolClass,
                SectionId = s.Section,
                BranchId = SeedData.DemoBranchId,
                ParentId = s.Id == SeedData.DemoStudentId ? SeedData.ParentUserId : null,
                ParentName = s.Parent,
                ParentPhone = s.Phone,
                ParentEmail = s.Email,
                IsActive = s.Active
            });
        }

        await db.SaveChangesAsync();

        if (!await db.ParentStudents.AnyAsync(ps => ps.StudentId == SeedData.DemoStudentId))
        {
            db.ParentStudents.Add(new ParentStudent
            {
                ParentId = SeedData.ParentUserId,
                StudentId = SeedData.DemoStudentId,
                Relationship = "Father"
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureTimetableSeededAsync(ApplicationDbContext db)
    {
        if (await db.TimetableEntries.AnyAsync()) return;

        var mathSubjectId = await EnsureSubjectAsync(
            db, "MATH", "Mathematics", "الرياضيات", SeedData.MathSubjectId);
        var scienceSubjectId = await EnsureSubjectAsync(
            db, "SCIENCE", "Science", "العلوم", SeedData.ScienceSubjectId);
        var englishSubjectId = await EnsureSubjectAsync(
            db, "ENGLISH", "English", "اللغة الإنجليزية", SeedData.EnglishSubjectId);

        var roomId = await db.Rooms
            .Where(r => r.TenantId == SeedData.DemoTenantId)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();
        if (roomId == Guid.Empty) roomId = SeedData.DemoRoomId;

        var grade5SectionA = await GetSectionIdAsync(db, SeedData.Grade5Id, "Section A", "A");
        var grade5SectionB = await GetSectionIdAsync(db, SeedData.Grade5Id, "Section B", "B");

        if (!grade5SectionA.HasValue || !grade5SectionB.HasValue) return;

        db.TimetableEntries.AddRange(
            new TimetableEntry
            {
                TenantId = SeedData.DemoTenantId,
                SchoolClassId = SeedData.Grade5Id,
                SectionId = grade5SectionA.Value,
                TeacherId = SeedData.DemoTeacherId,
                SubjectId = mathSubjectId,
                RoomId = roomId,
                DayOfWeek = 1,
                PeriodNumber = 1,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(9, 0)
            },
            new TimetableEntry
            {
                TenantId = SeedData.DemoTenantId,
                SchoolClassId = SeedData.Grade5Id,
                SectionId = grade5SectionA.Value,
                TeacherId = SeedData.DemoTeacherId,
                SubjectId = scienceSubjectId,
                RoomId = roomId,
                DayOfWeek = 1,
                PeriodNumber = 2,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0)
            },
            new TimetableEntry
            {
                TenantId = SeedData.DemoTenantId,
                SchoolClassId = SeedData.Grade5Id,
                SectionId = grade5SectionB.Value,
                TeacherId = SeedData.DemoTeacherId,
                SubjectId = englishSubjectId,
                RoomId = roomId,
                DayOfWeek = 1,
                PeriodNumber = 1,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(9, 0)
            },
            new TimetableEntry
            {
                TenantId = SeedData.DemoTenantId,
                SchoolClassId = SeedData.Grade5Id,
                SectionId = grade5SectionB.Value,
                TeacherId = SeedData.DemoTeacherId,
                SubjectId = mathSubjectId,
                RoomId = roomId,
                DayOfWeek = 1,
                PeriodNumber = 2,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0)
            }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedSampleConversationAsync(ApplicationDbContext db)
    {
        if (await db.Conversations.AnyAsync()) return;

        db.Conversations.Add(new Conversation
        {
            Id = SeedData.DemoConversationId,
            TenantId = SeedData.DemoTenantId,
            StudentId = SeedData.DemoStudentId,
            Subject = "Math homework - Chapter 4",
            Status = "Active"
        });

        await db.SaveChangesAsync();

        db.ConversationParticipants.AddRange(
            new ConversationParticipant
            {
                ConversationId = SeedData.DemoConversationId,
                UserId = SeedData.ParentUserId,
                JoinedAt = DateTime.UtcNow.AddDays(-7)
            },
            new ConversationParticipant
            {
                ConversationId = SeedData.DemoConversationId,
                UserId = SeedData.TeacherUserId,
                JoinedAt = DateTime.UtcNow.AddDays(-7)
            }
        );

        db.Messages.Add(new Message
        {
            Id = SeedData.DemoMessageId,
            TenantId = SeedData.DemoTenantId,
            ConversationId = SeedData.DemoConversationId,
            SenderId = SeedData.TeacherUserId,
            Content = "Please review the practice problems with Omar before Thursday's quiz.",
            MessageType = MessageType.Text,
            ModerationStatus = ModerationStatus.Approved,
            SentAt = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();
    }
}
