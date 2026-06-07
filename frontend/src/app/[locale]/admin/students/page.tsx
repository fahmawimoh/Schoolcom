"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useForm, type FieldPath } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useLocale, useTranslations } from "next-intl";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ChevronLeft,
  ChevronRight,
  Loader2,
  Pencil,
  Plus,
  Search,
  ShieldAlert,
  Trash2,
  Users,
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
import {
  ApiRequestError,
  studentsApi,
  type CreateStudentPayload,
  type Student,
  type UpdateStudentPayload,
} from "@/lib/api/client";
import { sectionsApi } from "@/lib/api/academic";
import {
  createStudentFormSchema,
  type StudentFormValues,
} from "@/lib/validations/student";
import { useAuthStore } from "@/lib/auth/store";
import { cn, pickLocalized } from "@/lib/utils";

const PAGE_SIZE = 5;

const fieldClass = (hasError: boolean) =>
  cn(
    hasError && "border-destructive focus-visible:ring-destructive"
  );

function getSaveErrorMessage(err: unknown, fallback: string): string {
  if (err instanceof ApiRequestError) return err.message;
  if (err instanceof Error && err.message) return err.message;
  return fallback;
}

export default function AdminStudentsPage() {
  const t = useTranslations("students");
  const tValidation = useTranslations("students.validation");
  const locale = useLocale();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [schoolClassId, setSchoolClassId] = useState("");
  const [sectionId, setSectionId] = useState("");
  const [search, setSearch] = useState("");
  const [searchInput, setSearchInput] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingStudent, setEditingStudent] = useState<Student | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const isAdmin =
    user?.roles.includes("SchoolAdmin") ||
    user?.roles.includes("SuperAdmin");

  const queryKey = ["students", page, schoolClassId, sectionId, search, locale];

  const { data: filters } = useQuery({
    queryKey: ["student-filters"],
    queryFn: () => studentsApi.getFilters(),
    enabled: isAdmin,
  });

  const { data, isLoading, isError, error } = useQuery({
    queryKey,
    queryFn: () =>
      studentsApi.list({
        page,
        pageSize: PAGE_SIZE,
        schoolClassId,
        sectionId,
        search,
        lang: locale,
      }),
    enabled: isAdmin,
  });

  const studentFormSchema = useMemo(
    () =>
      createStudentFormSchema({
        firstNameEnRequired: tValidation("firstNameEnRequired"),
        firstNameArRequired: tValidation("firstNameArRequired"),
        lastNameEnRequired: tValidation("lastNameEnRequired"),
        lastNameArRequired: tValidation("lastNameArRequired"),
        schoolClassRequired: tValidation("schoolClassRequired"),
        sectionRequired: tValidation("sectionRequired"),
        parentNameRequired: tValidation("parentNameRequired"),
        parentPhoneRequired: tValidation("parentPhoneRequired"),
        emailInvalid: tValidation("emailInvalid"),
      }),
    [tValidation]
  );

  const form = useForm<StudentFormValues>({
    resolver: zodResolver(studentFormSchema),
    mode: "onTouched",
    defaultValues: {
      firstNameEn: "",
      firstNameAr: "",
      lastNameEn: "",
      lastNameAr: "",
      schoolClassId: "",
      sectionId: "",
      parentName: "",
      parentPhone: "",
      parentEmail: "",
      isActive: true,
    },
  });

  const watchedSchoolClassId = form.watch("schoolClassId");

  const { data: sectionOptions, isLoading: sectionsLoading } = useQuery({
    queryKey: ["student-section-options", watchedSchoolClassId],
    queryFn: () => sectionsApi.getBySchoolClass(watchedSchoolClassId),
    enabled: isAdmin && !!watchedSchoolClassId,
  });

  const fieldError = (name: FieldPath<StudentFormValues>) =>
    form.formState.errors[name]?.message;

  const invalidateStudents = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["students"] });
  }, [queryClient]);

  const createMutation = useMutation({
    mutationFn: (payload: CreateStudentPayload) => studentsApi.create(payload),
    onSuccess: (student) => {
      toast.success(t("createdWithId", { id: student.studentNumber }));
      setDialogOpen(false);
      setPage(1);
      invalidateStudents();
    },
    onError: (err: unknown) =>
      toast.error(getSaveErrorMessage(err, t("saveFailed"))),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateStudentPayload }) =>
      studentsApi.update(id, payload),
    onSuccess: () => {
      toast.success(t("updated"));
      setDialogOpen(false);
      setEditingStudent(null);
      invalidateStudents();
    },
    onError: (err: unknown) =>
      toast.error(getSaveErrorMessage(err, t("saveFailed"))),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => studentsApi.delete(id),
    onSuccess: () => {
      toast.success(t("deleted"));
      setDeletingId(null);
      if (data?.items.length === 1 && page > 1) setPage((p) => p - 1);
      invalidateStudents();
    },
    onError: (err: unknown) =>
      toast.error(getSaveErrorMessage(err, t("saveFailed"))),
  });

  useEffect(() => {
    const timer = setTimeout(() => {
      setSearch(searchInput.trim());
      setPage(1);
    }, 300);
    return () => clearTimeout(timer);
  }, [searchInput]);

  useEffect(() => {
    if (!dialogOpen) return;
    const currentSectionId = form.getValues("sectionId");
    if (
      currentSectionId &&
      sectionOptions &&
      !sectionOptions.some((s) => s.sectionId === currentSectionId)
    ) {
      form.setValue("sectionId", "", { shouldValidate: true });
    }
  }, [sectionOptions, dialogOpen, form]);

  const openCreate = () => {
    setEditingStudent(null);
    const defaultClassId = filters?.schoolClasses[0]?.id ?? "";
    form.reset({
      firstNameEn: "",
      firstNameAr: "",
      lastNameEn: "",
      lastNameAr: "",
      schoolClassId: defaultClassId,
      sectionId: "",
      parentName: "",
      parentPhone: "",
      parentEmail: "",
      isActive: true,
    });
    setDialogOpen(true);
  };

  const openEdit = (student: Student) => {
    setEditingStudent(student);
    form.reset({
      firstNameEn: student.firstNameEn,
      firstNameAr: student.firstNameAr,
      lastNameEn: student.lastNameEn,
      lastNameAr: student.lastNameAr,
      schoolClassId: student.schoolClassId,
      sectionId: student.sectionId,
      parentName: student.parentName,
      parentPhone: student.parentPhone,
      parentEmail: student.parentEmail ?? "",
      isActive: student.status === "Active",
    });
    setDialogOpen(true);
  };

  const onSubmit = (values: StudentFormValues) => {
    const payload = {
      firstNameEn: values.firstNameEn.trim(),
      firstNameAr: values.firstNameAr.trim(),
      lastNameEn: values.lastNameEn.trim(),
      lastNameAr: values.lastNameAr.trim(),
      schoolClassId: values.schoolClassId,
      sectionId: values.sectionId,
      parentName: values.parentName.trim(),
      parentPhone: values.parentPhone.trim(),
      parentEmail: values.parentEmail.trim() || undefined,
      isActive: values.isActive,
    };

    if (editingStudent) {
      updateMutation.mutate({ id: editingStudent.id, payload });
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
              <Users className="h-7 w-7 text-primary" />
              {t("title")}
            </h1>
            <p className="text-muted-foreground mt-1">{t("subtitle")}</p>
          </div>
          <Button onClick={openCreate}>
            <Plus className="h-4 w-4" />
            {t("addStudent")}
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
            <div className="flex flex-col md:flex-row gap-3">
              <div className="relative flex-1">
                <Search className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  className="ps-9"
                  placeholder={t("searchPlaceholder")}
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                />
              </div>
              <select
                className="h-10 rounded-lg border border-input bg-background px-3 text-sm"
                value={schoolClassId}
                onChange={(e) => {
                  setSchoolClassId(e.target.value);
                  setPage(1);
                }}
              >
                <option value="">{t("allSchoolClasses")}</option>
                {filters?.schoolClasses.map((sc) => (
                  <option key={sc.id} value={sc.id}>
                    {pickLocalized(locale, sc.nameEn, sc.nameAr)}
                  </option>
                ))}
              </select>
              <select
                className="h-10 rounded-lg border border-input bg-background px-3 text-sm"
                value={sectionId}
                onChange={(e) => {
                  setSectionId(e.target.value);
                  setPage(1);
                }}
              >
                <option value="">{t("allSections")}</option>
                {filters?.sections.map((s) => (
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
                      <th className="text-start p-3 font-medium">{t("studentId")}</th>
                      <th className="text-start p-3 font-medium">{t("fullName")}</th>
                      <th className="text-start p-3 font-medium">{t("schoolClass")}</th>
                      <th className="text-start p-3 font-medium">{t("section")}</th>
                      <th className="text-start p-3 font-medium">{t("parentName")}</th>
                      <th className="text-start p-3 font-medium">{t("phone")}</th>
                      <th className="text-start p-3 font-medium">{t("status")}</th>
                      <th className="text-end p-3 font-medium">{t("actions")}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((student) => (
                      <tr key={student.id} className="border-t">
                        <td className="p-3 font-mono text-xs" dir="ltr">
                          {student.studentNumber}
                        </td>
                        <td className="p-3">{student.fullName}</td>
                        <td className="p-3">
                          {pickLocalized(
                            locale,
                            student.schoolClassNameEn,
                            student.schoolClassNameAr
                          )}
                        </td>
                        <td className="p-3">
                          {pickLocalized(
                            locale,
                            student.sectionNameEn,
                            student.sectionNameAr
                          )}
                        </td>
                        <td className="p-3">{student.parentName}</td>
                        <td className="p-3" dir="ltr">
                          {student.parentPhone}
                        </td>
                        <td className="p-3">
                          <span
                            className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                              student.status === "Active"
                                ? "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400"
                                : "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400"
                            }`}
                          >
                            {student.status === "Active"
                              ? t("active")
                              : t("inactive")}
                          </span>
                        </td>
                        <td className="p-3">
                          <div className="flex justify-end gap-1">
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => openEdit(student)}
                            >
                              <Pencil className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              className="text-destructive hover:text-destructive"
                              disabled={deleteMutation.isPending}
                              onClick={() => {
                                if (
                                  window.confirm(
                                    t("deleteConfirm", { name: student.fullName })
                                  )
                                ) {
                                  setDeletingId(student.id);
                                  deleteMutation.mutate(student.id);
                                }
                              }}
                            >
                              {deleteMutation.isPending &&
                              deletingId === student.id ? (
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
                {t("noStudents")}
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
              <CardTitle>
                {editingStudent ? t("editStudent") : t("addStudent")}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <form
                onSubmit={form.handleSubmit(onSubmit, onInvalid)}
                className="space-y-4"
                noValidate
              >
                {editingStudent && (
                  <div className="space-y-2">
                    <span className="text-sm font-medium leading-none">
                      {t("studentId")}
                    </span>
                    <p
                      className="font-mono text-sm rounded-lg border border-input bg-muted px-3 py-2 text-muted-foreground select-all"
                      dir="ltr"
                      aria-label={t("studentId")}
                    >
                      {editingStudent.studentNumber}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {t("studentIdReadOnly")}
                    </p>
                  </div>
                )}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div className="space-y-2">
                    <Label htmlFor="firstNameEn">{t("firstNameEn")}</Label>
                    <Input
                      id="firstNameEn"
                      dir="ltr"
                      aria-invalid={!!fieldError("firstNameEn")}
                      className={fieldClass(!!fieldError("firstNameEn"))}
                      {...form.register("firstNameEn")}
                    />
                    {fieldError("firstNameEn") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("firstNameEn")}
                      </p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="firstNameAr">{t("firstNameAr")}</Label>
                    <Input
                      id="firstNameAr"
                      dir="rtl"
                      aria-invalid={!!fieldError("firstNameAr")}
                      className={fieldClass(!!fieldError("firstNameAr"))}
                      {...form.register("firstNameAr")}
                    />
                    {fieldError("firstNameAr") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("firstNameAr")}
                      </p>
                    )}
                  </div>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div className="space-y-2">
                    <Label htmlFor="lastNameEn">{t("lastNameEn")}</Label>
                    <Input
                      id="lastNameEn"
                      dir="ltr"
                      aria-invalid={!!fieldError("lastNameEn")}
                      className={fieldClass(!!fieldError("lastNameEn"))}
                      {...form.register("lastNameEn")}
                    />
                    {fieldError("lastNameEn") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("lastNameEn")}
                      </p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="lastNameAr">{t("lastNameAr")}</Label>
                    <Input
                      id="lastNameAr"
                      dir="rtl"
                      aria-invalid={!!fieldError("lastNameAr")}
                      className={fieldClass(!!fieldError("lastNameAr"))}
                      {...form.register("lastNameAr")}
                    />
                    {fieldError("lastNameAr") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("lastNameAr")}
                      </p>
                    )}
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="schoolClassId">{t("schoolClass")}</Label>
                  <select
                    id="schoolClassId"
                    aria-invalid={!!fieldError("schoolClassId")}
                    className={cn(
                      "flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
                      fieldClass(!!fieldError("schoolClassId"))
                    )}
                    {...form.register("schoolClassId", {
                      onChange: () => {
                        form.setValue("sectionId", "", { shouldValidate: true });
                      },
                    })}
                  >
                    <option value="">{t("selectSchoolClass")}</option>
                    {filters?.schoolClasses.map((sc) => (
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
                    disabled={!watchedSchoolClassId || sectionsLoading}
                    aria-invalid={!!fieldError("sectionId")}
                    className={cn(
                      "flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:opacity-50",
                      fieldClass(!!fieldError("sectionId"))
                    )}
                    {...form.register("sectionId")}
                  >
                    <option value="">
                      {sectionsLoading
                        ? t("loadingSections")
                        : t("selectSection")}
                    </option>
                    {sectionOptions?.map((s) => (
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
                  <Label htmlFor="parentName">{t("parentName")}</Label>
                  <Input
                    id="parentName"
                    aria-invalid={!!fieldError("parentName")}
                    className={fieldClass(!!fieldError("parentName"))}
                    {...form.register("parentName")}
                  />
                  {fieldError("parentName") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("parentName")}
                    </p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="parentPhone">{t("phone")}</Label>
                  <Input
                    id="parentPhone"
                    type="tel"
                    dir="ltr"
                    aria-invalid={!!fieldError("parentPhone")}
                    className={fieldClass(!!fieldError("parentPhone"))}
                    {...form.register("parentPhone")}
                  />
                  {fieldError("parentPhone") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("parentPhone")}
                    </p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="parentEmail">{t("emailOptional")}</Label>
                  <Input
                    id="parentEmail"
                    type="email"
                    dir="ltr"
                    aria-invalid={!!fieldError("parentEmail")}
                    className={fieldClass(!!fieldError("parentEmail"))}
                    {...form.register("parentEmail")}
                  />
                  {fieldError("parentEmail") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("parentEmail")}
                    </p>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  <input
                    id="isActive"
                    type="checkbox"
                    className="h-4 w-4 rounded border-input"
                    checked={form.watch("isActive")}
                    onChange={(e) =>
                      form.setValue("isActive", e.target.checked, {
                        shouldValidate: true,
                      })
                    }
                  />
                  <Label htmlFor="isActive">{t("active")}</Label>
                </div>
                <div className="flex gap-2 pt-2">
                  <Button
                    type="button"
                    variant="outline"
                    className="flex-1"
                    onClick={() => {
                      setDialogOpen(false);
                      setEditingStudent(null);
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
