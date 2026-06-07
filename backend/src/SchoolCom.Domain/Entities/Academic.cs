using SchoolCom.Domain.Common;

namespace SchoolCom.Domain.Entities;

public class SchoolClass : TenantEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Section> Sections { get; set; } = [];
    public ICollection<Student> Students { get; set; } = [];
    public ICollection<TeacherAssignment> TeacherAssignments { get; set; } = [];
    public ICollection<TimetableEntry> TimetableEntries { get; set; } = [];
}

public class Section : TenantEntity
{
    public Guid SchoolClassId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? HomeroomTeacherUserId { get; set; }

    public SchoolClass SchoolClass { get; set; } = null!;
    public User? HomeroomTeacherUser { get; set; }
    public ICollection<Student> Students { get; set; } = [];
    public ICollection<TeacherAssignment> TeacherAssignments { get; set; } = [];
    public ICollection<TimetableEntry> TimetableEntries { get; set; } = [];
}

public class Subject : TenantEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TeacherAssignment> TeacherAssignments { get; set; } = [];
    public ICollection<TimetableEntry> TimetableEntries { get; set; } = [];
}

public class Room : TenantEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? Capacity { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TimetableEntry> TimetableEntries { get; set; } = [];
}

public class Teacher : TenantEntity
{
    public string FirstNameEn { get; set; } = string.Empty;
    public string FirstNameAr { get; set; } = string.Empty;
    public string LastNameEn { get; set; } = string.Empty;
    public string LastNameAr { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string SubjectEn { get; set; } = string.Empty;
    public string SubjectAr { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid? UserId { get; set; }

    public User? User { get; set; }
    public ICollection<TeacherAssignment> Assignments { get; set; } = [];
    public ICollection<TimetableEntry> TimetableEntries { get; set; } = [];
}

/// <summary>
/// When SectionId is null the teacher is assigned to the entire school class.
/// </summary>
public class TeacherAssignment : BaseEntity
{
    public Guid TeacherId { get; set; }
    public Guid SchoolClassId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid SubjectId { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public SchoolClass SchoolClass { get; set; } = null!;
    public Section? Section { get; set; }
    public Subject Subject { get; set; } = null!;
}

public class TimetableEntry : TenantEntity
{
    public Guid SchoolClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid RoomId { get; set; }
    public int DayOfWeek { get; set; }
    public int PeriodNumber { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public SchoolClass SchoolClass { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Room Room { get; set; } = null!;
}
