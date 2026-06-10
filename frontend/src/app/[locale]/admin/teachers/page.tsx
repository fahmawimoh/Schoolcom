"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useForm, type FieldPath } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useLocale, useTranslations } from "next-intl";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  BookOpen,
  ChevronLeft,
  ChevronRight,
  ClipboardList,
  Loader2,
  Pencil,
  Plus,
  Search,
  ShieldAlert,
  Trash2,
  X,
} from "lucide-react";
import { toast } from "sonner";
import { DashboardShell } from "@/components/layout/dashboard-shell";
import {
  DeleteConfirmDialog,
  type DeleteConfirmState,
} from "@/components/admin/delete-confirm-dialog";
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
import { getApiErrorMessage } from "@/lib/api/error-messages";
import {
  applyDeleteToListCache,
  refreshListQueries,
} from "@/lib/query/list-delete";
import {
  schoolClassesApi,
  sectionsApi,
  subjectsApi,
  type CreateTeacherPayload,
  type Teacher,
  type TeacherAssignmentInput,
  type UpdateTeacherPayload,
  teachersApi,
} from "@/lib/api/academic";
import {
  createTeacherFormSchema,
  type TeacherFormValues,
} from "@/lib/validations/teacher";
import { useAuthStore } from "@/lib/auth/store";
import { cn, pickLocalized } from "@/lib/utils";

const PAGE_SIZE = 10;

type AssignmentRow = {
  schoolClassId: string;
  sectionId: string;
  subjectId: string;
};

const fieldClass = (hasError: boolean) =>
  cn(hasError && "border-destructive focus-visible:ring-destructive");

function getSaveErrorMessage(err: unknown, fallback: string): string {
  if (err instanceof ApiRequestError) return err.message;
  if (err instanceof Error && err.message) return err.message;
  return fallback;
}

function teacherFullName(locale: string, teacher: Teacher): string {
  const first = pickLocalized(locale, teacher.firstNameEn, teacher.firstNameAr);
  const last = pickLocalized(locale, teacher.lastNameEn, teacher.lastNameAr);
  return `${first} ${last}`;
}

function assignmentLabel(
  locale: string,
  assignment: Teacher["assignments"][number]
): string {
  const className = pickLocalized(
    locale,
    assignment.schoolClassNameEn,
    assignment.schoolClassNameAr
  );
  const sectionName = assignment.sectionId
    ? pickLocalized(
        locale,
        assignment.sectionNameEn ?? "",
        assignment.sectionNameAr ?? ""
      )
    : null;
  const subjectName = pickLocalized(
    locale,
    assignment.subjectNameEn,
    assignment.subjectNameAr
  );
  return sectionName
    ? `${className} — ${sectionName} — ${subjectName}`
    : `${className} — ${subjectName}`;
}

