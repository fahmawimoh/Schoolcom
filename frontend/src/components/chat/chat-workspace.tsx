"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useTranslations } from "next-intl";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  AlertCircle,
  Check,
  CheckCheck,
  Clock,
  FileText,
  Image as ImageIcon,
  Loader2,
  Paperclip,
  RefreshCw,
  Search,
  Send,
  Video,
  X,
} from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import {
  messagingApi,
  messagingAttachmentUrl,
  validateFileSize,
  validateFileType,
  type ChatMessage,
  type ConversationListItem,
  type LinkedStudent,
} from "@/lib/api/messaging";
import { useChatHub } from "@/lib/chat/use-chat-hub";

interface ChatWorkspaceProps {
  role: "teacher" | "parent";
}

function ListSkeleton() {
  return (
    <div className="space-y-3 p-4">
      {[1, 2, 3].map((i) => (
        <div key={i} className="animate-pulse space-y-2">
          <div className="h-4 bg-muted rounded w-3/4" />
          <div className="h-3 bg-muted rounded w-1/2" />
        </div>
      ))}
    </div>
  );
}

function MessageSkeleton() {
  return (
    <div className="space-y-4 p-4">
      {[1, 2, 4].map((i) => (
        <div
          key={i}
          className={cn(
            "animate-pulse h-12 rounded-lg bg-muted",
            i % 2 === 0 ? "ms-auto w-2/3" : "w-2/3"
          )}
        />
      ))}
    </div>
  );
}

