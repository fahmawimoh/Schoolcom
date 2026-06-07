using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Teachers;

public static class TeacherMapper
{
    public static TeacherDto ToDto(Teacher entity) =>
        new(
            entity.Id,
            entity.FirstNameEn,
            entity.FirstNameAr,
            entity.LastNameEn,
            entity.LastNameAr,
            entity.Email,
            entity.Phone,
            entity.EmployeeNumber,
            entity.SubjectEn,
            entity.SubjectAr,
            entity.IsActive,
            entity.UserId,
            entity.Assignments
                .Select(a => new TeacherAssignmentDto(
                    a.Id,
                    a.SchoolClassId,
                    a.SectionId,
                    a.SubjectId,
                    a.SchoolClass.NameEn,
                    a.SchoolClass.NameAr,
                    a.Section?.NameEn,
                    a.Section?.NameAr,
                    a.Subject.NameEn,
                    a.Subject.NameAr))
                .ToList(),
            entity.CreatedAt);
}
