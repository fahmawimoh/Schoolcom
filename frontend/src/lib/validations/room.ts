import { z } from "zod";

export type RoomValidationMessages = {
  nameEnRequired: string;
  nameArRequired: string;
  capacityInvalid: string;
  schoolClassRequired: string;
  sectionRequired: string;
  roomDuplicate: string;
};

export function createRoomFormSchema(messages: RoomValidationMessages) {
  return z.object({
    nameEn: z.string().min(1, messages.nameEnRequired).max(100),
    nameAr: z.string().min(1, messages.nameArRequired).max(100),
    capacity: z
      .string()
      .optional()
      .refine(
        (s) => !s || (/^\d+$/.test(s) && Number(s) > 0),
        messages.capacityInvalid
      ),
    isActive: z.boolean(),
    schoolClassId: z.string().min(1, messages.schoolClassRequired),
    sectionId: z.string().min(1, messages.sectionRequired),
  });
}

export type RoomFormValues = z.infer<ReturnType<typeof createRoomFormSchema>>;

export function isDuplicateRoom(
  rooms: Array<{
    id: string;
    nameEn: string;
    schoolClassId?: string | null;
    sectionId?: string | null;
  }>,
  values: {
    nameEn: string;
    schoolClassId: string;
    sectionId: string;
  },
  excludeId?: string
): boolean {
  const normalizedName = values.nameEn.trim().toLowerCase();

  return rooms.some((room) => {
    if (excludeId && room.id === excludeId) return false;
    if (room.schoolClassId !== values.schoolClassId) return false;
    if (room.sectionId !== values.sectionId) return false;

    return room.nameEn.trim().toLowerCase() === normalizedName;
  });
}
