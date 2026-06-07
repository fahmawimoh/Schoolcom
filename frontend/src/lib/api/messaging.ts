import { apiFetch, ApiRequestError } from "@/lib/api/client";

export interface ConversationListItem {
  id: string;
  studentId: string;
  studentName: string;
  parentName: string;
  teacherName: string;
  otherParticipantId?: string | null;
  subject: string;
  lastMessagePreview?: string | null;
  lastMessageAt?: string | null;
  unreadCount: number;
  otherParticipantOnline: boolean;
}

export interface LinkedStudent {
  id: string;
  fullName: string;
  classroomName?: string | null;
  hasConversation: boolean;
}

export interface MessageAttachment {
  id: string;
  fileName: string;
  mimeType: string;
  sizeBytes: number;
  downloadUrl: string;
}

export interface ChatMessage {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  content: string;
  messageType: string;
  moderationStatus: string;
  deliveryStatus: string;
  sentAt: string;
  attachments: MessageAttachment[];
  isOwnMessage: boolean;
  rejectionReason?: string | null;
}

export interface MessageListResult {
  items: ChatMessage[];
  nextCursor?: string | null;
  hasMore: boolean;
}

export interface PendingModeration {
  approvalRequestId: string;
  messageId: string;
  conversationId: string;
  studentId: string;
  parentUserId: string;
  teacherUserId: string;
  studentName: string;
  senderName: string;
  teacherName: string;
  content: string;
  messageType: string;
  sentAt: string;
  attachments: MessageAttachment[];
}

export interface ModerationHistoryItem {
  messageId: string;
  studentName: string;
  senderName: string;
  action: string;
  notes?: string | null;
  reviewerName: string;
  reviewedAt: string;
}

export interface ModerationFilters {
  studentId?: string;
  parentUserId?: string;
  teacherUserId?: string;
  status?: string;
  fromDate?: string;
  toDate?: string;
}

export interface UserNotification {
  id: string;
  type: string;
  title: string;
  body: string;
  referenceId?: string | null;
  isRead: boolean;
  createdAt: string;
}

/** BFF path for attachment downloads returned by the API as `/v1/messaging/...`. */
export function messagingAttachmentUrl(downloadUrl: string): string {
  const path = downloadUrl.startsWith("/v1/")
    ? downloadUrl.slice(3)
    : downloadUrl;
  return `/api/v1${path.startsWith("/") ? path : `/${path}`}`;
}

export const FILE_LIMITS = {
  image: 2 * 1024 * 1024,
  video: 20 * 1024 * 1024,
  document: 3 * 1024 * 1024,
} as const;

const ALLOWED_IMAGE = ["image/jpeg", "image/jpg", "image/png", "image/webp"];
const ALLOWED_VIDEO = ["video/mp4", "video/quicktime", "video/webm"];
const ALLOWED_DOC = [
  "application/pdf",
  "application/msword",
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
  "application/vnd.ms-excel",
  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
];

export function validateFileType(file: File): string | null {
  const mime = file.type;
  const allowed = [
    ...ALLOWED_IMAGE,
    ...ALLOWED_VIDEO,
    ...ALLOWED_DOC,
  ];
  if (!allowed.includes(mime)) {
    return `File type "${mime || file.name}" is not allowed.`;
  }
  return null;
}

export function validateFileSize(file: File): string | null {
  const mime = file.type;
  let max = FILE_LIMITS.document;
  let label = "3 MB";

  if (mime.startsWith("image/")) {
    max = FILE_LIMITS.image;
    label = "2 MB";
  } else if (mime.startsWith("video/")) {
    max = FILE_LIMITS.video;
    label = "20 MB";
  }

  if (file.size > max) {
    return `File exceeds maximum size of ${label}.`;
  }
  return null;
}

