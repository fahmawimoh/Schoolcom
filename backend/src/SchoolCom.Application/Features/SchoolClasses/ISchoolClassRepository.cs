using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.SchoolClasses;

public interface ISchoolClassRepository
{
    Task<SchoolClassListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct);

    Task<SchoolClass?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task<SchoolClass?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task<bool> NameExistsAsync(Guid tenantId, string nameEn, Guid? excludeId, CancellationToken ct);
    Task<bool> HasLinkedStudentsOrAssignmentsAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task AddAsync(SchoolClass entity, CancellationToken ct);
    Task DeleteAsync(SchoolClass entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
