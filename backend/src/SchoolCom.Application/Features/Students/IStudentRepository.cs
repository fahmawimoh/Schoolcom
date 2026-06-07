using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Students;

public interface IStudentRepository
{
    Task<StudentListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        Guid? schoolClassId,
        Guid? sectionId,
        string? search,
        string? locale,
        CancellationToken ct);

    Task<StudentFiltersDto> GetFiltersAsync(Guid tenantId, CancellationToken ct);

    Task<Student?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<Student?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<bool> StudentNumberExistsAsync(Guid tenantId, string studentNumber, Guid? excludeId, CancellationToken ct);

    Task<string> GenerateNextStudentNumberAsync(Guid tenantId, CancellationToken ct);

    Task<bool> SectionBelongsToSchoolClassAsync(Guid tenantId, Guid schoolClassId, Guid sectionId, CancellationToken ct);

    Task AddAsync(Student student, CancellationToken ct);

    Task DeleteAsync(Student student, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
