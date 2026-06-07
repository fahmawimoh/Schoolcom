import { z } from "zod";

export type SchoolClassValidationMessages = {
  nameEnRequired: string;
  nameArRequired: string;
  sortOrderInvalid: string;
  sectionDuplicate: string;
};

export function createSchoolClassFormSchema(messages: SchoolClassValidationMessages) {
  return z.object({
    nameEn: z.string().min(1, messages.nameEnRequired).max(100),
    nameAr: z.string().min(1, messages.nameArRequired).max(100),
    descriptionEn: z.string().max(500).optional(),
    descriptionAr: z.string().max(500).optional(),
    sortOrder: z
      .string()
      .refine(
        (s) => s !== "" && /^\d+$/.test(s) && Number(s) >= 0,
        messages.sortOrderInvalid
      ),
    isActive: z.boolean(),
    selectedSections: z
      .array(z.string())
      .refine(
        (keys) => keys.length === new Set(keys.map((k) => k.trim().toLowerCase())).size,
        messages.sectionDuplicate
      ),
  });
}

export type SchoolClassFormValues = z.infer<
  ReturnType<typeof createSchoolClassFormSchema>
>;
