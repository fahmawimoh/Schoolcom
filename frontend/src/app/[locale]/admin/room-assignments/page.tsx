"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useLocale, useTranslations } from "next-intl";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Building2,
  ChevronLeft,
  ChevronRight,
  Loader2,
  Pencil,
  Plus,
  ShieldAlert,
  Trash2,
} from "lucide-react";
import { toast } from "sonner";
import { z } from "zod";
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
import { Label } from "@/components/ui/label";
import { getLocalizedApiErrorMessage } from "@/lib/api/error-messages";
import {
  applyDeleteToListCache,
  refreshListQueries,
} from "@/lib/query/list-delete";
import {
  type CreateRoomAssignmentPayload,
  type RoomAssignment,
  type UpdateRoomAssignmentPayload,
  roomAssignmentsApi,
  roomsApi,
  schoolClassesApi,
} from "@/lib/api/academic";
import { useAuthStore } from "@/lib/auth/store";
import { pickLocalized } from "@/lib/utils";

const PAGE_SIZE = 10;

type FormValues = {
  schoolClassId: string;
  classSectionId: string;
  roomId: string;
};

export default function AdminRoomAssignmentsPage() {
  const t = useTranslations("roomAssignments");
  const tValidation = useTranslations("roomAssignments.validation");
  const locale = useLocale();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();

  const [page, setPage] = useState(1);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<RoomAssignment | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<DeleteConfirmState>(null);

  const isAdmin =
    user?.roles.includes("SchoolAdmin") ||
    user?.roles.includes("SuperAdmin");

  const formSchema = useMemo(
    () =>
      z.object({
        schoolClassId: z.string().min(1, tValidation("schoolClassRequired")),
        classSectionId: z.string().min(1, tValidation("classSectionRequired")),
        roomId: z.string().min(1, tValidation("roomRequired")),
      }),
    [tValidation]
  );

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: { schoolClassId: "", classSectionId: "", roomId: "" },
  });

  const watchedSchoolClassId = form.watch("schoolClassId");

  const { data: schoolClasses } = useQuery({
    queryKey: ["school-classes-options"],
    queryFn: () => schoolClassesApi.list({ page: 1, pageSize: 100, isActive: true }),
    enabled: isAdmin,
  });

  const { data: rooms } = useQuery({
    queryKey: ["rooms-options"],
    queryFn: () => roomsApi.list({ page: 1, pageSize: 100, isActive: true }),
    enabled: isAdmin && dialogOpen,
  });

  const { data: classSectionOptions } = useQuery({
    queryKey: ["class-section-options", watchedSchoolClassId],
    queryFn: () =>
      roomAssignmentsApi.getClassSectionOptions(
        watchedSchoolClassId || undefined
      ),
    enabled: isAdmin && dialogOpen && !!watchedSchoolClassId,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["room-assignments", page],
    queryFn: () => roomAssignmentsApi.list({ page, pageSize: PAGE_SIZE }),
    enabled: isAdmin,
  });

  const getSaveErrorMessage = useCallback(
    (err: unknown, fallback: string) =>
      getLocalizedApiErrorMessage(err, locale, fallback, (key) =>
        tValidation(key as "assignmentDuplicate")
      ),
    [locale, tValidation]
  );

  const invalidate = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["room-assignments"] });
  }, [queryClient]);

  const createMutation = useMutation({
    mutationFn: (payload: CreateRoomAssignmentPayload) =>
      roomAssignmentsApi.create(payload),
    onSuccess: () => {
      toast.success(t("created"));
      setDialogOpen(false);
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
      payload: UpdateRoomAssignmentPayload;
    }) => roomAssignmentsApi.update(id, payload),
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
    mutationFn: (id: string) => roomAssignmentsApi.delete(id),
    onSuccess: async (_result, deletedId) => {
      applyDeleteToListCache<RoomAssignment>(
        queryClient,
        ["room-assignments"],
        deletedId
      );
      setDeletingId(null);
      setDeleteConfirm(null);
      toast.success(t("deleted"));
      if (data?.items.length === 1 && page > 1) setPage((p) => p - 1);
      await refreshListQueries(queryClient, ["room-assignments"], [
        ["rooms-options"],
      ]);
    },
    onError: (err: unknown) => {
      setDeletingId(null);
      toast.error(getSaveErrorMessage(err, t("saveFailed")));
    },
  });

  useEffect(() => {
    if (!dialogOpen || editingItem) return;
    form.setValue("classSectionId", "");
  }, [watchedSchoolClassId, dialogOpen, editingItem, form]);

  const openCreate = () => {
    setEditingItem(null);
    form.reset({
      schoolClassId: schoolClasses?.items[0]?.id ?? "",
      classSectionId: "",
      roomId: "",
    });
    setDialogOpen(true);
  };

  const openEdit = (item: RoomAssignment) => {
    setEditingItem(item);
    form.reset({
      schoolClassId: item.schoolClassId,
      classSectionId: item.classSectionId,
      roomId: item.roomId,
    });
    setDialogOpen(true);
  };

  const onSubmit = (values: FormValues) => {
    const payload = {
      classSectionId: values.classSectionId,
      roomId: values.roomId,
    };
    if (editingItem) {
      updateMutation.mutate({ id: editingItem.id, payload });
    } else {
      createMutation.mutate(payload);
    }
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
              <Building2 className="h-7 w-7 text-primary" />
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
          <CardContent>
            {isLoading ? (
              <div className="flex justify-center py-12">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
              </div>
            ) : data && data.items.length > 0 ? (
              <div className="overflow-x-auto rounded-lg border">
                <table className="w-full text-sm">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="text-start p-3 font-medium">{t("schoolClass")}</th>
                      <th className="text-start p-3 font-medium">{t("section")}</th>
                      <th className="text-start p-3 font-medium">{t("room")}</th>
                      <th className="text-end p-3 font-medium">{t("actions")}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((item) => (
                      <tr key={item.id} className="border-t">
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
                          {pickLocalized(locale, item.roomNameEn, item.roomNameAr)}
                          {item.roomCode ? ` (${item.roomCode})` : ""}
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
                              onClick={() =>
                                setDeleteConfirm({
                                  id: item.id,
                                  description: t("deleteConfirm"),
                                })
                              }
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
              <div className="flex items-center justify-between pt-4">
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
          <Card className="w-full max-w-lg">
            <CardHeader>
              <CardTitle>{editingItem ? t("edit") : t("add")}</CardTitle>
            </CardHeader>
            <CardContent>
              <form
                onSubmit={form.handleSubmit(onSubmit, () =>
                  toast.error(tValidation("fixErrors"))
                )}
                className="space-y-4"
              >
                <div className="space-y-2">
                  <Label htmlFor="schoolClassId">{t("schoolClass")}</Label>
                  <select
                    id="schoolClassId"
                    className="flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm"
                    {...form.register("schoolClassId")}
                  >
                    <option value="">{t("selectSchoolClass")}</option>
                    {schoolClasses?.items.map((sc) => (
                      <option key={sc.id} value={sc.id}>
                        {pickLocalized(locale, sc.nameEn, sc.nameAr)}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="classSectionId">{t("section")}</Label>
                  <select
                    id="classSectionId"
                    disabled={!watchedSchoolClassId}
                    className="flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm disabled:opacity-60"
                    {...form.register("classSectionId")}
                  >
                    <option value="">{t("selectSection")}</option>
                    {classSectionOptions?.map((opt) => (
                      <option key={opt.classSectionId} value={opt.classSectionId}>
                        {pickLocalized(locale, opt.schoolClassNameEn, opt.schoolClassNameAr)}{" "}
                        — {pickLocalized(locale, opt.sectionNameEn, opt.sectionNameAr)}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="roomId">{t("room")}</Label>
                  <select
                    id="roomId"
                    className="flex h-10 w-full rounded-lg border border-input bg-background px-3 text-sm"
                    {...form.register("roomId")}
                  >
                    <option value="">{t("selectRoom")}</option>
                    {rooms?.items.map((room) => (
                      <option key={room.id} value={room.id}>
                        {pickLocalized(locale, room.nameEn, room.nameAr)}
                        {room.code ? ` (${room.code})` : ""}
                      </option>
                    ))}
                  </select>
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
