namespace SchoolCom.Application.Features.Rooms;

public record RoomDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? Code,
    int? Capacity,
    bool IsActive,
    Guid? SchoolClassId,
    Guid? SectionId,
    Guid? ClassSectionId,
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
    int? Capacity,
    bool IsActive = true,
    Guid SchoolClassId = default,
    Guid SectionId = default);

public record UpdateRoomRequest(
    string NameEn,
    string NameAr,
    int? Capacity,
    bool IsActive,
    Guid SchoolClassId,
    Guid SectionId);
