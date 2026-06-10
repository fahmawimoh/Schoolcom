using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.RoomAssignments;

public interface IRoomAssignmentRepository
{
    Task<RoomAssignmentListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        Guid? schoolClassId,
        Guid? sectionId,
        Guid? roomId,
        CancellationToken ct);

    Task<RoomAssignment?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<RoomAssignment?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<bool> ClassSectionExistsAsync(Guid tenantId, Guid classSectionId, CancellationToken ct);

    Task<bool> RoomExistsAsync(Guid tenantId, Guid roomId, CancellationToken ct);

    Task<bool> ClassSectionAlreadyAssignedAsync(
        Guid tenantId,
        Guid classSectionId,
        Guid? excludeId,
        CancellationToken ct);

    Task<bool> RoomAlreadyAssignedAsync(
        Guid tenantId,
        Guid roomId,
        Guid? excludeId,
        CancellationToken ct);

    Task<bool> ExistsForClassSectionAndRoomNameAsync(
        Guid tenantId,
        Guid classSectionId,
        string nameEn,
        Guid? excludeRoomId,
        CancellationToken ct);

    Task<RoomAssignment?> GetByRoomIdForUpdateAsync(
        Guid tenantId,
        Guid roomId,
        CancellationToken ct);

    Task<IReadOnlyList<ClassSectionLookupDto>> GetClassSectionOptionsAsync(
        Guid tenantId,
        Guid? schoolClassId,
        CancellationToken ct);

    Task AddAsync(RoomAssignment entity, CancellationToken ct);

    Task DeleteAsync(RoomAssignment entity, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
