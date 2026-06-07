import { z } from "zod";

export type SectionValidationMessages = {
  schoolClassRequired: string;
  nameEnRequired: string;
  nameArRequired: string;
};

export function createSectionFormSchema(messages: SectionValidationMessages) {
  return z.object({
    schoolClassId: z.string().min(1, messages.schoolClassRequired),
    nameEn: z.string().min(1, messages.nameEnRequired).max(100),
    nameAr: z.string().min(1, messages.nameArRequired).max(100),
    descriptionEn: z.string().max(500).optional(),
    descriptionAr: z.string().max(500).optional(),
    isActive: z.boolean(),
  });
}

export type SectionFormValues = z.infer<
  ReturnType<typeof createSectionFormSchema>
>;
