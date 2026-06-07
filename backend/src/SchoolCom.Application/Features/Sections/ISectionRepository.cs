using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Sections;

public interface ISectionRepository
{
    Task<SectionListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        Guid? schoolClassId,
        string? search,
        bool? isActive,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct);

    Task<Section?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<Section?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<IReadOnlyList<SchoolClassSectionOptionDto>> GetSectionsBySchoolClassAsync(
        Guid tenantId,
        Guid schoolClassId,
        CancellationToken ct);

    Task<bool> SchoolClassExistsAsync(Guid tenantId, Guid schoolClassId, CancellationToken ct);

    Task<bool> ClassSectionExistsAsync(
        Guid tenantId,
        Guid schoolClassId,
        string nameEn,
        Guid? excludeSectionId,
        CancellationToken ct);

    Task<bool> HasLinkedStudentsTeachersOrTimetableAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task AddAsync(Section entity, CancellationToken ct);

    Task DeleteAsync(Section entity, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