function AssignmentRowEditor({
  row,
  index,
  locale,
  schoolClasses,
  subjects,
  onChange,
  onRemove,
  canRemove,
  t,
}: {
  row: AssignmentRow;
  index: number;
  locale: string;
  schoolClasses: { id: string; nameEn: string; nameAr: string }[];
  subjects: { id: string; nameEn: string; nameAr: string }[];
  onChange: (index: number, row: AssignmentRow) => void;
  onRemove: (index: number) => void;
  canRemove: boolean;
  t: ReturnType<typeof useTranslations<"teachers">>;
}) {
  const { data: sections, isLoading: sectionsLoading } = useQuery({
    queryKey: ["assignment-sections", row.schoolClassId],
    queryFn: () => sectionsApi.getBySchoolClass(row.schoolClassId),
    enabled: !!row.schoolClassId,
  });

  return (
    <div className="grid grid-cols-1 gap-2 rounded-lg border p-3">
      <div className="flex items-center justify-between">
        <span className="text-xs font-medium text-muted-foreground">
          {t("assignmentRow", { number: index + 1 })}
        </span>
        {canRemove && (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            className="h-7 w-7 p-0 text-destructive"
            onClick={() => onRemove(index)}
          >
            <X className="h-4 w-4" />
          </Button>
        )}
      </div>
      <div className="space-y-2">
        <Label>{t("schoolClass")}</Label>
        <select
          className="flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm"
          value={row.schoolClassId}
          onChange={(e) =>
            onChange(index, {
              schoolClassId: e.target.value,
              sectionId: "",
              subjectId: row.subjectId,
            })
          }
        >
          <option value="">{t("selectSchoolClass")}</option>
          {schoolClasses.map((sc) => (
            <option key={sc.id} value={sc.id}>
              {pickLocalized(locale, sc.nameEn, sc.nameAr)}
            </option>
          ))}
        </select>
      </div>
      <div className="space-y-2">
        <Label>{t("sectionOptional")}</Label>
        <select
          className="flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm disabled:opacity-50"
          value={row.sectionId}
          disabled={!row.schoolClassId || sectionsLoading}
          onChange={(e) =>
            onChange(index, { ...row, sectionId: e.target.value })
          }
        >
          <option value="">
            {sectionsLoading ? t("loadingSections") : t("wholeClass")}
          </option>
          {sections?.map((s) => (
            <option key={s.sectionId} value={s.sectionId}>
              {pickLocalized(locale, s.sectionNameEn, s.sectionNameAr)}
            </option>
          ))}
        </select>
      </div>
      <div className="space-y-2">
        <Label>{t("subject")}</Label>
        <select
          className="flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm"
          value={row.subjectId}
          onChange={(e) =>
            onChange(index, { ...row, subjectId: e.target.value })
          }
        >
          <option value="">{t("selectSubject")}</option>
          {subjects.map((s) => (
            <option key={s.id} value={s.id}>
              {pickLocalized(locale, s.nameEn, s.nameAr)}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
}

export default function AdminTeachersPage() {
  const t = useTranslations("teachers");
  const tValidation = useTranslations("teachers.validation");
  const locale = useLocale();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [searchInput, setSearchInput] = useState("");
  const [statusFilter, setStatusFilter] = useState<"" | "active" | "inactive">("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [assignmentOpen, setAssignmentOpen] = useState(false);
  const [editingTeacher, setEditingTeacher] = useState<Teacher | null>(null);
  const [assigningTeacher, setAssigningTeacher] = useState<Teacher | null>(null);
  const [assignmentRows, setAssignmentRows] = useState<AssignmentRow[]>([]);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<DeleteConfirmState>(null);

  const isAdmin =
    user?.roles.includes("SchoolAdmin") ||
    user?.roles.includes("SuperAdmin");

  const isActiveFilter =
    statusFilter === "active" ? true : statusFilter === "inactive" ? false : undefined;

  const queryKey = ["teachers", page, search, statusFilter];

  const { data, isLoading, isError, error } = useQuery({
    queryKey,
    queryFn: () =>
      teachersApi.list({
        page,
        pageSize: PAGE_SIZE,
        search,
        isActive: isActiveFilter,
      }),
    enabled: isAdmin,
  });

  const { data: schoolClasses } = useQuery({
    queryKey: ["school-classes-options"],
    queryFn: () => schoolClassesApi.list({ page: 1, pageSize: 200, isActive: true }),
    enabled: isAdmin && assignmentOpen,
  });

  const { data: subjects } = useQuery({
    queryKey: ["subjects-options"],
    queryFn: () => subjectsApi.list({ page: 1, pageSize: 200, isActive: true }),
    enabled: isAdmin && assignmentOpen,
  });

  const formSchema = useMemo(
    () =>
      createTeacherFormSchema({
        firstNameEnRequired: tValidation("firstNameEnRequired"),
        firstNameArRequired: tValidation("firstNameArRequired"),
        lastNameEnRequired: tValidation("lastNameEnRequired"),
        lastNameArRequired: tValidation("lastNameArRequired"),
        emailRequired: tValidation("emailRequired"),
        emailInvalid: tValidation("emailInvalid"),
        phoneRequired: tValidation("phoneRequired"),
        employeeNumberRequired: tValidation("employeeNumberRequired"),
        subjectEnRequired: tValidation("subjectEnRequired"),
        subjectArRequired: tValidation("subjectArRequired"),
      }),
    [tValidation]
  );

  const form = useForm<TeacherFormValues>({
    resolver: zodResolver(formSchema),
    mode: "onTouched",
    defaultValues: {
      firstNameEn: "",
      firstNameAr: "",
      lastNameEn: "",
      lastNameAr: "",
      email: "",
      phone: "",
      employeeNumber: "",
      subjectEn: "",
      subjectAr: "",
      isActive: true,
    },
  });

  const fieldError = (name: FieldPath<TeacherFormValues>) =>
    form.formState.errors[name]?.message;

  const invalidate = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["teachers"] });
  }, [queryClient]);

  const createMutation = useMutation({
    mutationFn: (payload: CreateTeacherPayload) => teachersApi.create(payload),
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
    mutationFn: ({ id, payload }: { id: string; payload: UpdateTeacherPayload }) =>
      teachersApi.update(id, payload),
    onSuccess: () => {
      toast.success(t("updated"));
      setDialogOpen(false);
      setEditingTeacher(null);
      invalidate();
    },
    onError: (err: unknown) =>
      toast.error(getSaveErrorMessage(err, t("saveFailed"))),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => teachersApi.delete(id),
    onSuccess: async (_result, deletedId) => {
      applyDeleteToListCache<Teacher>(queryClient, ["teachers"], deletedId);
      setDeletingId(null);
      setDeleteConfirm(null);
      toast.success(t("deleted"));
      if (data?.items.length === 1 && page > 1) setPage((p) => p - 1);
      await refreshListQueries(queryClient, ["teachers"], [
        ["teachers-options"],
      ]);
    },
    onError: (err: unknown) => {
      setDeletingId(null);
      toast.error(getApiErrorMessage(err, t("saveFailed")));
    },
  });

  const assignmentMutation = useMutation({
    mutationFn: ({
      id,
      assignments,
    }: {
      id: string;
      assignments: TeacherAssignmentInput[];
    }) => teachersApi.setAssignments(id, { assignments }),
    onSuccess: () => {
      toast.success(t("assignmentsUpdated"));
      setAssignmentOpen(false);
      setAssigningTeacher(null);
      invalidate();
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

  const openCreate = () => {
    setEditingTeacher(null);
    form.reset({
      firstNameEn: "",
      firstNameAr: "",
      lastNameEn: "",
      lastNameAr: "",
      email: "",
      phone: "",
      employeeNumber: "",
      subjectEn: "",
      subjectAr: "",
      isActive: true,
    });
    setDialogOpen(true);
  };

  const openEdit = (teacher: Teacher) => {
    setEditingTeacher(teacher);
    form.reset({
      firstNameEn: teacher.firstNameEn,
      firstNameAr: teacher.firstNameAr,
      lastNameEn: teacher.lastNameEn,
      lastNameAr: teacher.lastNameAr,
      email: teacher.email,
      phone: teacher.phone,
      employeeNumber: teacher.employeeNumber,
      subjectEn: teacher.subjectEn,
      subjectAr: teacher.subjectAr,
      isActive: teacher.isActive,
    });
    setDialogOpen(true);
  };

  const openAssignments = (teacher: Teacher) => {
    setAssigningTeacher(teacher);
    setAssignmentRows(
      teacher.assignments.length > 0
        ? teacher.assignments.map((a) => ({
            schoolClassId: a.schoolClassId,
            sectionId: a.sectionId ?? "",
            subjectId: a.subjectId,
          }))
        : [{ schoolClassId: "", sectionId: "", subjectId: "" }]
    );
    setAssignmentOpen(true);
  };

  const updateAssignmentRow = (index: number, row: AssignmentRow) => {
    setAssignmentRows((prev) => prev.map((r, i) => (i === index ? row : r)));
  };

  const removeAssignmentRow = (index: number) => {
    setAssignmentRows((prev) => prev.filter((_, i) => i !== index));
  };

  const addAssignmentRow = () => {
    setAssignmentRows((prev) => [
      ...prev,
      { schoolClassId: "", sectionId: "", subjectId: "" },
    ]);
  };

  const saveAssignments = () => {
    const incomplete = assignmentRows.some(
      (r) => !r.schoolClassId || !r.subjectId
    );
    if (incomplete) {
      toast.error(tValidation("assignmentIncomplete"));
      return;
    }

    const assignments: TeacherAssignmentInput[] = assignmentRows.map((r) => ({
      schoolClassId: r.schoolClassId,
      sectionId: r.sectionId || undefined,
      subjectId: r.subjectId,
    }));

    if (assigningTeacher) {
      assignmentMutation.mutate({ id: assigningTeacher.id, assignments });
    }
  };

  const onSubmit = (values: TeacherFormValues) => {
    const payload = {
      firstNameEn: values.firstNameEn.trim(),
      firstNameAr: values.firstNameAr.trim(),
      lastNameEn: values.lastNameEn.trim(),
      lastNameAr: values.lastNameAr.trim(),
      email: values.email.trim(),
      phone: values.phone.trim(),
      employeeNumber: values.employeeNumber.trim(),
      subjectEn: values.subjectEn.trim(),
      subjectAr: values.subjectAr.trim(),
      isActive: values.isActive,
    };

    if (editingTeacher) {
      updateMutation.mutate({ id: editingTeacher.id, payload });
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
              <BookOpen className="h-7 w-7 text-primary" />
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
                value={statusFilter}
                onChange={(e) => {
                  setStatusFilter(e.target.value as "" | "active" | "inactive");
                  setPage(1);
                }}
              >
                <option value="">{t("allStatuses")}</option>
                <option value="active">{t("active")}</option>
                <option value="inactive">{t("inactive")}</option>
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
                      <th className="text-start p-3 font-medium">{t("name")}</th>
                      <th className="text-start p-3 font-medium">{t("employeeNumber")}</th>
                      <th className="text-start p-3 font-medium">{t("subject")}</th>
                      <th className="text-start p-3 font-medium">{t("email")}</th>
                      <th className="text-start p-3 font-medium">{t("phone")}</th>
                      <th className="text-start p-3 font-medium">{t("assignments")}</th>
                      <th className="text-start p-3 font-medium">{t("status")}</th>
                      <th className="text-end p-3 font-medium">{t("actions")}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((teacher) => (
                      <tr key={teacher.id} className="border-t">
                        <td className="p-3">{teacherFullName(locale, teacher)}</td>
                        <td className="p-3 font-mono text-xs" dir="ltr">
                          {teacher.employeeNumber}
                        </td>
                        <td className="p-3">
                          {pickLocalized(locale, teacher.subjectEn, teacher.subjectAr)}
                        </td>
                        <td className="p-3" dir="ltr">
                          {teacher.email}
                        </td>
                        <td className="p-3" dir="ltr">
                          {teacher.phone}
                        </td>
                        <td className="p-3">{teacher.assignments.length}</td>
                        <td className="p-3">
                          <span
                            className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                              teacher.isActive
                                ? "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400"
                                : "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400"
                            }`}
                          >
                            {teacher.isActive ? t("active") : t("inactive")}
                          </span>
                        </td>
                        <td className="p-3">
                          <div className="flex justify-end gap-1">
                            <Button
                              variant="ghost"
                              size="sm"
                              title={t("manageAssignments")}
                              onClick={() => openAssignments(teacher)}
                            >
                              <ClipboardList className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => openEdit(teacher)}
                            >
                              <Pencil className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              className="text-destructive hover:text-destructive"
                              disabled={deleteMutation.isPending}
                              onClick={() => {
                                const name = teacherFullName(locale, teacher);
                                setDeleteConfirm({
                                  id: teacher.id,
                                  description: t("deleteConfirm", { name }),
                                });
                              }}
                            >
                              {deleteMutation.isPending &&
                              deletingId === teacher.id ? (
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
              <CardTitle>{editingTeacher ? t("edit") : t("add")}</CardTitle>
            </CardHeader>
            <CardContent>
              <form
                onSubmit={form.handleSubmit(onSubmit, onInvalid)}
                className="space-y-4"
                noValidate
              >
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
                  <Label htmlFor="employeeNumber">{t("employeeNumber")}</Label>
                  <Input
                    id="employeeNumber"
                    dir="ltr"
                    aria-invalid={!!fieldError("employeeNumber")}
                    className={fieldClass(!!fieldError("employeeNumber"))}
                    {...form.register("employeeNumber")}
                  />
                  {fieldError("employeeNumber") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("employeeNumber")}
                    </p>
                  )}
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div className="space-y-2">
                    <Label htmlFor="subjectEn">{t("subjectEn")}</Label>
                    <Input
                      id="subjectEn"
                      dir="ltr"
                      aria-invalid={!!fieldError("subjectEn")}
                      className={fieldClass(!!fieldError("subjectEn"))}
                      {...form.register("subjectEn")}
                    />
                    {fieldError("subjectEn") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("subjectEn")}
                      </p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="subjectAr">{t("subjectAr")}</Label>
                    <Input
                      id="subjectAr"
                      dir="rtl"
                      aria-invalid={!!fieldError("subjectAr")}
                      className={fieldClass(!!fieldError("subjectAr"))}
                      {...form.register("subjectAr")}
                    />
                    {fieldError("subjectAr") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("subjectAr")}
                      </p>
                    )}
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="email">{t("email")}</Label>
                  <Input
                    id="email"
                    type="email"
                    dir="ltr"
                    aria-invalid={!!fieldError("email")}
                    className={fieldClass(!!fieldError("email"))}
                    {...form.register("email")}
                  />
                  {fieldError("email") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("email")}
                    </p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="phone">{t("phone")}</Label>
                  <Input
                    id="phone"
                    type="tel"
                    dir="ltr"
                    aria-invalid={!!fieldError("phone")}
                    className={fieldClass(!!fieldError("phone"))}
                    {...form.register("phone")}
                  />
                  {fieldError("phone") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("phone")}
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
                      setEditingTeacher(null);
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

      {assignmentOpen && assigningTeacher && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <Card className="w-full max-w-lg max-h-[90vh] overflow-y-auto">
            <CardHeader>
              <CardTitle>{t("assignmentsTitle")}</CardTitle>
              <CardDescription>
                {t("assignmentsSubtitle", {
                  name: teacherFullName(locale, assigningTeacher),
                })}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {assigningTeacher.assignments.length > 0 && (
                <div className="space-y-1">
                  <p className="text-xs font-medium text-muted-foreground">
                    {t("currentAssignments")}
                  </p>
                  <ul className="text-sm space-y-1">
                    {assigningTeacher.assignments.map((a) => (
                      <li key={a.id} className="text-muted-foreground">
                        {assignmentLabel(locale, a)}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
              <div className="space-y-3 max-h-80 overflow-y-auto">
                {assignmentRows.map((row, index) => (
                  <AssignmentRowEditor
                    key={index}
                    row={row}
                    index={index}
                    locale={locale}
                    schoolClasses={schoolClasses?.items ?? []}
                    subjects={subjects?.items ?? []}
                    onChange={updateAssignmentRow}
                    onRemove={removeAssignmentRow}
                    canRemove={assignmentRows.length > 1}
                    t={t}
                  />
                ))}
              </div>
              <Button
                type="button"
                variant="outline"
                className="w-full"
                onClick={addAssignmentRow}
              >
                <Plus className="h-4 w-4" />
                {t("addAssignment")}
              </Button>
              <div className="flex gap-2 pt-2">
                <Button
                  type="button"
                  variant="outline"
                  className="flex-1"
                  onClick={() => {
                    setAssignmentOpen(false);
                    setAssigningTeacher(null);
                  }}
                >
                  {t("cancel")}
                </Button>
                <Button
                  className="flex-1"
                  disabled={assignmentMutation.isPending}
                  onClick={saveAssignments}
                >
                  {assignmentMutation.isPending ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    t("saveAssignments")
                  )}
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      <DeleteConfirmDialog
        state={deleteConfirm}
        onOpenChange={(open) => {
          if (!open && !deleteMutation.isPending) setDeleteConfirm(null);
        }}
        isPending={deleteMutation.isPending}
        onConfirm={(id) => {
          setDeletingId(id);
          deleteMutation.mutate(id);
        }}
      />
    </DashboardShell>
  );
}
