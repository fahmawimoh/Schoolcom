using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Timetables;

public static class TimetableMapper
{
    public static TimetableEntryDto ToDto(TimetableEntry entity) =>
        new(
            entity.Id,
            entity.SchoolClassId,
            entity.SchoolClass.NameEn,
            entity.SchoolClass.NameAr,
            entity.SectionId,
            entity.Section.NameEn,
            entity.Section.NameAr,
            entity.TeacherId,
            entity.Teacher.FirstNameEn,
            entity.Teacher.FirstNameAr,
            entity.Teacher.LastNameEn,
            entity.Teacher.LastNameAr,
            entity.SubjectId,
            entity.Subject.NameEn,
            entity.Subject.NameAr,
            entity.RoomId,
            entity.Room.NameEn,
            entity.Room.NameAr,
            entity.DayOfWeek,
            entity.PeriodNumber,
            entity.StartTime,
            entity.EndTime,
            entity.CreatedAt);
}
