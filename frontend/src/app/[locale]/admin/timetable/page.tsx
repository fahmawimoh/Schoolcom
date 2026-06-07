"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useForm, type FieldPath } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useLocale, useTranslations } from "next-intl";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  CalendarClock,
  ChevronLeft,
  ChevronRight,
  Loader2,
  Pencil,
  Plus,
  ShieldAlert,
  Trash2,
} from "lucide-react";
import { toast } from "sonner";
import { DashboardShell } from "@/components/layout/dashboard-shell";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ApiRequestError } from "@/lib/api/client";
import {
  roomsApi,
  schoolClassesApi,
  sectionsApi,
  subjectsApi,
  teachersApi,
  timetablesApi,
  type CreateTimetableEntryPayload,
  type TimetableEntry,
  type UpdateTimetableEntryPayload,
} from "@/lib/api/academic";
import {
  createTimetableFormSchema,
  type TimetableFormValues,
} from "@/lib/validations/timetable";
import { useAuthStore } from "@/lib/auth/store";
import { cn, pickLocalized } from "@/lib/utils";

const PAGE_SIZE = 10;
const DAY_KEYS = ["1", "2", "3", "4", "5", "6", "7"] as const;

const fieldClass = (hasError: boolean) =>
  cn(hasError && "border-destructive focus-visible:ring-destructive");

function getSaveErrorMessage(err: unknown, fallback: string): string {
  if (err instanceof ApiRequestError) return err.message;
  if (err instanceof Error && err.message) return err.message;
  return fallback;
}

function toTimeInput(value: string): string {
  return value?.slice(0, 5) ?? "";
}

function toApiTime(value: string): string {
  return value.length === 5 ? `${value}:00` : value;
}

function teacherName(
  locale: string,
  entry: TimetableEntry
): string {
  const first = pickLocalized(
    locale,
    entry.teacherFirstNameEn,
    entry.teacherFirstNameAr
  );
  const last = pickLocalized(
    locale,
    entry.teacherLastNameEn,
    entry.teacherLastNameAr
  );
  return `${first} ${last}`;
}

