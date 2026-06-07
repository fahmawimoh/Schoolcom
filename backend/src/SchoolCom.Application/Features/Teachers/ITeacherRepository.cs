using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Teachers;

public interface ITeacherRepository
{
    Task<TeacherListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        string? subject,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct);

    Task<Teacher?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<Teacher?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<bool> EmployeeNumberExistsAsync(
        Guid tenantId,
        string employeeNumber,
        Guid? excludeId,
        CancellationToken ct);

    Task<bool> UserBelongsToTenantAsync(Guid tenantId, Guid userId, CancellationToken ct);

    Task<bool> SchoolClassExistsAsync(Guid tenantId, Guid schoolClassId, CancellationToken ct);

    Task<bool> SectionBelongsToSchoolClassAsync(
        Guid tenantId,
        Guid schoolClassId,
        Guid sectionId,
        CancellationToken ct);

    Task<bool> SubjectExistsAsync(Guid tenantId, Guid subjectId, CancellationToken ct);

    Task ReplaceAssignmentsAsync(
        Guid tenantId,
        Guid teacherId,
        IReadOnlyList<TeacherAssignmentInput> assignments,
        CancellationToken ct);

    Task AddAsync(Teacher entity, CancellationToken ct);

    Task DeleteAsync(Teacher entity, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
