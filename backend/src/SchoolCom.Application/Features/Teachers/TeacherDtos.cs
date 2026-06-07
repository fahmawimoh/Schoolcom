namespace SchoolCom.Application.Features.Teachers;

public record TeacherAssignmentDto(
    Guid Id,
    Guid SchoolClassId,
    Guid? SectionId,
    Guid SubjectId,
    string SchoolClassNameEn,
    string SchoolClassNameAr,
    string? SectionNameEn,
    string? SectionNameAr,
    string SubjectNameEn,
    string SubjectNameAr);

public record TeacherDto(
    Guid Id,
    string FirstNameEn,
    string FirstNameAr,
    string LastNameEn,
    string LastNameAr,
    string Email,
    string Phone,
    string EmployeeNumber,
    string SubjectEn,
    string SubjectAr,
    bool IsActive,
    Guid? UserId,
    IReadOnlyList<TeacherAssignmentDto> Assignments,
    DateTime CreatedAt);

public record TeacherListResult(
    IReadOnlyList<TeacherDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record CreateTeacherRequest(
    string FirstNameEn,
    string FirstNameAr,
    string LastNameEn,
    string LastNameAr,
    string Email,
    string Phone,
    string EmployeeNumber,
    string SubjectEn,
    string SubjectAr,
    bool IsActive = true,
    Guid? UserId = null);

public record UpdateTeacherRequest(
    string FirstNameEn,
    string FirstNameAr,
    string LastNameEn,
    string LastNameAr,
    string Email,
    string Phone,
    string EmployeeNumber,
    string SubjectEn,
    string SubjectAr,
    bool IsActive,
    Guid? UserId = null);

public record TeacherAssignmentInput(
    Guid SchoolClassId,
    Guid? SectionId,
    Guid SubjectId);

public record AssignTeacherRequest(IReadOnlyList<TeacherAssignmentInput> Assignments);
