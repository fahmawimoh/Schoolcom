using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.SchoolClasses;

public record SchoolClassSectionItem(
    Guid Id,
    string NameEn,
    string NameAr);

public record SchoolClassDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    int SortOrder,
    bool IsActive,
    int SectionCount,
    IReadOnlyList<SchoolClassSectionItem> Sections,
    int StudentCount,
    DateTime CreatedAt);

public static class SchoolClassMapper
{
    public static SchoolClassDto ToDto(SchoolClass entity) =>
        new(
            entity.Id,
            entity.NameEn,
            entity.NameAr,
            entity.DescriptionEn,
            entity.DescriptionAr,
            entity.SortOrder,
            entity.IsActive,
            entity.ClassSections.Count,
            entity.ClassSections
                .OrderBy(cs => cs.Section.NameEn)
                .Select(cs => new SchoolClassSectionItem(
                    cs.SectionId,
                    cs.Section.NameEn,
                    cs.Section.NameAr))
                .ToList(),
            entity.Students.Count,
            entity.CreatedAt);
}

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
    bool IsActive = true,
    IReadOnlyList<Guid>? SectionIds = null);

public record UpdateSchoolClassRequest(
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<Guid>? SectionIds = null);
