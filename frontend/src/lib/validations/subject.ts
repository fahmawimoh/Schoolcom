import { z } from "zod";

export type SubjectValidationMessages = {
  nameEnRequired: string;
  nameArRequired: string;
};

export function createSubjectFormSchema(messages: SubjectValidationMessages) {
  return z.object({
    nameEn: z.string().min(1, messages.nameEnRequired).max(100),
    nameAr: z.string().min(1, messages.nameArRequired).max(100),
    code: z.string().max(20).optional(),
    isActive: z.boolean(),
  });
}

export type SubjectFormValues = z.infer<
  ReturnType<typeof createSubjectFormSchema>
>;
