import { z } from "zod";

export type SchoolClassValidationMessages = {
  nameEnRequired: string;
  nameArRequired: string;
  sortOrderInvalid: string;
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
  });
}

export type SchoolClassFormValues = z.infer<
  ReturnType<typeof createSchoolClassFormSchema>
>;