export function ChatWorkspace({ role }: ChatWorkspaceProps) {
  const t = useTranslations("messages");
  const queryClient = useQueryClient();
  const [activeId, setActiveId] = useState<string | null>(null);
  const [draft, setDraft] = useState("");
  const [files, setFiles] = useState<File[]>([]);
  const [filePreviews, setFilePreviews] = useState<string[]>([]);
  const [uploadProgress, setUploadProgress] = useState<number | null>(null);
  const [conversationSearch, setConversationSearch] = useState("");
  const [messageSearch, setMessageSearch] = useState("");
  const [debouncedConvSearch, setDebouncedConvSearch] = useState("");
  const [debouncedMsgSearch, setDebouncedMsgSearch] = useState("");
  const [showNewChat, setShowNewChat] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const id = setTimeout(() => setDebouncedConvSearch(conversationSearch), 300);
    return () => clearTimeout(id);
  }, [conversationSearch]);

  useEffect(() => {
    const id = setTimeout(() => setDebouncedMsgSearch(messageSearch), 300);
    return () => clearTimeout(id);
  }, [messageSearch]);

  const {
    data: conversations,
    isLoading: loadingList,
    isError: listError,
    refetch: refetchList,
  } = useQuery({
    queryKey: ["conversations", debouncedConvSearch],
    queryFn: () => messagingApi.getConversations(debouncedConvSearch || undefined),
  });

  const { data: linkedStudents, isLoading: loadingStudents } = useQuery({
    queryKey: ["linked-students"],
    queryFn: () => messagingApi.getLinkedStudents(),
  });

  const activeConversation = conversations?.find((c) => c.id === activeId);

  const [loadingOlder, setLoadingOlder] = useState(false);

  const {
    data: messageData,
    isLoading: loadingMessages,
    isError: messagesError,
    refetch: refetchMessages,
  } = useQuery({
    queryKey: ["messages", activeId, debouncedMsgSearch],
    queryFn: async () => {
      if (!activeId) return { items: [], hasMore: false, nextCursor: null };
      return messagingApi.getMessages(
        activeId,
        undefined,
        debouncedMsgSearch || undefined
      );
    },
    enabled: !!activeId,
  });

  const [olderMessages, setOlderMessages] = useState<ChatMessage[]>([]);
  const [nextCursor, setNextCursor] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(false);

  useEffect(() => {
    setOlderMessages([]);
    setNextCursor(messageData?.nextCursor ?? null);
    setHasMore(messageData?.hasMore ?? false);
  }, [activeId, debouncedMsgSearch, messageData?.nextCursor, messageData?.hasMore]);

  const messages = useMemo(() => {
    const all = [...(messageData?.items ?? []), ...olderMessages];
    return all.filter((m) => m.moderationStatus === "Approved");
  }, [messageData?.items, olderMessages]);

  const appendMessage = useCallback(
    (msg: ChatMessage) => {
      if (!activeId || !msg?.id || msg.conversationId !== activeId) return;
      if (msg.moderationStatus !== "Approved") return;
      queryClient.setQueryData(
        ["messages", activeId, debouncedMsgSearch],
        (old: typeof messageData | undefined) => {
          if (!old) return { items: [msg], hasMore: false, nextCursor: null };
          if (old.items.some((m) => m.id === msg.id)) return old;
          return { ...old, items: [...old.items, msg] };
        }
      );
    },
    [activeId, queryClient, messageData, debouncedMsgSearch]
  );

  useChatHub(
    activeId,
    (msg) => {
      if (msg?.id) appendMessage(msg);
      else if (activeId)
        void queryClient.invalidateQueries({
          queryKey: ["messages", activeId],
        });
    },
    (n) => {
      toast.info(n.title, { description: n.body });
      void queryClient.invalidateQueries({ queryKey: ["notifications"] });
      void queryClient.invalidateQueries({ queryKey: ["conversations"] });
    }
  );

  useEffect(() => {
    if (activeId) {
      void messagingApi.markRead(activeId);
      void queryClient.invalidateQueries({ queryKey: ["conversations"] });
    }
  }, [activeId, messages.length, queryClient]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, activeId]);

  const loadOlder = async () => {
    if (!activeId || !nextCursor || loadingOlder) return;
    setLoadingOlder(true);
    try {
      const result = await messagingApi.getMessages(
        activeId,
        nextCursor,
        debouncedMsgSearch || undefined
      );
      setOlderMessages((prev) => [...result.items, ...prev]);
      setNextCursor(result.nextCursor ?? null);
      setHasMore(result.hasMore);
    } finally {
      setLoadingOlder(false);
    }
  };

  const sendMutation = useMutation({
    mutationFn: () => {
      if (!activeId) throw new Error("No conversation");
      return messagingApi.sendMessage(
        activeId,
        draft,
        files,
        setUploadProgress
      );
    },
    onSuccess: (result) => {
      setDraft("");
      setFiles([]);
      setFilePreviews([]);
      setUploadProgress(null);
      if (!result.requiresApproval) {
        appendMessage(result.message);
      } else {
        toast.info(t("pendingApproval"));
      }
      void queryClient.invalidateQueries({ queryKey: ["conversations"] });
    },
    onError: (err: Error) => {
      setUploadProgress(null);
      toast.error(err.message);
    },
  });

  const ensureMutation = useMutation({
    mutationFn: (studentId: string) => messagingApi.ensureConversation(studentId),
    onSuccess: (conv) => {
      setActiveId(conv.id);
      setShowNewChat(false);
      void queryClient.invalidateQueries({ queryKey: ["conversations"] });
      void queryClient.invalidateQueries({ queryKey: ["linked-students"] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const onFilePick = (e: React.ChangeEvent<HTMLInputElement>) => {
    const picked = Array.from(e.target.files ?? []);
    for (const f of picked) {
      const typeErr = validateFileType(f);
      if (typeErr) {
        toast.error(typeErr);
        return;
      }
      const err = validateFileSize(f);
      if (err) {
        toast.error(err);
        return;
      }
    }
    setFiles((prev) => [...prev, ...picked]);
    picked.forEach((f) => {
      if (f.type.startsWith("image/")) {
        const url = URL.createObjectURL(f);
        setFilePreviews((p) => [...p, url]);
      }
    });
    e.target.value = "";
  };

  const removeFile = (index: number) => {
    setFiles((prev) => prev.filter((_, i) => i !== index));
    setFilePreviews((prev) => {
      const url = prev[index];
      if (url) URL.revokeObjectURL(url);
      return prev.filter((_, i) => i !== index);
    });
  };

  const otherLabel = (c: ConversationListItem) =>
    role === "teacher" ? c.parentName : c.teacherName;

  return (
    <div className="flex h-[calc(100vh-8rem)] min-h-[480px] rounded-xl border overflow-hidden shadow-lg bg-[#f0f2f5] dark:bg-background">
      <aside
        className={cn(
          "w-full md:w-80 lg:w-96 border-e flex flex-col bg-white dark:bg-card",
          activeId && "hidden md:flex"
        )}
      >
        <div className="p-4 border-b space-y-3 bg-[#f0f2f5] dark:bg-muted/30">
          <div className="flex items-center justify-between gap-2">
            <h2 className="font-semibold text-lg text-[#111b21] dark:text-foreground">
              {t("conversations")}
            </h2>
            <Button
              size="sm"
              variant="outline"
              onClick={() => setShowNewChat((v) => !v)}
            >
              {t("newChat")}
            </Button>
          </div>
          <div className="relative">
            <Search className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              className="ps-9"
              placeholder={t("searchConversations")}
              value={conversationSearch}
              onChange={(e) => setConversationSearch(e.target.value)}
            />
          </div>
        </div>

        {showNewChat && (
          <div className="border-b p-3 max-h-40 overflow-y-auto space-y-1 bg-card">
            {loadingStudents ? (
              <p className="text-xs text-muted-foreground">{t("loadingStudents")}</p>
            ) : linkedStudents && linkedStudents.length > 0 ? (
              linkedStudents.map((s) => (
                <button
                  key={s.id}
                  type="button"
                  disabled={ensureMutation.isPending}
                  onClick={() => ensureMutation.mutate(s.id)}
                  className="w-full text-start rounded-lg px-2 py-2 text-sm hover:bg-muted transition-colors"
                >
                  <span className="font-medium">{s.fullName}</span>
                  {s.classroomName && (
                    <span className="text-xs text-muted-foreground block">
                      {s.classroomName}
                    </span>
                  )}
                </button>
              ))
            ) : (
              <p className="text-xs text-muted-foreground">{t("noLinkedStudents")}</p>
            )}
          </div>
        )}

        {listError ? (
          <div className="flex-1 flex flex-col items-center justify-center gap-3 p-6 text-center">
            <AlertCircle className="h-8 w-8 text-destructive" />
            <p className="text-sm text-muted-foreground">{t("error")}</p>
            <Button size="sm" variant="outline" onClick={() => void refetchList()}>
              <RefreshCw className="h-4 w-4 me-1" />
              {t("retry")}
            </Button>
          </div>
        ) : loadingList ? (
          <ListSkeleton />
        ) : conversations && conversations.length > 0 ? (
          <ul className="flex-1 overflow-y-auto">
            {conversations.map((c) => (
              <ConversationRow
                key={c.id}
                item={c}
                active={c.id === activeId}
                otherLabel={otherLabel(c)}
                t={t}
                onClick={() => {
                  setActiveId(c.id);
                  setMessageSearch("");
                  setDebouncedMsgSearch("");
                  setOlderMessages([]);
                }}
              />
            ))}
          </ul>
        ) : (
          <div className="p-6 text-sm text-muted-foreground text-center">
            {t("noConversations")}
          </div>
        )}
      </aside>

      <section
        className={cn(
          "flex-1 flex flex-col min-w-0 bg-background",
          !activeId && "hidden md:flex"
        )}
      >
        {activeId && activeConversation ? (
          <>
            <header className="px-4 py-3 border-b flex items-center gap-3 bg-[#075e54] text-white dark:bg-[#1f2c34]">
              <Button
                variant="ghost"
                size="sm"
                className="md:hidden shrink-0"
                onClick={() => setActiveId(null)}
              >
                ←
              </Button>
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2">
                  <p className="font-semibold truncate text-white">
                    {activeConversation.studentName}
                  </p>
                  <OnlineBadge
                    online={activeConversation.otherParticipantOnline}
                    t={t}
                    variant="header"
                  />
                </div>
                <p className="text-xs text-white/80 truncate">
                  {otherLabel(activeConversation)}
                </p>
              </div>
            </header>

            <div className="px-4 py-2 border-b bg-[#f0f2f5] dark:bg-muted/20">
              <div className="relative">
                <Search className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  className="ps-9 h-9"
                  placeholder={t("searchMessages")}
                  value={messageSearch}
                  onChange={(e) => setMessageSearch(e.target.value)}
                />
              </div>
            </div>

            <div
              className="flex-1 overflow-y-auto p-4 space-y-2 bg-[#e5ddd5] dark:bg-[#0b141a]"
              style={{
                backgroundImage:
                  "url(\"data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23d4cdc4' fill-opacity='0.35'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E\")",
              }}
            >
              {hasMore && (
                <div className="text-center">
                  <Button
                    size="sm"
                    variant="ghost"
                    disabled={loadingOlder}
                    onClick={() => void loadOlder()}
                  >
                    {loadingOlder ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      t("loadOlder")
                    )}
                  </Button>
                </div>
              )}
              {messagesError ? (
                <div className="flex flex-col items-center gap-2 py-8">
                  <AlertCircle className="h-6 w-6 text-destructive" />
                  <Button size="sm" variant="outline" onClick={() => void refetchMessages()}>
                    {t("retry")}
                  </Button>
                </div>
              ) : loadingMessages ? (
                <MessageSkeleton />
              ) : messages.length > 0 ? (
                messages.map((m) => (
                  <MessageBubble key={m.id} message={m} t={t} role={role} />
                ))
              ) : (
                <p className="text-center text-sm text-muted-foreground py-8">
                  {t("noMessages")}
                </p>
              )}
              <div ref={bottomRef} />
            </div>

            {files.length > 0 && (
              <div className="px-4 py-2 border-t flex flex-wrap gap-2 bg-card">
                {files.map((f, i) => (
                  <AttachmentPreview
                    key={`${f.name}-${i}`}
                    file={f}
                    previewUrl={filePreviews[i]}
                    onRemove={() => removeFile(i)}
                  />
                ))}
              </div>
            )}

            {uploadProgress !== null && (
              <div className="px-4 py-1 bg-card">
                <div className="h-1.5 bg-muted rounded-full overflow-hidden">
                  <div
                    className="h-full bg-primary transition-all duration-300"
                    style={{ width: `${uploadProgress}%` }}
                  />
                </div>
                <p className="text-[10px] text-muted-foreground mt-0.5 text-end">
                  {uploadProgress}%
                </p>
              </div>
            )}

            <form
              className="p-3 border-t flex gap-2 items-end bg-[#f0f2f5] dark:bg-card"
              onSubmit={(e) => {
                e.preventDefault();
                if (!draft.trim() && files.length === 0) return;
                sendMutation.mutate();
              }}
            >
              <label className="cursor-pointer p-2 rounded-full hover:bg-muted shrink-0">
                <Paperclip className="h-5 w-5 text-muted-foreground" />
                <input
                  type="file"
                  className="hidden"
                  multiple
                  accept="image/jpeg,image/png,image/webp,video/mp4,video/quicktime,video/webm,.pdf,.doc,.docx,.xls,.xlsx,application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                  onChange={onFilePick}
                />
              </label>
              <Input
                className="flex-1"
                placeholder={t("typeMessage")}
                value={draft}
                onChange={(e) => setDraft(e.target.value)}
              />
              <Button
                type="submit"
                size="icon"
                disabled={sendMutation.isPending}
                className="rounded-full bg-green-600 hover:bg-green-700 shrink-0"
              >
                {sendMutation.isPending ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <Send className="h-4 w-4" />
                )}
              </Button>
            </form>
          </>
        ) : (
          <div className="flex-1 flex items-center justify-center text-muted-foreground text-sm p-6 text-center">
            {t("selectConversation")}
          </div>
        )}
      </section>
    </div>
  );
}

function OnlineBadge({
  online,
  t,
  variant = "list",
}: {
  online: boolean;
  t: ReturnType<typeof useTranslations<"messages">>;
  variant?: "list" | "header";
}) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1 text-[10px] rounded-full px-1.5 py-0.5",
        variant === "header"
          ? online
            ? "text-white/90"
            : "text-white/60"
          : online
            ? "bg-green-100 text-green-800 dark:bg-green-900/50 dark:text-green-300"
            : "bg-muted text-muted-foreground"
      )}
      title={online ? t("online") : t("offline")}
    >
      <span
        className={cn(
          "h-1.5 w-1.5 rounded-full",
          online ? "bg-green-500" : "bg-muted-foreground"
        )}
      />
      {online ? t("online") : t("offline")}
    </span>
  );
}

