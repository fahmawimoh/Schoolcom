using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;

namespace SchoolCom.Application.Features.SchoolClasses;

internal static class SchoolClassSectionValidator
{
    public static void EnsureNoDuplicateNamesInRequest(IReadOnlyList<SchoolClassSectionInput>? sections)
    {
        if (sections is not { Count: > 0 }) return;

        var normalized = sections
            .Select(s => s.NameEn.Trim().ToLower())
            .ToList();

        if (normalized.Count != normalized.Distinct().Count())
        {
            throw new ConflictException(
                AcademicErrorCodes.ClassSectionDuplicate,
                AcademicErrorMessages.ClassSectionDuplicateEn,
                AcademicErrorMessages.ClassSectionDuplicateAr,
                AcademicTranslationKeys.ClassSectionDuplicate);
        }
    }

    public static void EnsureNoDuplicateAgainstExisting(
        IReadOnlyList<SchoolClassSectionInput> sections,
        IEnumerable<string> existingSectionNamesEn)
    {
        var existing = existingSectionNamesEn
            .Select(n => n.Trim().ToLower())
            .ToHashSet();

        foreach (var section in sections)
        {
            if (existing.Contains(section.NameEn.Trim().ToLower()))
            {
                throw new ConflictException(
                    AcademicErrorCodes.ClassSectionDuplicate,
                    AcademicErrorMessages.ClassSectionDuplicateEn,
                    AcademicErrorMessages.ClassSectionDuplicateAr,
                    AcademicTranslationKeys.ClassSectionDuplicate);
            }
        }
    }
}
