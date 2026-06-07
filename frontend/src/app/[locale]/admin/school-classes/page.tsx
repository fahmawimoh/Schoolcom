"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useForm, type FieldPath } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useLocale, useTranslations } from "next-intl";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ChevronLeft,
  ChevronRight,
  GraduationCap,
  Loader2,
  Pencil,
  Plus,
  Search,
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
import { getLocalizedApiErrorMessage } from "@/lib/api/error-messages";
import {
  type CreateSchoolClassPayload,
  type SchoolClass,
  type SchoolClassSectionInput,
  type UpdateSchoolClassPayload,
  schoolClassesApi,
  sectionsApi,
} from "@/lib/api/academic";
import {
  SECTION_PRESETS,
  normalizeSectionName,
} from "@/lib/academic/section-presets";
import {
  createSchoolClassFormSchema,
  type SchoolClassFormValues,
} from "@/lib/validations/schoolClass";
import { useAuthStore } from "@/lib/auth/store";
import { cn, pickLocalized } from "@/lib/utils";

const PAGE_SIZE = 10;

const fieldClass = (hasError: boolean) =>
  cn(hasError && "border-destructive focus-visible:ring-destructive");

export default function AdminSchoolClassesPage() {
  const t = useTranslations("schoolClasses");
  const tValidation = useTranslations("schoolClasses.validation");
  const locale = useLocale();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [searchInput, setSearchInput] = useState("");
  const [statusFilter, setStatusFilter] = useState<"" | "active" | "inactive">("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<SchoolClass | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [assignedSectionNames, setAssignedSectionNames] = useState<string[]>([]);

  const isAdmin =
    user?.roles.includes("SchoolAdmin") ||
    user?.roles.includes("SuperAdmin");

  const isActiveFilter =
    statusFilter === "active" ? true : statusFilter === "inactive" ? false : undefined;

  const queryKey = ["school-classes", page, search, statusFilter];

  const { data, isLoading, isError, error } = useQuery({
    queryKey,
    queryFn: () =>
      schoolClassesApi.list({
        page,
        pageSize: PAGE_SIZE,
        search,
        isActive: isActiveFilter,
      }),
    enabled: isAdmin,
  });

  const formSchema = useMemo(
    () =>
      createSchoolClassFormSchema({
        nameEnRequired: tValidation("nameEnRequired"),
        nameArRequired: tValidation("nameArRequired"),
        sortOrderInvalid: tValidation("sortOrderInvalid"),
        sectionDuplicate: tValidation("sectionDuplicate"),
      }),
    [tValidation]
  );

  const getSaveErrorMessage = useCallback(
    (err: unknown, fallback: string) =>
      getLocalizedApiErrorMessage(err, locale, fallback, (key) =>
        tValidation(key as "classSectionDuplicate")
      ),
    [locale, tValidation]
  );

  const form = useForm<SchoolClassFormValues>({
    resolver: zodResolver(formSchema),
    mode: "onTouched",
    defaultValues: {
      nameEn: "",
      nameAr: "",
      descriptionEn: "",
      descriptionAr: "",
      sortOrder: "0",
      isActive: true,
      selectedSections: [],
    },
  });

  const selectedSections = form.watch("selectedSections");

  const fieldError = (name: FieldPath<SchoolClassFormValues>) =>
    form.formState.errors[name]?.message;

  const invalidate = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["school-classes"] });
  }, [queryClient]);

  const createMutation = useMutation({
    mutationFn: (payload: CreateSchoolClassPayload) =>
      schoolClassesApi.create(payload),
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
    mutationFn: ({ id, payload }: { id: string; payload: UpdateSchoolClassPayload }) =>
      schoolClassesApi.update(id, payload),
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
    mutationFn: (id: string) => schoolClassesApi.delete(id),
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
    const timer = setTimeout(() => {
      setSearch(searchInput.trim());
      setPage(1);
    }, 300);
    return () => clearTimeout(timer);
  }, [searchInput]);

  const loadAssignedSections = useCallback(async (schoolClassId: string) => {
    const sections = await sectionsApi.getBySchoolClass(schoolClassId);
    setAssignedSectionNames(sections.map((s) => s.sectionNameEn));
  }, []);

  const openCreate = () => {
    setEditingItem(null);
    setAssignedSectionNames([]);
    form.reset({
      nameEn: "",
      nameAr: "",
      descriptionEn: "",
      descriptionAr: "",
      sortOrder: "0",
      isActive: true,
      selectedSections: [],
    });
    setDialogOpen(true);
  };

  const openEdit = async (item: SchoolClass) => {
    setEditingItem(item);
    setAssignedSectionNames([]);
    form.reset({
      nameEn: item.nameEn,
      nameAr: item.nameAr,
      descriptionEn: item.descriptionEn ?? "",
      descriptionAr: item.descriptionAr ?? "",
      sortOrder: String(item.sortOrder),
      isActive: item.isActive,
      selectedSections: [],
    });
    setDialogOpen(true);
    try {
      await loadAssignedSections(item.id);
    } catch {
      setAssignedSectionNames([]);
    }
  };

  const assignedSectionSet = useMemo(
    () => new Set(assignedSectionNames.map(normalizeSectionName)),
    [assignedSectionNames]
  );

  const buildSectionsPayload = (
    keys: string[]
  ): SchoolClassSectionInput[] =>
    SECTION_PRESETS.filter((preset) => keys.includes(preset.nameEn)).map(
      (preset) => ({
        nameEn: preset.nameEn,
        nameAr: preset.nameAr,
      })
    );

  const toggleSection = (nameEn: string) => {
    if (assignedSectionSet.has(normalizeSectionName(nameEn))) {
      toast.error(t("sectionAlreadyAssigned"));
      return;
    }

    const current = form.getValues("selectedSections");
    const next = current.includes(nameEn)
      ? current.filter((key) => key !== nameEn)
      : [...current, nameEn];

    form.setValue("selectedSections", next, { shouldValidate: true });
  };

  const onSubmit = (values: SchoolClassFormValues) => {
    const sections = buildSectionsPayload(values.selectedSections);
    const payload = {
      nameEn: values.nameEn.trim(),
      nameAr: values.nameAr.trim(),
      descriptionEn: values.descriptionEn?.trim() || undefined,
      descriptionAr: values.descriptionAr?.trim() || undefined,
      sortOrder: Number(values.sortOrder),
      isActive: values.isActive,
      ...(sections.length > 0 ? { sections } : {}),
    };

    if (editingItem) {
      updateMutation.mutate({ id: editingItem.id, payload });
    } else {
      createMutation.mutate(payload as CreateSchoolClassPayload);
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
              <GraduationCap className="h-7 w-7 text-primary" />
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
                      <th className="text-start p-3 font-medium">{t("sortOrder")}</th>
                      <th className="text-start p-3 font-medium">{t("sections")}</th>
                      <th className="text-start p-3 font-medium">{t("students")}</th>
                      <th className="text-start p-3 font-medium">{t("status")}</th>
                      <th className="text-end p-3 font-medium">{t("actions")}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((item) => (
                      <tr key={item.id} className="border-t">
                        <td className="p-3">
                          {pickLocalized(locale, item.nameEn, item.nameAr)}
                        </td>
                        <td className="p-3">{item.sortOrder}</td>
                        <td className="p-3">{item.sectionCount}</td>
                        <td className="p-3">{item.studentCount}</td>
                        <td className="p-3">
                          <span
                            className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                              item.isActive
                                ? "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400"
                                : "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400"
                            }`}
                          >
                            {item.isActive ? t("active") : t("inactive")}
                          </span>
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
                                const name = pickLocalized(
                                  locale,
                                  item.nameEn,
                                  item.nameAr
                                );
                                if (window.confirm(t("deleteConfirm", { name }))) {
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
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div className="space-y-2">
                    <Label htmlFor="nameEn">{t("nameEn")}</Label>
                    <Input
                      id="nameEn"
                      dir="ltr"
                      aria-invalid={!!fieldError("nameEn")}
                      className={fieldClass(!!fieldError("nameEn"))}
                      {...form.register("nameEn")}
                    />
                    {fieldError("nameEn") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("nameEn")}
                      </p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="nameAr">{t("nameAr")}</Label>
                    <Input
                      id="nameAr"
                      dir="rtl"
                      aria-invalid={!!fieldError("nameAr")}
                      className={fieldClass(!!fieldError("nameAr"))}
                      {...form.register("nameAr")}
                    />
                    {fieldError("nameAr") && (
                      <p className="text-xs text-destructive" role="alert">
                        {fieldError("nameAr")}
                      </p>
                    )}
                  </div>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div className="space-y-2">
                    <Label htmlFor="descriptionEn">{t("descriptionEn")}</Label>
                    <Input
                      id="descriptionEn"
                      dir="ltr"
                      {...form.register("descriptionEn")}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="descriptionAr">{t("descriptionAr")}</Label>
                    <Input
                      id="descriptionAr"
                      dir="rtl"
                      {...form.register("descriptionAr")}
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="sortOrder">{t("sortOrder")}</Label>
                  <Input
                    id="sortOrder"
                    type="number"
                    min={0}
                    dir="ltr"
                    aria-invalid={!!fieldError("sortOrder")}
                    className={fieldClass(!!fieldError("sortOrder"))}
                    {...form.register("sortOrder")}
                  />
                  {fieldError("sortOrder") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("sortOrder")}
                    </p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label>{t("sectionsLabel")}</Label>
                  <p className="text-xs text-muted-foreground">{t("sectionsHint")}</p>
                  {editingItem && assignedSectionNames.length > 0 && (
                    <div className="flex flex-wrap gap-2">
                      <span className="text-xs text-muted-foreground w-full">
                        {t("existingSections")}:
                      </span>
                      {assignedSectionNames.map((name) => (
                        <span
                          key={name}
                          className="inline-flex rounded-full bg-muted px-2.5 py-0.5 text-xs font-medium"
                        >
                          {name}
                        </span>
                      ))}
                    </div>
                  )}
                  <div className="flex flex-wrap gap-2">
                    {SECTION_PRESETS.map((preset) => {
                      const isAssigned = assignedSectionSet.has(
                        normalizeSectionName(preset.nameEn)
                      );
                      const isSelected = selectedSections.includes(preset.nameEn);

                      return (
                        <button
                          key={preset.nameEn}
                          type="button"
                          disabled={isAssigned}
                          title={
                            isAssigned ? t("sectionAlreadyAssigned") : undefined
                          }
                          onClick={() => toggleSection(preset.nameEn)}
                          className={cn(
                            "inline-flex h-9 min-w-9 items-center justify-center rounded-lg border px-3 text-sm font-medium transition-colors",
                            isAssigned &&
                              "cursor-not-allowed border-muted bg-muted text-muted-foreground opacity-60",
                            !isAssigned &&
                              isSelected &&
                              "border-primary bg-primary text-primary-foreground",
                            !isAssigned &&
                              !isSelected &&
                              "border-input bg-background hover:bg-muted/50"
                          )}
                        >
                          {pickLocalized(locale, preset.nameEn, preset.nameAr)}
                        </button>
                      );
                    })}
                  </div>
                  {fieldError("selectedSections") && (
                    <p className="text-xs text-destructive" role="alert">
                      {fieldError("selectedSections")}
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