function ConversationRow({
  item,
  active,
  otherLabel,
  onClick,
  t,
}: {
  item: ConversationListItem;
  active: boolean;
  otherLabel: string;
  onClick: () => void;
  t: ReturnType<typeof useTranslations<"messages">>;
}) {
  return (
    <li>
      <button
        type="button"
        onClick={onClick}
        className={cn(
          "w-full text-start px-4 py-3 border-b border-[#e9edef] hover:bg-[#f5f6f6] dark:hover:bg-muted/50 transition-colors",
          active && "bg-[#f0f2f5] dark:bg-muted"
        )}
      >
        <div className="flex justify-between gap-2 items-start">
          <div className="min-w-0 flex-1">
            <span className="font-medium truncate block">{item.studentName}</span>
            <span className="text-xs text-muted-foreground truncate block">
              {otherLabel}
            </span>
          </div>
          <div className="flex flex-col items-end gap-1 shrink-0">
            {item.lastMessageAt && (
              <span className="text-[10px] text-muted-foreground">
                {new Date(item.lastMessageAt).toLocaleDateString(undefined, {
                  month: "short",
                  day: "numeric",
                })}
              </span>
            )}
            {item.unreadCount > 0 && (
              <span className="bg-green-600 text-white text-xs rounded-full px-1.5 min-w-[1.25rem] text-center">
                {item.unreadCount}
              </span>
            )}
          </div>
        </div>
        <p className="text-xs text-muted-foreground truncate mt-1">
          {item.lastMessagePreview || t("noMessages")}
        </p>
      </button>
    </li>
  );
}

