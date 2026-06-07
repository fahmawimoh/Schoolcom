namespace SchoolCom.Application.Features.Timetables;

public record TimetableEntryDto(
    Guid Id,
    Guid SchoolClassId,
    string SchoolClassNameEn,
    string SchoolClassNameAr,
    Guid SectionId,
    string SectionNameEn,
    string SectionNameAr,
    Guid TeacherId,
    string TeacherFirstNameEn,
    string TeacherFirstNameAr,
    string TeacherLastNameEn,
    string TeacherLastNameAr,
    Guid SubjectId,
    string SubjectNameEn,
    string SubjectNameAr,
    Guid RoomId,
    string RoomNameEn,
    string RoomNameAr,
    int DayOfWeek,
    int PeriodNumber,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DateTime CreatedAt);

public record TimetableEntryListResult(
    IReadOnlyList<TimetableEntryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record CreateTimetableEntryRequest(
    Guid SchoolClassId,
    Guid SectionId,
    Guid TeacherId,
    Guid SubjectId,
    Guid RoomId,
    int DayOfWeek,
    int PeriodNumber,
    TimeOnly StartTime,
    TimeOnly EndTime);

public record UpdateTimetableEntryRequest(
    Guid SchoolClassId,
    Guid SectionId,
    Guid TeacherId,
    Guid SubjectId,
    Guid RoomId,
    int DayOfWeek,
    int PeriodNumber,
    TimeOnly StartTime,
    TimeOnly EndTime);
