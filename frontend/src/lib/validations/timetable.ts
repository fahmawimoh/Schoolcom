import { z } from "zod";

export type TimetableValidationMessages = {
  schoolClassRequired: string;
  sectionRequired: string;
  teacherRequired: string;
  subjectRequired: string;
  roomRequired: string;
  dayOfWeekRequired: string;
  periodNumberInvalid: string;
  startTimeRequired: string;
  endTimeRequired: string;
};

export function createTimetableFormSchema(messages: TimetableValidationMessages) {
  return z.object({
    schoolClassId: z.string().min(1, messages.schoolClassRequired),
    sectionId: z.string().min(1, messages.sectionRequired),
    teacherId: z.string().min(1, messages.teacherRequired),
    subjectId: z.string().min(1, messages.subjectRequired),
    roomId: z.string().min(1, messages.roomRequired),
    dayOfWeek: z.string().min(1, messages.dayOfWeekRequired),
    periodNumber: z
      .string()
      .refine(
        (s) => s !== "" && /^\d+$/.test(s) && Number(s) >= 1,
        messages.periodNumberInvalid
      ),
    startTime: z.string().min(1, messages.startTimeRequired),
    endTime: z.string().min(1, messages.endTimeRequired),
  });
}

export type TimetableFormValues = z.infer<
  ReturnType<typeof createTimetableFormSchema>
>;
