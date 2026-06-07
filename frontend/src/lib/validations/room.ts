import { z } from "zod";

export type RoomValidationMessages = {
  nameEnRequired: string;
  nameArRequired: string;
  capacityInvalid: string;
};

export function createRoomFormSchema(messages: RoomValidationMessages) {
  return z.object({
    nameEn: z.string().min(1, messages.nameEnRequired).max(100),
    nameAr: z.string().min(1, messages.nameArRequired).max(100),
    code: z.string().max(20).optional(),
    capacity: z
      .string()
      .optional()
      .refine(
        (s) => !s || (/^\d+$/.test(s) && Number(s) > 0),
        messages.capacityInvalid
      ),
    isActive: z.boolean(),
  });
}

export type RoomFormValues = z.infer<ReturnType<typeof createRoomFormSchema>>;
