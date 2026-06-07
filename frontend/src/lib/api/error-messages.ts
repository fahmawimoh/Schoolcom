import { ApiRequestError } from "@/lib/api/client";

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

    if (err.status === 409 && err.errorCode === "CLASS_SECTION_DUPLICATE") {
      return locale === "ar"
        ? "هذا الصف مع الشعبة موجود مسبقاً"
        : "This class-section combination already exists.";
    }

    return err.message;
  }

  if (err instanceof Error && err.message) return err.message;
  return fallback;
}
