namespace SchoolCom.Application.Features.Sections;

public record SectionDto(
    Guid Id,
    Guid SchoolClassId,
    string SchoolClassNameEn,
    string SchoolClassNameAr,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool IsActive,
    int StudentCount,
    DateTime CreatedAt);

public record SectionListResult(
    IReadOnlyList<SectionDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record SchoolClassSectionOptionDto(
    Guid SectionId,
    string SectionNameEn,
    string SectionNameAr);

public record CreateSectionRequest(
    Guid SchoolClassId,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool IsActive = true);

public record UpdateSectionRequest(
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool IsActive);
