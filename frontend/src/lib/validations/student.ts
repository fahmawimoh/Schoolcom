import { z } from "zod";

/** Matches .NET Guid.Parse — seed/demo IDs are not always RFC 4122 variant-compliant. */
const guidSchema = (message: string) =>
  z
    .string()
    .min(1, message)
    .regex(
      /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i,
      message
    );

export type StudentValidationMessages = {
  firstNameEnRequired: string;
  firstNameArRequired: string;
  lastNameEnRequired: string;
  lastNameArRequired: string;
  schoolClassRequired: string;
  sectionRequired: string;
  parentNameRequired: string;
  parentPhoneRequired: string;
  emailInvalid: string;
};

export function createStudentFormSchema(messages: StudentValidationMessages) {
  return z.object({
    firstNameEn: z.string().min(1, messages.firstNameEnRequired).max(100),
    firstNameAr: z.string().min(1, messages.firstNameArRequired).max(100),
    lastNameEn: z.string().min(1, messages.lastNameEnRequired).max(100),
    lastNameAr: z.string().min(1, messages.lastNameArRequired).max(100),
    schoolClassId: guidSchema(messages.schoolClassRequired),
    sectionId: guidSchema(messages.sectionRequired),
    parentName: z.string().min(1, messages.parentNameRequired).max(200),
    parentPhone: z.string().min(1, messages.parentPhoneRequired).max(32),
    parentEmail: z.union([
      z.literal(""),
      z.string().email(messages.emailInvalid),
    ]),
    isActive: z.boolean(),
  });
}

export type StudentFormValues = z.infer<
  ReturnType<typeof createStudentFormSchema>
>;
