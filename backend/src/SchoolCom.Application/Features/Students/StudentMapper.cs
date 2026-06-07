using SchoolCom.Application.Common;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Students;

public static class StudentMapper
{
    public static StudentDto ToDto(Student s, string? locale = null) => new(
        s.Id,
        s.StudentNumber,
        s.FirstNameEn,
        s.FirstNameAr,
        s.LastNameEn,
        s.LastNameAr,
        LocalizationHelper.FullName(s.FirstNameEn, s.FirstNameAr, s.LastNameEn, s.LastNameAr, locale),
        s.SchoolClassId,
        s.SchoolClass.NameEn,
        s.SchoolClass.NameAr,
        s.SectionId,
        s.Section.NameEn,
        s.Section.NameAr,
        s.ParentId,
        s.ParentName,
        s.ParentPhone,
        s.ParentEmail,
        s.IsActive ? "Active" : "Inactive",
        s.CreatedAt);
}