function AttachmentPreview({
  file,
  previewUrl,
  onRemove,
}: {
  file: File;
  previewUrl?: string;
  onRemove: () => void;
}) {
  return (
    <div className="relative rounded-lg border bg-muted/50 overflow-hidden max-w-[120px]">
      {previewUrl ? (
        // eslint-disable-next-line @next/next/no-img-element
        <img src={previewUrl} alt="" className="h-20 w-full object-cover" />
      ) : (
        <div className="h-20 flex items-center justify-center px-2">
          <FileText className="h-6 w-6 text-muted-foreground" />
        </div>
      )}
      <p className="text-[10px] truncate px-1 py-0.5">{file.name}</p>
      <button
        type="button"
        onClick={onRemove}
        className="absolute top-0.5 end-0.5 rounded-full bg-background/90 p-0.5"
      >
        <X className="h-3 w-3" />
      </button>
    </div>
  );
}

function MessageBubble({
  message: m,
  t,
  role,
}: {
  message: ChatMessage;
  t: ReturnType<typeof useTranslations<"messages">>;
  role: "teacher" | "parent";
}) {
  const isOwn = m.isOwnMessage;
  const showApprovalState = role === "parent" && isOwn;

  return (
    <div className={cn("flex", isOwn ? "justify-end" : "justify-start")}>
      <div
        className={cn(
          "max-w-[85%] sm:max-w-[70%] rounded-lg px-3 py-1.5 shadow-md relative",
          isOwn
            ? "bg-[#d9fdd3] dark:bg-green-900/50 rounded-ee-sm"
            : "bg-white dark:bg-[#202c33] rounded-es-sm"
        )}
      >
        {!isOwn && (
          <p className="text-xs font-semibold text-[#075e54] dark:text-green-400 mb-0.5">
            {m.senderName}
          </p>
        )}
        {showApprovalState && <StatusBadge message={m} t={t} />}
        {m.content && (
          <p className="text-sm whitespace-pre-wrap break-words">{m.content}</p>
        )}
        {m.attachments.map((a) => (
          <AttachmentLink key={a.id} attachment={a} />
        ))}
        <div
          className={cn(
            "flex items-center gap-1 mt-1 text-[10px] text-muted-foreground",
            isOwn && "justify-end"
          )}
        >
          <span>
            {new Date(m.sentAt).toLocaleString(undefined, {
              hour: "2-digit",
              minute: "2-digit",
              month: "short",
              day: "numeric",
            })}
          </span>
          {isOwn && (
            <DeliveryIcon
              status={m.deliveryStatus}
              moderation={showApprovalState ? m.moderationStatus : "Approved"}
            />
          )}
        </div>
      </div>
    </div>
  );
}

