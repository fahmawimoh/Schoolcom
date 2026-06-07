using SchoolCom.Domain.Common;

namespace SchoolCom.Domain.Entities;

public class Student : TenantEntity
{
    public string StudentNumber { get; set; } = string.Empty;
    public string FirstNameEn { get; set; } = string.Empty;
    public string FirstNameAr { get; set; } = string.Empty;
    public string LastNameEn { get; set; } = string.Empty;
    public string LastNameAr { get; set; } = string.Empty;
    public Guid SchoolClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string ParentPhone { get; set; } = string.Empty;
    public string? ParentEmail { get; set; }
    public bool IsActive { get; set; } = true;

    public SchoolClass SchoolClass { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public SchoolBranch? Branch { get; set; }
    public User? Parent { get; set; }
    public ICollection<ParentStudent> ParentStudents { get; set; } = [];
}

public class ParentStudent : IAuditableTimestamps
{
    public Guid ParentId { get; set; }
    public User Parent { get; set; } = null!;
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public string Relationship { get; set; } = "Parent";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SchoolBranch : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
}
