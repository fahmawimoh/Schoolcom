namespace SchoolCom.Application.Features.Rooms;

public record RoomDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? Code,
    int? Capacity,
    bool IsActive,
    DateTime CreatedAt);

public record RoomListResult(
    IReadOnlyList<RoomDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record CreateRoomRequest(
    string NameEn,
    string NameAr,
    string? Code,
    int? Capacity,
    bool IsActive = true);

public record UpdateRoomRequest(
    string NameEn,
    string NameAr,
    string? Code,
    int? Capacity,
    bool IsActive);
