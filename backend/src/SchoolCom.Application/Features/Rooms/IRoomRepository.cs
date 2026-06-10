using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Rooms;

public interface IRoomRepository
{
    Task<RoomListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct);

    Task<Room?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<Room?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<bool> HasLinkedTimetableEntriesAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<bool> CodeExistsAsync(
        Guid tenantId,
        string? code,
        Guid? excludeRoomId,
        CancellationToken ct);

    Task<string> GenerateUniqueRoomCodeAsync(Guid tenantId, CancellationToken ct);

    Task<RoomAssignment?> GetAssignmentByRoomIdAsync(Guid tenantId, Guid roomId, CancellationToken ct);

    Task AddAsync(Room entity, CancellationToken ct);

    Task DeleteAsync(Room entity, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
