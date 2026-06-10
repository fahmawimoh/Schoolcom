import { z } from "zod";

export type SectionValidationMessages = {
  nameEnRequired: string;
  nameArRequired: string;
  sectionNameDuplicate: string;
};

export function createSectionFormSchema(messages: SectionValidationMessages) {
  return z.object({
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
