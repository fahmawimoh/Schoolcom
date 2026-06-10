using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;

namespace SchoolCom.Application.Features.SchoolClasses;

internal static class SchoolClassSectionValidator
{
    public static void EnsureSectionsProvided(IReadOnlyList<Guid>? sectionIds)
    {
        if (sectionIds is not { Count: > 0 })
        {
            throw new ConflictException(
                AcademicErrorCodes.SectionsRequired,
                AcademicErrorMessages.SectionsRequiredEn,
                AcademicErrorMessages.SectionsRequiredAr,
                AcademicTranslationKeys.SectionsRequired);
        }
    }

    public static void EnsureNoDuplicateSectionIds(IReadOnlyList<Guid> sectionIds)
    {
        if (sectionIds.Count != sectionIds.Distinct().Count())
        {
            throw new ConflictException(
                AcademicErrorCodes.ClassSectionDuplicate,
                AcademicErrorMessages.ClassSectionDuplicateEn,
                AcademicErrorMessages.ClassSectionDuplicateAr,
                AcademicTranslationKeys.ClassSectionDuplicate);
        }
    }
}