export default function AdminTimetablePage() {
  const t = useTranslations("timetable");
  const tValidation = useTranslations("timetable.validation");
  const locale = useLocale();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [filterClassId, setFilterClassId] = useState("");
  const [filterSectionId, setFilterSectionId] = useState("");
  const [filterTeacherId, setFilterTeacherId] = useState("");
  const [filterSubjectId, setFilterSubjectId] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<TimetableEntry | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const isAdmin =
    user?.roles.includes("SchoolAdmin") ||
    user?.roles.includes("SuperAdmin");

  const queryKey = [
    "timetables",
    page,
    filterClassId,
    filterSectionId,
    filterTeacherId,
    filterSubjectId,
  ];

  const { data: schoolClasses } = useQuery({
    queryKey: ["school-classes-options"],
    queryFn: () => schoolClassesApi.list({ page: 1, pageSize: 200, isActive: true }),
    enabled: isAdmin,
  });

  const { data: filterSections } = useQuery({
    queryKey: ["timetable-filter-sections", filterClassId],
    queryFn: () => sectionsApi.getBySchoolClass(filterClassId),
    enabled: isAdmin && !!filterClassId,
  });

  const { data: teachers } = useQuery({
    queryKey: ["teachers-options"],
    queryFn: () => teachersApi.list({ page: 1, pageSize: 200, isActive: true }),
    enabled: isAdmin,
  });

  const { data: subjects } = useQuery({
    queryKey: ["subjects-options"],
    queryFn: () => subjectsApi.list({ page: 1, pageSize: 200, isActive: true }),
    enabled: isAdmin,
  });

  const { data: rooms } = useQuery({
    queryKey: ["rooms-options"],
    queryFn: () => roomsApi.list({ page: 1, pageSize: 200, isActive: true }),
    enabled: isAdmin && dialogOpen,
  });

  const { data, isLoading, isError, error } = useQuery({
    queryKey,
    queryFn: () =>
      timetablesApi.list({
        page,
        pageSize: PAGE_SIZE,
        schoolClassId: filterClassId || undefined,
        sectionId: filterSectionId || undefined,
        teacherId: filterTeacherId || undefined,
        subjectId: filterSubjectId || undefined,
      }),
    enabled: isAdmin,
  });

  const formSchema = useMemo(
    () =>
      createTimetableFormSchema({
        schoolClassRequired: tValidation("schoolClassRequired"),
        sectionRequired: tValidation("sectionRequired"),
        teacherRequired: tValidation("teacherRequired"),
        subjectRequired: tValidation("subjectRequired"),
        roomRequired: tValidation("roomRequired"),
        dayOfWeekRequired: tValidation("dayOfWeekRequired"),
        periodNumberInvalid: tValidation("periodNumberInvalid"),
        startTimeRequired: tValidation("startTimeRequired"),
        endTimeRequired: tValidation("endTimeRequired"),
      }),
    [tValidation]
  );

  const form = useForm<TimetableFormValues>({
    resolver: zodResolver(formSchema),
    mode: "onTouched",
    defaultValues: {
      schoolClassId: "",
      sectionId: "",
      teacherId: "",
      subjectId: "",
      roomId: "",
      dayOfWeek: "",
      periodNumber: "1",
      startTime: "",
      endTime: "",
    },
  });

  const watchedFormClassId = form.watch("schoolClassId");

  const { data: formSections, isLoading: formSectionsLoading } = useQuery({
    queryKey: ["timetable-form-sections", watchedFormClassId],
    queryFn: () => sectionsApi.getBySchoolClass(watchedFormClassId),
    enabled: isAdmin && dialogOpen && !!watchedFormClassId,
  });

  const fieldError = (name: FieldPath<TimetableFormValues>) =>
    form.formState.errors[name]?.message;

  const invalidate = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["timetables"] });
  }, [queryClient]);

  const createMutation = useMutation({
    mutationFn: (payload: CreateTimetableEntryPayload) =>
      timetablesApi.create(payload),
    onSuccess: () => {
      toast.success(t("created"));
      setDialogOpen(false);
      setPage(1);
      invalidate();
    },
    onError: (err: unknown) =>
      toast.error(getSaveErrorMessage(err, t("saveFailed"))),
  });

  const updateMutation = useMutation({
    mutationFn: ({
      id,
      payload,
    }: {
      id: string;
      payload: UpdateTimetableEntryPayload;
    }) => timetablesApi.update(id, payload),
    onSuccess: () => {
      toast.success(t("updated"));
      setDialogOpen(false);
      setEditingItem(null);
      invalidate();
    },
    onError: (err: unknown) =>
      toast.error(getSaveErrorMessage(err, t("saveFailed"))),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => timetablesApi.delete(id),
    onSuccess: () => {
      toast.success(t("deleted"));
      setDeletingId(null);
      if (data?.items.length === 1 && page > 1) setPage((p) => p - 1);
      invalidate();
    },
    onError: (err: unknown) =>
      toast.error(getSaveErrorMessage(err, t("saveFailed"))),
  });

  useEffect(() => {
    if (!filterClassId) setFilterSectionId("");
  }, [filterClassId]);

  useEffect(() => {
    if (!dialogOpen) return;
    const currentSectionId = form.getValues("sectionId");
    if (
      currentSectionId &&
      formSections &&
      !formSections.some((s) => s.sectionId === currentSectionId)
    ) {
      form.setValue("sectionId", "", { shouldValidate: true });
    }
  }, [formSections, dialogOpen, form]);

  const openCreate = () => {
    setEditingItem(null);
    const defaultClassId = schoolClasses?.items[0]?.id ?? "";
    form.reset({
      schoolClassId: defaultClassId,
      sectionId: "",
      teacherId: "",
      subjectId: "",
      roomId: "",
      dayOfWeek: "1",
      periodNumber: "1",
      startTime: "",
      endTime: "",
    });
    setDialogOpen(true);
  };

  const openEdit = (item: TimetableEntry) => {
    setEditingItem(item);
    form.reset({
      schoolClassId: item.schoolClassId,
      sectionId: item.sectionId,
      teacherId: item.teacherId,
      subjectId: item.subjectId,
      roomId: item.roomId,
      dayOfWeek: String(item.dayOfWeek),
      periodNumber: String(item.periodNumber),
      startTime: toTimeInput(item.startTime),
      endTime: toTimeInput(item.endTime),
    });
    setDialogOpen(true);
  };

  const onSubmit = (values: TimetableFormValues) => {
    const payload = {
      schoolClassId: values.schoolClassId,
      sectionId: values.sectionId,
      teacherId: values.teacherId,
      subjectId: values.subjectId,
      roomId: values.roomId,
      dayOfWeek: Number(values.dayOfWeek),
      periodNumber: Number(values.periodNumber),
      startTime: toApiTime(values.startTime),
      endTime: toApiTime(values.endTime),
    };

    if (editingItem) {
      updateMutation.mutate({ id: editingItem.id, payload });
    } else {
      createMutation.mutate(payload);
    }
  };

  const onInvalid = () => {
    toast.error(tValidation("fixErrors"));
  };

  if (!isAdmin) {
    return (
      <DashboardShell role="admin">
        <Card className="max-w-lg mx-auto mt-12">
          <CardContent className="flex flex-col items-center gap-4 py-12">
            <ShieldAlert className="h-12 w-12 text-destructive" />
            <p className="text-center text-muted-foreground">{t("adminOnly")}</p>
          </CardContent>
        </Card>
      </DashboardShell>
    );
  }

  const isSaving = createMutation.isPending || updateMutation.isPending;

  return (
    <DashboardShell role="admin">
      <div className="max-w-6xl mx-auto space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold flex items-center gap-2">
              <CalendarClock className="h-7 w-7 text-primary" />
              {t("title")}
            </h1>
            <p className="text-muted-foreground mt-1">{t("subtitle")}</p>
          </div>
          <Button onClick={openCreate}>
            <Plus className="h-4 w-4" />
            {t("add")}
          </Button>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>{t("listTitle")}</CardTitle>
            <CardDescription>
              {data
                ? t("showingCount", {
                    from: data.totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1,
                    to: Math.min(page * PAGE_SIZE, data.totalCount),
                    total: data.totalCount,
                  })
                : t("loading")}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
              <select
                className="h-10 rounded-lg border border-input bg-background px-3 text-sm"
                value={filterClassId}
                onChange={(e) => {
                  setFilterClassId(e.target.value);
                  setPage(1);
                }}
              >
                <option value="">{t("allSchoolClasses")}</option>
                {schoolClasses?.items.map((sc) => (
                  <option key={sc.id} value={sc.id}>
                    {pickLocalized(locale, sc.nameEn, sc.nameAr)}
                  </option>
                ))}
              </select>
              <select
                className="h-10 rounded-lg border border-input bg-background px-3 text-sm disabled:opacity-50"
                value={filterSectionId}
                disabled={!filterClassId}
                onChange={(e) => {
                  setFilterSectionId(e.target.value);
                  setPage(1);
                }}
              >
                <option value="">{t("allSections")}</option>
                {filterSections?.map((s) => (
                  <option key={s.sectionId} value={s.sectionId}>
                    {pickLocalized(locale, s.sectionNameEn, s.sectionNameAr)}
                  </option>
                ))}
              </select>
              <select
                className="h-10 rounded-lg border border-input bg-background px-3 text-sm"
                value={filterTeacherId}
                onChange={(e) => {
                  setFilterTeacherId(e.target.value);
                  setPage(1);
                }}
              >
                <option value="">{t("allTeachers")}</option>
                {teachers?.items.map((teacher) => (
                  <option key={teacher.id} value={teacher.id}>
                    {teacherFullName(locale, teacher)}
                  </option>
                ))}
              </select>
              <select
                className="h-10 rounded-lg border border-input bg-background px-3 text-sm"
                value={filterSubjectId}
                onChange={(e) => {
                  setFilterSubjectId(e.target.value);
                  setPage(1);
                }}
              >
                <option value="">{t("allSubjects")}</option>
                {subjects?.items.map((s) => (
                  <option key={s.id} value={s.id}>
                    {pickLocalized(locale, s.nameEn, s.nameAr)}
                  </option>
                ))}
              </select>
            </div>

            {isLoading ? (
              <div className="flex justify-center py-12">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
              </div>
            ) : isError ? (
              <p className="text-center text-destructive py-8">
                {(error as Error).message}
              </p>
            ) : data && data.items.length > 0 ? (
              <div className="overflow-x-auto rounded-lg border">
                <table className="w-full text-sm">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="text-start p-3 font-medium">{t("day")}</th>
                      <th className="text-start p-3 font-medium">{t("period")}</th>
                      <th className="text-start p-3 font-medium">{t("time")}</th>
                      <th className="text-start p-3 font-medium">{t("schoolClass")}</th>
                      <th className="text-start p-3 font-medium">{t("section")}</th>
                      <th className="text-start p-3 font-medium">{t("subject")}</th>
                      <th className="text-start p-3 font-medium">{t("teacher")}</th>
                      <th className="text-start p-3 font-medium">{t("room")}</th>
                      <th className="text-end p-3 font-medium">{t("actions")}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((item) => (
                      <tr key={item.id} className="border-t">
                        <td className="p-3">
                          {t(`days.${item.dayOfWeek}` as "days.1")}
                        </td>
                        <td className="p-3">{item.periodNumber}</td>
                        <td className="p-3 whitespace-nowrap" dir="ltr">
                          {toTimeInput(item.startTime)} – {toTimeInput(item.endTime)}
                        </td>
                        <td className="p-3">
                          {pickLocalized(
                            locale,
                            item.schoolClassNameEn,
                            item.schoolClassNameAr
                          )}
                        </td>
                        <td className="p-3">
                          {pickLocalized(
                            locale,
                            item.sectionNameEn,
                            item.sectionNameAr
                          )}
                        </td>
                        <td className="p-3">
                          {pickLocalized(
                            locale,
                            item.subjectNameEn,
                            item.subjectNameAr
                          )}
                        </td>
                        <td className="p-3">{teacherName(locale, item)}</td>
                        <td className="p-3">
                          {pickLocalized(locale, item.roomNameEn, item.roomNameAr)}
                        </td>
                        <td className="p-3">
                          <div className="flex justify-end gap-1">
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => openEdit(item)}
                            >
                              <Pencil className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              className="text-destructive hover:text-destructive"
                              disabled={deleteMutation.isPending}
                              onClick={() => {
                                if (window.confirm(t("deleteConfirm"))) {
                                  setDeletingId(item.id);
                                  deleteMutation.mutate(item.id);
                                }
                              }}
                            >
                              {deleteMutation.isPending &&
                              deletingId === item.id ? (
                                <Loader2 className="h-4 w-4 animate-spin" />
                              ) : (
                                <Trash2 className="h-4 w-4" />
                              )}
                            </Button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-center text-muted-foreground py-12">
                {t("noItems")}
              </p>
            )}

            {data && data.totalPages > 1 && (
              <div className="flex items-center justify-between pt-2">
                <p className="text-sm text-muted-foreground">
                  {t("pageInfo", { page: data.page, totalPages: data.totalPages })}
                </p>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page <= 1}
                    onClick={() => setPage((p) => p - 1)}
                  >
                    <ChevronLeft className="h-4 w-4" />
                    {t("previous")}
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page >= data.totalPages}
                    onClick={() => setPage((p) => p + 1)}
                  >
                    {t("next")}
                    <ChevronRight className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {dialogOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <Card className="w-full max-w-lg max-h-[90vh] overflow-y-auto">
            <CardHeader>
              <CardTitle>{editingItem ? t("edit") : t("add")}</CardTitle>
            </CardHeader>
            <CardContent>
              <form
                onSubmit={form.handleSubmit(onSubmit, onInvalid)}
                className="space-y-4"
                noValidate
              >
                <div className="space-y-2">
                  <Label htmlFor="schoolClassId">{t("schoolClass")}</Label>
                  <select
                    id="schoolClassId"
                    aria-invalid={!!fieldError("schoolClassId")}
                    className={cn(
                      "flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm",
                      fieldClass(!!fieldError("schoolClassId"))
                    )}
                    {...form.register("schoolClassId", {
                      onChange: () => {
                        form.setValue("sectionId", "", { shouldValidate: true });
                      },
                    })}
                  >
                    <option value="">{t("selectSchoolClass")}</option>
                    {schoolClasses?.items.map((sc) => (
                      <option key={sc.id} value={sc.id}>
                        {pickLocalized(locale, sc.nameEn, sc.nameAr)}
                      </option>
                    ))}
                  </select>
                  {fieldError("schoolClassId") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("schoolClassId")}
                    </p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="sectionId">{t("section")}</Label>
                  <select
                    id="sectionId"
                    disabled={!watchedFormClassId || formSectionsLoading}
                    aria-invalid={!!fieldError("sectionId")}
                    className={cn(
                      "flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm disabled:opacity-50",
                      fieldClass(!!fieldError("sectionId"))
                    )}
                    {...form.register("sectionId")}
                  >
                    <option value="">
                      {formSectionsLoading
                        ? t("loadingSections")
                        : t("selectSection")}
                    </option>
                    {formSections?.map((s) => (
                      <option key={s.sectionId} value={s.sectionId}>
                        {pickLocalized(locale, s.sectionNameEn, s.sectionNameAr)}
                      </option>
                    ))}
                  </select>
                  {fieldError("sectionId") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("sectionId")}
                    </p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="teacherId">{t("teacher")}</Label>
                  <select
                    id="teacherId"
                    aria-invalid={!!fieldError("teacherId")}
                    className={cn(
                      "flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm",
                      fieldClass(!!fieldError("teacherId"))
                    )}
                    {...form.register("teacherId")}
                  >
                    <option value="">{t("selectTeacher")}</option>
                    {teachers?.items.map((teacher) => (
                      <option key={teacher.id} value={teacher.id}>
                        {teacherFullName(locale, teacher)}
                      </option>
                    ))}
                  </select>
                  {fieldError("teacherId") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("teacherId")}
                    </p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="subjectId">{t("subject")}</Label>
                  <select
                    id="subjectId"
                    aria-invalid={!!fieldError("subjectId")}
                    className={cn(
                      "flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm",
                      fieldClass(!!fieldError("subjectId"))
                    )}
                    {...form.register("subjectId")}
                  >
                    <option value="">{t("selectSubject")}</option>
                    {subjects?.items.map((s) => (
                      <option key={s.id} value={s.id}>
                        {pickLocalized(locale, s.nameEn, s.nameAr)}
                      </option>
                    ))}
                  </select>
                  {fieldError("subjectId") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("subjectId")}
                    </p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="roomId">{t("room")}</Label>
                  <select
                    id="roomId"
                    aria-invalid={!!fieldError("roomId")}
                    className={cn(
                      "flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm",
                      fieldClass(!!fieldError("roomId"))
                    )}
                    {...form.register("roomId")}
                  >
                    <option value="">{t("selectRoom")}</option>
                    {rooms?.items.map((r) => (
                      <option key={r.id} value={r.id}>
                        {pickLocalized(locale, r.nameEn, r.nameAr)}
                      </option>
                    ))}
                  </select>
                  {fieldError("roomId") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("roomId")}
                    </p>
                  )}
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div className="space-y-2">
                    <Label htmlFor="dayOfWeek">{t("day")}</Label>
                    <select
                      id="dayOfWeek"
                      aria-invalid={!!fieldError("dayOfWeek")}
                      className={cn(
                        "flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm",
                        fieldClass(!!fieldError("dayOfWeek"))
                      )}
                      {...form.register("dayOfWeek")}
                    >
                      <option value="">{t("selectDay")}</option>
                      {DAY_KEYS.map((d) => (
                        <option key={d} value={d}>
                          {t(`days.${d}`)}
                        </option>
                      ))}
                    </select>
                    {fieldError("dayOfWeek") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("dayOfWeek")}
                      </p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="periodNumber">{t("period")}</Label>
                    <Input
                      id="periodNumber"
                      type="number"
                      min={1}
                      dir="ltr"
                      aria-invalid={!!fieldError("periodNumber")}
                      className={fieldClass(!!fieldError("periodNumber"))}
                      {...form.register("periodNumber")}
                    />
                    {fieldError("periodNumber") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("periodNumber")}
                      </p>
                    )}
                  </div>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div className="space-y-2">
                    <Label htmlFor="startTime">{t("startTime")}</Label>
                    <Input
                      id="startTime"
                      type="time"
                      dir="ltr"
                      aria-invalid={!!fieldError("startTime")}
                      className={fieldClass(!!fieldError("startTime"))}
                      {...form.register("startTime")}
                    />
                    {fieldError("startTime") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("startTime")}
                      </p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="endTime">{t("endTime")}</Label>
                    <Input
                      id="endTime"
                      type="time"
                      dir="ltr"
                      aria-invalid={!!fieldError("endTime")}
                      className={fieldClass(!!fieldError("endTime"))}
                      {...form.register("endTime")}
                    />
                    {fieldError("endTime") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("endTime")}
                      </p>
                    )}
                  </div>
                </div>
                <div className="flex gap-2 pt-2">
                  <Button
                    type="button"
                    variant="outline"
                    className="flex-1"
                    onClick={() => {
                      setDialogOpen(false);
                      setEditingItem(null);
                    }}
                  >
                    {t("cancel")}
                  </Button>
                  <Button type="submit" className="flex-1" disabled={isSaving}>
                    {isSaving ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      t("save")
                    )}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}
    </DashboardShell>
  );
}

function teacherFullName(locale: string, teacher: { firstNameEn: string; firstNameAr: string; lastNameEn: string; lastNameAr: string }) {
  const first = pickLocalized(locale, teacher.firstNameEn, teacher.firstNameAr);
  const last = pickLocalized(locale, teacher.lastNameEn, teacher.lastNameAr);
  return `${first} ${last}`;
}
