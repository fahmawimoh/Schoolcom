namespace SchoolCom.Application.Features.RoomAssignments;

public record RoomAssignmentDto(
    Guid Id,
    Guid ClassSectionId,
    Guid SchoolClassId,
    string SchoolClassNameEn,
    string SchoolClassNameAr,
    Guid SectionId,
    string SectionNameEn,
    string SectionNameAr,
    Guid RoomId,
    string RoomNameEn,
    string RoomNameAr,
    string? RoomCode,
    DateTime CreatedAt);

public record RoomAssignmentListResult(
    IReadOnlyList<RoomAssignmentDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record CreateRoomAssignmentRequest(
    Guid ClassSectionId,
    Guid RoomId);

public record UpdateRoomAssignmentRequest(
    Guid ClassSectionId,
    Guid RoomId);

public record ClassSectionLookupDto(
    Guid ClassSectionId,
    Guid SchoolClassId,
    string SchoolClassNameEn,
    string SchoolClassNameAr,
    Guid SectionId,
    string SectionNameEn,
    string SectionNameAr);
