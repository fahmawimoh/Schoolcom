import { z } from "zod";

export type TeacherValidationMessages = {
  firstNameEnRequired: string;
  firstNameArRequired: string;
  lastNameEnRequired: string;
  lastNameArRequired: string;
  emailRequired: string;
  emailInvalid: string;
  phoneRequired: string;
  employeeNumberRequired: string;
  subjectEnRequired: string;
  subjectArRequired: string;
};

export function createTeacherFormSchema(messages: TeacherValidationMessages) {
  return z.object({
    firstNameEn: z.string().min(1, messages.firstNameEnRequired).max(100),
    firstNameAr: z.string().min(1, messages.firstNameArRequired).max(100),
    lastNameEn: z.string().min(1, messages.lastNameEnRequired).max(100),
    lastNameAr: z.string().min(1, messages.lastNameArRequired).max(100),
    email: z.string().min(1, messages.emailRequired).email(messages.emailInvalid),
    phone: z.string().min(1, messages.phoneRequired).max(32),
    employeeNumber: z.string().min(1, messages.employeeNumberRequired).max(50),
    subjectEn: z.string().min(1, messages.subjectEnRequired).max(100),
    subjectAr: z.string().min(1, messages.subjectArRequired).max(100),
    isActive: z.boolean(),
  });
}

export type TeacherFormValues = z.infer<
  ReturnType<typeof createTeacherFormSchema>
>;

export type TeacherAssignmentValidationMessages = {
  schoolClassRequired: string;
  subjectRequired: string;
};

export function createTeacherAssignmentRowSchema(
  messages: TeacherAssignmentValidationMessages
) {
  return z.object({
    schoolClassId: z.string().min(1, messages.schoolClassRequired),
    sectionId: z.string().optional(),
    subjectId: z.string().min(1, messages.subjectRequired),
  });
}

export type TeacherAssignmentRowValues = z.infer<
  ReturnType<typeof createTeacherAssignmentRowSchema>
>;
