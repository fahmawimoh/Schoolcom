namespace SchoolCom.Application.Features.Students;

public record StudentDto(
    Guid Id,
    string StudentNumber,
    string FirstNameEn,
    string FirstNameAr,
    string LastNameEn,
    string LastNameAr,
    string FullName,
    Guid SchoolClassId,
    string SchoolClassNameEn,
    string SchoolClassNameAr,
    Guid SectionId,
    string SectionNameEn,
    string SectionNameAr,
    Guid? ParentId,
    string ParentName,
    string ParentPhone,
    string? ParentEmail,
    string Status,
    DateTime CreatedAt);

public record StudentListResult(
    IReadOnlyList<StudentDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record StudentFiltersDto(
    IReadOnlyList<SchoolClassFilterOptionDto> SchoolClasses,
    IReadOnlyList<SectionFilterOptionDto> Sections,
    IReadOnlyList<ParentFilterOptionDto> Parents);

public record SchoolClassFilterOptionDto(Guid Id, string NameEn, string NameAr);
public record SectionFilterOptionDto(Guid Id, Guid SchoolClassId, string NameEn, string NameAr);
public record ParentFilterOptionDto(Guid Id, string Name);

public record CreateStudentRequest(
    string? StudentNumber,
    string FirstNameEn,
    string FirstNameAr,
    string LastNameEn,
    string LastNameAr,
    Guid SchoolClassId,
    Guid SectionId,
    Guid? ParentId,
    string ParentName,
    string ParentPhone,
    string? ParentEmail,
    bool IsActive = true);

public record UpdateStudentRequest(
    string? StudentNumber,
    string FirstNameEn,
    string FirstNameAr,
    string LastNameEn,
    string LastNameAr,
    Guid SchoolClassId,
    Guid SectionId,
    Guid? ParentId,
    string ParentName,
    string ParentPhone,
    string? ParentEmail,
    bool IsActive);
