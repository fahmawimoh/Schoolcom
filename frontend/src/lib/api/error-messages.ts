import { ApiRequestError } from "@/lib/api/client";

export function getApiErrorMessage(err: unknown, fallback: string): string {
  if (err instanceof ApiRequestError) return err.message;
  if (err instanceof Error && err.message) return err.message;
  return fallback;
}

export function getLocalizedApiErrorMessage(
  err: unknown,
  locale: string,
  fallback: string,
  translate?: (key: string) => string
): string {
  if (err instanceof ApiRequestError) {
    if (translate && err.translationKey) {
      const key = err.translationKey.replace(/^academic\./, "");
      try {
        return translate(key);
      } catch {
        // Fall through to message below.
      }
    }

    if (err.status === 409) {
      if (err.errorCode === "CLASS_SECTION_DUPLICATE") {
        return locale === "ar"
          ? "هذا الصف مُعيَّن مسبقاً لهذه الشعبة"
          : "This class is already assigned to this section.";
      }
      if (err.errorCode === "SECTION_NAME_DUPLICATE") {
        return locale === "ar"
          ? "هذه الشعبة موجودة مسبقاً"
          : "This section already exists.";
      }
      if (err.errorCode === "SCHOOL_CLASS_NAME_DUPLICATE") {
        return locale === "ar"
          ? "صف بهذا الاسم موجود مسبقاً"
          : "A school class with this name already exists.";
      }
      if (err.errorCode === "ROOM_ASSIGNMENT_DUPLICATE") {
        return locale === "ar"
          ? "هذه الغرفة موجودة مسبقاً للصف والشعبة المحددين"
          : "This room already exists for the selected class and section.";
      }
      if (err.errorCode === "SECTIONS_REQUIRED") {
        return locale === "ar"
          ? "يرجى اختيار شعبة واحدة على الأقل للصف الدراسي"
          : "Please select at least one section for the school class.";
      }
    }

    return err.message;
  }

  if (err instanceof Error && err.message) return err.message;
  return fallback;
}