function buildModerationQuery(filters?: ModerationFilters, limit?: number): string {
  const params = new URLSearchParams();
  if (limit) params.set("limit", String(limit));
  if (filters?.studentId) params.set("studentId", filters.studentId);
  if (filters?.parentUserId) params.set("parentUserId", filters.parentUserId);
  if (filters?.teacherUserId) params.set("teacherUserId", filters.teacherUserId);
  if (filters?.status) params.set("status", filters.status);
  if (filters?.fromDate) params.set("fromDate", filters.fromDate);
  if (filters?.toDate) params.set("toDate", filters.toDate);
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export const messagingApi = {
  getConversations: (search?: string) => {
    const qs = search ? `?search=${encodeURIComponent(search)}` : "";
    return apiFetch<ConversationListItem[]>(`/messaging/conversations${qs}`);
  },

  getLinkedStudents: () =>
    apiFetch<LinkedStudent[]>("/messaging/linked-students"),

  ensureConversation: (studentId: string) =>
    apiFetch<ConversationListItem>("/messaging/conversations/ensure", {
      method: "POST",
      body: JSON.stringify({ studentId }),
    }),

  getMessages: (
    conversationId: string,
    cursor?: string,
    search?: string
  ) => {
    const params = new URLSearchParams();
    if (cursor) params.set("cursor", cursor);
    if (search) params.set("search", search);
    const qs = params.toString() ? `?${params}` : "";
    return apiFetch<MessageListResult>(
      `/messaging/conversations/${conversationId}/messages${qs}`
    );
  },

  markRead: (conversationId: string) =>
    apiFetch<void>(`/messaging/conversations/${conversationId}/read`, {
      method: "POST",
    }),

  sendMessage: async (
    conversationId: string,
    content: string,
    files: File[],
    onProgress?: (pct: number) => void
  ) => {
    const form = new FormData();
    form.append("content", content);
    const messageType =
      files[0]?.type.startsWith("image/")
        ? "Image"
        : files[0]?.type.startsWith("video/")
          ? "Video"
          : files.length > 0
            ? "Document"
            : "Text";
    form.append("messageType", messageType);
    for (const file of files) {
      const typeErr = validateFileType(file);
      if (typeErr) throw new ApiRequestError(typeErr, 400);
      const err = validateFileSize(file);
      if (err) throw new ApiRequestError(err, 400);
      form.append("files", file, file.name);
    }

    const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "/api/v1";
    const legacyToken =
      typeof window !== "undefined"
        ? localStorage.getItem("accessToken")
        : null;
    const headers: HeadersInit = {};
    if (legacyToken) headers["Authorization"] = `Bearer ${legacyToken}`;

    const xhr = new XMLHttpRequest();
    return new Promise<{ message: ChatMessage; requiresApproval: boolean }>(
      (resolve, reject) => {
        xhr.upload.onprogress = (e) => {
          if (e.lengthComputable && onProgress)
            onProgress(Math.round((e.loaded / e.total) * 100));
        };
        xhr.onload = () => {
          try {
            const body = JSON.parse(xhr.responseText);
            if (xhr.status >= 400) {
              reject(
                new ApiRequestError(
                  body?.message ?? body?.title ?? "Send failed",
                  xhr.status
                )
              );
              return;
            }
            const data = body.data ?? body;
            resolve(data);
          } catch {
            reject(new Error("Invalid response"));
          }
        };
        xhr.onerror = () => reject(new Error("Network error"));
        xhr.open(
          "POST",
          `${API_BASE}/messaging/conversations/${conversationId}/messages`
        );
        xhr.withCredentials = true;
        if (legacyToken) xhr.setRequestHeader("Authorization", `Bearer ${legacyToken}`);
        xhr.send(form);
      }
    );
  },

  getNotifications: () => apiFetch<UserNotification[]>("/messaging/notifications"),
};

export const moderationApi = {
  getPending: (filters?: ModerationFilters) =>
    apiFetch<PendingModeration[]>(
      `/admin/moderation/pending${buildModerationQuery(filters)}`
    ),

  getHistory: (filters?: ModerationFilters, limit = 50) =>
    apiFetch<ModerationHistoryItem[]>(
      `/admin/moderation/history${buildModerationQuery(filters, limit)}`
    ),

  approve: (messageId: string, notes?: string) =>
    apiFetch<void>(`/admin/moderation/messages/${messageId}/approve`, {
      method: "POST",
      body: JSON.stringify({ notes }),
    }),

  reject: (messageId: string, notes?: string) =>
    apiFetch<void>(`/admin/moderation/messages/${messageId}/reject`, {
      method: "POST",
      body: JSON.stringify({ notes }),
    }),
};
