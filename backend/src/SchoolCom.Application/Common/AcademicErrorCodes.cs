namespace SchoolCom.Application.Common;

public static class AcademicErrorCodes
{
    public const string ClassSectionDuplicate = "CLASS_SECTION_DUPLICATE";
    public const string SectionNameDuplicate = "SECTION_NAME_DUPLICATE";
    public const string SchoolClassNameDuplicate = "SCHOOL_CLASS_NAME_DUPLICATE";
    public const string RoomCodeDuplicate = "ROOM_CODE_DUPLICATE";
    public const string RoomAssignmentDuplicate = "ROOM_ASSIGNMENT_DUPLICATE";
    public const string SectionsRequired = "SECTIONS_REQUIRED";
    public const string SectionNotFound = "SECTION_NOT_FOUND";
    public const string ClassSectionNotFound = "CLASS_SECTION_NOT_FOUND";
}

public static class AcademicTranslationKeys
{
    public const string ClassSectionDuplicate = "academic.classSectionDuplicate";
    public const string SectionNameDuplicate = "academic.sectionNameDuplicate";
    public const string SchoolClassNameDuplicate = "academic.schoolClassNameDuplicate";
    public const string RoomCodeDuplicate = "academic.roomCodeDuplicate";
    public const string RoomAssignmentDuplicate = "academic.roomAssignmentDuplicate";
    public const string SectionsRequired = "academic.sectionsRequired";
}

public static class AcademicErrorMessages
{
    public const string ClassSectionDuplicateEn =
        "This class is already assigned to this section.";

    public const string ClassSectionDuplicateAr =
        "هذا الصف مُعيَّن مسبقاً لهذه الشعبة";

    public const string SectionNameDuplicateEn =
        "This section already exists.";

    public const string SectionNameDuplicateAr =
        "هذه الشعبة موجودة مسبقاً";

    public const string SchoolClassNameDuplicateEn =
        "A school class with this name already exists.";

    public const string SchoolClassNameDuplicateAr =
        "صف بهذا الاسم موجود مسبقاً";

    public const string RoomCodeDuplicateEn =
        "A room with this number already exists.";

    public const string RoomCodeDuplicateAr =
        "غرفة بهذا الرقم موجودة مسبقاً";

    public const string RoomAssignmentDuplicateEn =
        "This room already exists for the selected class and section.";

    public const string RoomAssignmentDuplicateAr =
        "هذه الغرفة موجودة مسبقاً للصف والشعبة المحددين";

    public const string ClassSectionNotFoundEn =
        "The selected class and section combination was not found.";

    public const string ClassSectionNotFoundAr =
        "لم يتم العثور على تركيبة الصف والشعبة المحددة";

    public const string SectionsRequiredEn =
        "Please select at least one section for the school class.";

    public const string SectionsRequiredAr =
        "يرجى اختيار شعبة واحدة على الأقل للصف الدراسي";
}
