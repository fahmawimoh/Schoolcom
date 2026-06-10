"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useForm, type FieldPath } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useTranslations } from "next-intl";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  BookMarked,
  ChevronLeft,
  ChevronRight,
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
  type CreateSubjectPayload,
  type Subject,
  type UpdateSubjectPayload,
  subjectsApi,
} from "@/lib/api/academic";
import {
  createSubjectFormSchema,
  type SubjectFormValues,
} from "@/lib/validations/subject";
import { useAuthStore } from "@/lib/auth/store";
import { cn, pickLocalized } from "@/lib/utils";
import { useLocale } from "next-intl";

const PAGE_SIZE = 10;

const fieldClass = (hasError: boolean) =>
  cn(hasError && "border-destructive focus-visible:ring-destructive");

function getSaveErrorMessage(err: unknown, fallback: string): string {
  if (err instanceof ApiRequestError) return err.message;
  if (err instanceof Error && err.message) return err.message;
  return fallback;
}

export default function AdminSubjectsPage() {
  const t = useTranslations("subjects");
  const tValidation = useTranslations("subjects.validation");
  const locale = useLocale();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [searchInput, setSearchInput] = useState("");
  const [statusFilter, setStatusFilter] = useState<"" | "active" | "inactive">("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<Subject | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<DeleteConfirmState>(null);

  const isAdmin =
    user?.roles.includes("SchoolAdmin") ||
    user?.roles.includes("SuperAdmin");

  const isActiveFilter =
    statusFilter === "active" ? true : statusFilter === "inactive" ? false : undefined;

  const queryKey = ["subjects", page, search, statusFilter];

  const { data, isLoading, isError, error } = useQuery({
    queryKey,
    queryFn: () =>
      subjectsApi.list({
        page,
        pageSize: PAGE_SIZE,
        search,
        isActive: isActiveFilter,
      }),
    enabled: isAdmin,
  });

  const formSchema = useMemo(
    () =>
      createSubjectFormSchema({
        nameEnRequired: tValidation("nameEnRequired"),
        nameArRequired: tValidation("nameArRequired"),
      }),
    [tValidation]
  );

  const form = useForm<SubjectFormValues>({
    resolver: zodResolver(formSchema),
    mode: "onTouched",
    defaultValues: {
      nameEn: "",
      nameAr: "",
      code: "",
      isActive: true,
    },
  });

  const fieldError = (name: FieldPath<SubjectFormValues>) =>
    form.formState.errors[name]?.message;

  const invalidate = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["subjects"] });
  }, [queryClient]);

  const createMutation = useMutation({
    mutationFn: (payload: CreateSubjectPayload) => subjectsApi.create(payload),
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
    mutationFn: ({ id, payload }: { id: string; payload: UpdateSubjectPayload }) =>
      subjectsApi.update(id, payload),
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
    mutationFn: (id: string) => subjectsApi.delete(id),
    onSuccess: async (_result, deletedId) => {
      applyDeleteToListCache<Subject>(queryClient, ["subjects"], deletedId);
      setDeletingId(null);
      setDeleteConfirm(null);
      toast.success(t("deleted"));
      if (data?.items.length === 1 && page > 1) setPage((p) => p - 1);
      await refreshListQueries(queryClient, ["subjects"]);
    },
    onError: (err: unknown) => {
      setDeletingId(null);
      toast.error(getApiErrorMessage(err, t("saveFailed")));
    },
  });

  useEffect(() => {
    const timer = setTimeout(() => {
      setSearch(searchInput.trim());
      setPage(1);
    }, 300);
    return () => clearTimeout(timer);
  }, [searchInput]);

  const openCreate = () => {
    setEditingItem(null);
    form.reset({ nameEn: "", nameAr: "", code: "", isActive: true });
    setDialogOpen(true);
  };

  const openEdit = (item: Subject) => {
    setEditingItem(item);
    form.reset({
      nameEn: item.nameEn,
      nameAr: item.nameAr,
      code: item.code ?? "",
      isActive: item.isActive,
    });
    setDialogOpen(true);
  };

  const onSubmit = (values: SubjectFormValues) => {
    const payload = {
      nameEn: values.nameEn.trim(),
      nameAr: values.nameAr.trim(),
      code: values.code?.trim() || undefined,
      isActive: values.isActive,
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
              <BookMarked className="h-7 w-7 text-primary" />
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
                      <th className="text-start p-3 font-medium">{t("code")}</th>
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
                        <td className="p-3 font-mono text-xs" dir="ltr">
                          {item.code ?? "—"}
                        </td>
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
                                setDeleteConfirm({
                                  id: item.id,
                                  description: t("deleteConfirm", { name }),
                                });
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
                <div className="space-y-2">
                  <Label htmlFor="code">{t("code")}</Label>
                  <Input
                    id="code"
                    dir="ltr"
                    {...form.register("code")}
                  />
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
