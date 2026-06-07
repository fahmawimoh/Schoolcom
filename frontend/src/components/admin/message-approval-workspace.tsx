"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  AlertCircle,
  Check,
  Clock,
  History,
  Loader2,
  RefreshCw,
  ShieldCheck,
  X,
} from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { cn } from "@/lib/utils";
import {
  moderationApi,
  messagingAttachmentUrl,
  type ModerationFilters,
  type PendingModeration,
} from "@/lib/api/messaging";

type Tab = "pending" | "history";

export function MessageApprovalWorkspace() {
  const t = useTranslations("messageApproval");
  const tCommon = useTranslations("common");
  const queryClient = useQueryClient();
  const [tab, setTab] = useState<Tab>("pending");
  const [filters, setFilters] = useState<ModerationFilters>({});
  const [appliedFilters, setAppliedFilters] = useState<ModerationFilters>({});
  const [rejectTarget, setRejectTarget] = useState<PendingModeration | null>(
    null
  );
  const [rejectNotes, setRejectNotes] = useState("");
  const [approveTarget, setApproveTarget] = useState<PendingModeration | null>(
    null
  );

  const {
    data: pending,
    isLoading: loadingPending,
    isError: pendingError,
    refetch: refetchPending,
  } = useQuery({
    queryKey: ["moderation-pending", appliedFilters],
    queryFn: () => moderationApi.getPending(appliedFilters),
    enabled: tab === "pending",
  });

  const {
    data: history,
    isLoading: loadingHistory,
    isError: historyError,
    refetch: refetchHistory,
  } = useQuery({
    queryKey: ["moderation-history", appliedFilters],
    queryFn: () => moderationApi.getHistory(appliedFilters, 50),
    enabled: tab === "history",
  });

  const approveMutation = useMutation({
    mutationFn: ({ messageId, notes }: { messageId: string; notes?: string }) =>
      moderationApi.approve(messageId, notes),
    onSuccess: () => {
      toast.success(t("approved"));
      setApproveTarget(null);
      void queryClient.invalidateQueries({ queryKey: ["moderation-pending"] });
      void queryClient.invalidateQueries({ queryKey: ["moderation-history"] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const rejectMutation = useMutation({
    mutationFn: ({ messageId, notes }: { messageId: string; notes: string }) =>
      moderationApi.reject(messageId, notes),
    onSuccess: () => {
      toast.success(t("rejected"));
      setRejectTarget(null);
      setRejectNotes("");
      void queryClient.invalidateQueries({ queryKey: ["moderation-pending"] });
      void queryClient.invalidateQueries({ queryKey: ["moderation-history"] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const applyFilters = () => setAppliedFilters({ ...filters });
  const clearFilters = () => {
    setFilters({});
    setAppliedFilters({});
  };

  const confirmReject = () => {
    if (!rejectTarget) return;
    const notes = rejectNotes.trim();
    if (!notes) {
      toast.error(t("rejectReasonRequired"));
      return;
    }
    rejectMutation.mutate({ messageId: rejectTarget.messageId, notes });
  };

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <ShieldCheck className="h-7 w-7 text-primary" />
          {t("title")}
        </h1>
        <p className="text-muted-foreground mt-1">{t("subtitle")}</p>
      </div>

      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">{t("filtersTitle")}</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-3">
          <div className="space-y-1.5">
            <Label htmlFor="filter-student">{t("filterStudentId")}</Label>
            <Input
              id="filter-student"
              value={filters.studentId ?? ""}
              onChange={(e) =>
                setFilters((f) => ({
                  ...f,
                  studentId: e.target.value || undefined,
                }))
              }
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="filter-parent">{t("filterParentId")}</Label>
            <Input
              id="filter-parent"
              value={filters.parentUserId ?? ""}
              onChange={(e) =>
                setFilters((f) => ({
                  ...f,
                  parentUserId: e.target.value || undefined,
                }))
              }
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="filter-teacher">{t("filterTeacherId")}</Label>
            <Input
              id="filter-teacher"
              value={filters.teacherUserId ?? ""}
              onChange={(e) =>
                setFilters((f) => ({
                  ...f,
                  teacherUserId: e.target.value || undefined,
                }))
              }
            />
          </div>
          {tab === "history" && (
            <div className="space-y-1.5 sm:col-span-3">
              <Label htmlFor="filter-status">{t("filterAllStatuses")}</Label>
              <select
                id="filter-status"
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
                value={filters.status ?? ""}
                onChange={(e) =>
                  setFilters((f) => ({
                    ...f,
                    status: e.target.value || undefined,
                  }))
                }
              >
                <option value="">{t("filterAllStatuses")}</option>
                <option value="Approved">{t("filterApproved")}</option>
                <option value="Rejected">{t("filterRejected")}</option>
              </select>
            </div>
          )}
          <div className="flex gap-2 sm:col-span-3">
            <Button type="button" size="sm" onClick={applyFilters}>
              {t("applyFilters")}
            </Button>
            <Button type="button" size="sm" variant="outline" onClick={clearFilters}>
              {t("clearFilters")}
            </Button>
          </div>
        </CardContent>
      </Card>

      <div className="flex gap-2 border-b">
        <button
          type="button"
          onClick={() => setTab("pending")}
          className={cn(
            "flex items-center gap-2 px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors",
            tab === "pending"
              ? "border-primary text-primary"
              : "border-transparent text-muted-foreground hover:text-foreground"
          )}
        >
          <Clock className="h-4 w-4" />
          {t("pendingTitle")}
          {pending && pending.length > 0 && (
            <span className="rounded-full bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-200 px-2 py-0.5 text-xs">
              {pending.length}
            </span>
          )}
        </button>
        <button
          type="button"
          onClick={() => setTab("history")}
          className={cn(
            "flex items-center gap-2 px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors",
            tab === "history"
              ? "border-primary text-primary"
              : "border-transparent text-muted-foreground hover:text-foreground"
          )}
        >
          <History className="h-4 w-4" />
          {t("historyTitle")}
        </button>
      </div>

      {tab === "pending" ? (
        <PendingPanel
          items={pending}
          loading={loadingPending}
          error={pendingError}
          onRetry={() => void refetchPending()}
          onApprove={setApproveTarget}
          onReject={setRejectTarget}
          t={t}
        />
      ) : (
        <HistoryPanel
          items={history}
          loading={loadingHistory}
          error={historyError}
          onRetry={() => void refetchHistory()}
          t={t}
        />
      )}

      <Dialog open={!!rejectTarget} onOpenChange={() => setRejectTarget(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t("reject")}</DialogTitle>
            <DialogDescription>{t("rejectReasonRequired")}</DialogDescription>
          </DialogHeader>
          <textarea
            className="flex min-h-[80px] w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm"
            placeholder={t("rejectReasonPlaceholder")}
            value={rejectNotes}
            onChange={(e) => setRejectNotes(e.target.value)}
          />
          <DialogFooter>
            <Button variant="outline" onClick={() => setRejectTarget(null)}>
              {tCommon("cancel")}
            </Button>
            <Button
              variant="destructive"
              disabled={rejectMutation.isPending}
              onClick={confirmReject}
            >
              {rejectMutation.isPending ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                t("reject")
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={!!approveTarget} onOpenChange={() => setApproveTarget(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t("approve")}</DialogTitle>
            <DialogDescription>{t("approved")}</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setApproveTarget(null)}>
              {tCommon("cancel")}
            </Button>
            <Button
              disabled={approveMutation.isPending}
              onClick={() => {
                if (approveTarget) {
                  approveMutation.mutate({ messageId: approveTarget.messageId });
                }
              }}
            >
              {approveMutation.isPending ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                t("approve")
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function PendingPanel({
  items,
  loading,
  error,
  onRetry,
  onApprove,
  onReject,
  t,
}: {
  items?: PendingModeration[];
  loading: boolean;
  error: boolean;
  onRetry: () => void;
  onApprove: (item: PendingModeration) => void;
  onReject: (item: PendingModeration) => void;
  t: ReturnType<typeof useTranslations<"messageApproval">>;
}) {
  if (error) {
    return (
      <ErrorState
        onRetry={onRetry}
        message={t("noPending")}
      />
    );
  }

  if (loading) {
    return (
      <div className="flex justify-center py-16">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!items?.length) {
    return (
      <Card>
        <CardContent className="py-12 text-center text-muted-foreground">
          {t("noPending")}
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">{t("pendingSubtitle")}</p>
      {items.map((item) => (
        <Card key={item.messageId}>
          <CardHeader className="pb-2">
            <div className="flex flex-wrap items-start justify-between gap-2">
              <div>
                <CardTitle className="text-base">{item.studentName}</CardTitle>
                <CardDescription>
                  {t("fromParent")}: {item.senderName} → {t("toTeacher")}:{" "}
                  {item.teacherName}
                </CardDescription>
              </div>
              <time className="text-xs text-muted-foreground shrink-0">
                {new Date(item.sentAt).toLocaleString()}
              </time>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-sm whitespace-pre-wrap rounded-lg bg-muted/50 p-3">
              {item.content || "—"}
            </p>
            {item.attachments.length > 0 && (
              <ul className="flex flex-wrap gap-2 text-sm">
                {item.attachments.map((a) => (
                  <li key={a.id}>
                    <a
                      href={messagingAttachmentUrl(a.downloadUrl)}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-primary underline"
                    >
                      {a.fileName}
                    </a>
                  </li>
                ))}
              </ul>
            )}
            <div className="flex flex-wrap gap-2">
              <Button size="sm" onClick={() => onApprove(item)}>
                <Check className="h-4 w-4 me-1" />
                {t("approve")}
              </Button>
              <Button
                size="sm"
                variant="outline"
                className="text-destructive"
                onClick={() => onReject(item)}
              >
                <X className="h-4 w-4 me-1" />
                {t("reject")}
              </Button>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

function HistoryPanel({
  items,
  loading,
  error,
  onRetry,
  t,
}: {
  items?: Array<{
    messageId: string;
    studentName: string;
    senderName: string;
    action: string;
    notes?: string | null;
    reviewerName: string;
    reviewedAt: string;
  }>;
  loading: boolean;
  error: boolean;
  onRetry: () => void;
  t: ReturnType<typeof useTranslations<"messageApproval">>;
}) {
  if (error) {
    return <ErrorState onRetry={onRetry} message={t("noHistory")} />;
  }

  if (loading) {
    return (
      <div className="flex justify-center py-16">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!items?.length) {
    return (
      <Card>
        <CardContent className="py-12 text-center text-muted-foreground">
          {t("noHistory")}
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-3">
      <p className="text-sm text-muted-foreground">{t("historySubtitle")}</p>
      <ul className="divide-y rounded-lg border bg-card">
        {items.map((item) => (
          <li key={`${item.messageId}-${item.reviewedAt}`} className="p-4">
            <div className="flex flex-wrap justify-between gap-2">
              <div>
                <p className="font-medium text-sm">{item.studentName}</p>
                <p className="text-xs text-muted-foreground">
                  {item.senderName} · {item.action} by {item.reviewerName}
                </p>
                {item.notes && (
                  <p className="text-sm mt-1 text-muted-foreground">
                    {item.notes}
                  </p>
                )}
              </div>
              <time className="text-xs text-muted-foreground">
                {new Date(item.reviewedAt).toLocaleString()}
              </time>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}

function ErrorState({
  onRetry,
  message,
}: {
  onRetry: () => void;
  message: string;
}) {
  return (
    <div className="flex flex-col items-center gap-3 py-12">
      <AlertCircle className="h-8 w-8 text-destructive" />
      <p className="text-sm text-muted-foreground">{message}</p>
      <Button size="sm" variant="outline" onClick={onRetry}>
        <RefreshCw className="h-4 w-4 me-1" />
        Retry
      </Button>
    </div>
  );
}
