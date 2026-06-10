using SchoolCom.Application.Features.Sections;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.ClassSections;

public interface IClassSectionRepository
{
    Task<ClassSection?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<ClassSection?> GetByClassAndSectionAsync(
        Guid tenantId,
        Guid schoolClassId,
        Guid sectionId,
        CancellationToken ct);

    Task<bool> LinkExistsAsync(
        Guid tenantId,
        Guid schoolClassId,
        Guid sectionId,
        CancellationToken ct);

    Task<IReadOnlyList<ClassSectionOptionDto>> GetBySchoolClassAsync(
        Guid tenantId,
        Guid schoolClassId,
        CancellationToken ct);

    Task<IReadOnlyList<Guid>> GetLinkedSectionIdsAsync(
        Guid tenantId,
        Guid schoolClassId,
        CancellationToken ct);

    Task SyncLinksAsync(
        Guid tenantId,
        Guid schoolClassId,
        IReadOnlyList<Guid> sectionIds,
        CancellationToken ct);

    Task<bool> AllSectionsExistAsync(
        Guid tenantId,
        IReadOnlyList<Guid> sectionIds,
        CancellationToken ct);

    Task<int> CountBySchoolClassAsync(Guid tenantId, Guid schoolClassId, CancellationToken ct);

    Task RemoveAllBySchoolClassAsync(Guid tenantId, Guid schoolClassId, CancellationToken ct);
}