function StatusBadge({
  message: m,
  t,
}: {
  message: ChatMessage;
  t: ReturnType<typeof useTranslations<"messages">>;
}) {
  if (m.moderationStatus === "Pending") {
    return (
      <p className="text-xs text-amber-600 dark:text-amber-400 flex items-center gap-1 mb-1">
        <Clock className="h-3 w-3" />
        {t("statusPending")}
      </p>
    );
  }
  if (m.moderationStatus === "Rejected") {
    return (
      <div className="text-xs text-destructive mb-1 space-y-0.5">
        <p>{t("statusRejected")}</p>
        {m.rejectionReason && (
          <p className="opacity-90">{m.rejectionReason}</p>
        )}
      </div>
    );
  }
  return null;
}

function DeliveryIcon({
  status,
  moderation,
}: {
  status: string;
  moderation: string;
}) {
  if (moderation === "Pending") {
    return <Clock className="h-3.5 w-3.5 text-amber-500" />;
  }
  if (status === "Read") {
    return <CheckCheck className="h-3.5 w-3.5 text-blue-500" />;
  }
  if (status === "Delivered") {
    return <CheckCheck className="h-3.5 w-3.5" />;
  }
  return <Check className="h-3.5 w-3.5" />;
}

function AttachmentLink({
  attachment: a,
}: {
  attachment: ChatMessage["attachments"][0];
}) {
  const href = messagingAttachmentUrl(a.downloadUrl);
  const isImage = a.mimeType.startsWith("image/");

  if (isImage) {
    return (
      <a
        href={href}
        target="_blank"
        rel="noopener noreferrer"
        className="block mt-2 rounded-lg overflow-hidden max-w-[220px]"
      >
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img
          src={href}
          alt={a.fileName}
          className="max-h-48 w-full object-cover"
          loading="lazy"
        />
      </a>
    );
  }

  return (
    <a
      href={href}
      target="_blank"
      rel="noopener noreferrer"
      className="flex items-center gap-2 mt-2 text-sm text-primary underline"
    >
      {a.mimeType.startsWith("video/") ? (
        <Video className="h-4 w-4 shrink-0" />
      ) : (
        <FileText className="h-4 w-4 shrink-0" />
      )}
      {a.fileName}
    </a>
  );
}
