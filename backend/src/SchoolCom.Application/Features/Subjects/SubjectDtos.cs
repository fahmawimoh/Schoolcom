namespace SchoolCom.Application.Features.Subjects;

public record SubjectDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? Code,
    bool IsActive,
    DateTime CreatedAt);

public record SubjectListResult(
    IReadOnlyList<SubjectDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record CreateSubjectRequest(
    string NameEn,
    string NameAr,
    string? Code,
    bool IsActive = true);

public record UpdateSubjectRequest(
    string NameEn,
    string NameAr,
    string? Code,
    bool IsActive);
