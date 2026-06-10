namespace SchoolCom.Application.Features.Sections;

public record SectionDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool IsActive,
    int LinkedClassCount,
    DateTime CreatedAt);

public record SectionListResult(
    IReadOnlyList<SectionDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record ClassSectionOptionDto(
    Guid ClassSectionId,
    Guid SectionId,
    string SectionNameEn,
    string SectionNameAr);

public record CreateSectionRequest(
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
