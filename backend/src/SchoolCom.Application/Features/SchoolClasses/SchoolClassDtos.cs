namespace SchoolCom.Application.Features.SchoolClasses;

public record SchoolClassDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    int SortOrder,
    bool IsActive,
    int SectionCount,
    int StudentCount,
    DateTime CreatedAt);

public record SchoolClassListResult(
    IReadOnlyList<SchoolClassDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record CreateSchoolClassRequest(
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    int SortOrder,
    bool IsActive = true);

public record UpdateSchoolClassRequest(
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    int SortOrder,
    bool IsActive);
