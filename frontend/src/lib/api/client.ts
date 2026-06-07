import { notifySessionExpired } from "@/lib/auth/session-events";
import { useAuthStore } from "@/lib/auth/store";

export type UserRole =
  | "SuperAdmin"
  | "SchoolAdmin"
  | "Teacher"
  | "Parent";

export interface AuthUser {
  id: string;
  email?: string;
  phone?: string;
  firstName: string;
  lastName: string;
  roles: UserRole[];
  preferredLanguage: "en" | "ar";
  tenantId: string;
}

export interface AuthTokens {
  accessToken: string;
  expiresIn: number;
}

export interface LoginResponse {
  accessToken: string;
  expiresIn: number;
  user: AuthUser;
}

export interface OtpRequestResponse {
  message: string;
  expiresInSeconds: number;
}

export interface ApiErrorBody {
  success?: boolean;
  errorCode?: string;
  translationKey?: string;
  title?: string;
  message?: string;
  messageEn?: string;
  messageAr?: string;
  errors?: Array<{ message?: string; field?: string }>;
}

export class ApiRequestError extends Error {
  readonly status: number;
  readonly errorCode?: string;
  readonly translationKey?: string;

  constructor(
    message: string,
    status: number,
    errorCode?: string,
    translationKey?: string
  ) {
    super(message);
    this.name = "ApiRequestError";
    this.status = status;
    this.errorCode = errorCode;
    this.translationKey = translationKey;
  }

  get isSessionExpired(): boolean {
    return this.errorCode === "SESSION_EXPIRED";
  }
}

const API_BASE =
  process.env.NEXT_PUBLIC_API_URL ?? "/api/v1";

type ApiFetchOptions = RequestInit & {
  suppressSessionExpired?: boolean;
};

function hasActiveSession(): boolean {
  if (typeof window === "undefined") return false;
  return useAuthStore.getState().isAuthenticated;
}

export async function apiFetch<T>(
  path: string,
  options: ApiFetchOptions = {}
): Promise<T> {
  const { suppressSessionExpired, ...fetchOptions } = options;
  const headers: HeadersInit = {
    "Content-Type": "application/json",
    ...(fetchOptions.headers ?? {}),
  };

  const legacyToken =
    typeof window !== "undefined"
      ? localStorage.getItem("accessToken")
      : null;

  if (legacyToken) {
    (headers as Record<string, string>)["Authorization"] = `Bearer ${legacyToken}`;
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...fetchOptions,
    headers,
    credentials: "include",
  });

  const body = (await response.json().catch(() => ({}))) as ApiErrorBody;

  if (!response.ok) {
    const errorCode = body.errorCode;
    const translationKey = body.translationKey;
    const message =
      body?.errors?.[0]?.message ??
      body?.title ??
      body?.messageEn ??
      body?.message ??
      "Request failed";

    if (
      !suppressSessionExpired &&
      response.status === 401 &&
      (errorCode === "SESSION_EXPIRED" ||
        (hasActiveSession() && errorCode !== "AUTH_INVALID_CREDENTIALS" &&
          errorCode !== "AUTH_INVALID_OTP" &&
          errorCode !== "AUTH_USER_NOT_FOUND"))
    ) {
      notifySessionExpired(translationKey ?? "auth.sessionExpired");
    }

    throw new ApiRequestError(
      message,
      response.status,
      errorCode,
      translationKey
    );
  }

  return (body as { data?: T }).data ?? (body as T);
}

export const authApi = {
  login: (email: string, password: string) =>
    apiFetch<LoginResponse>("/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    }),

  requestOtp: (identifier: string, channel: "sms" | "email" = "sms") =>
    apiFetch<OtpRequestResponse>("/auth/otp/request", {
      method: "POST",
      body: JSON.stringify({ identifier, channel }),
    }),

  verifyOtp: (identifier: string, code: string) =>
    apiFetch<LoginResponse>("/auth/otp/verify", {
      method: "POST",
      body: JSON.stringify({ identifier, code }),
    }),

  logout: () =>
    apiFetch<void>("/auth/logout", { method: "POST" }),

  getSession: (options?: { suppressSessionExpired?: boolean }) =>
    apiFetch<{ user: AuthUser; expiresIn: number }>("/auth/me", {
      method: "GET",
      suppressSessionExpired: options?.suppressSessionExpired,
    }),
};

export interface Student {
  id: string;
  studentNumber: string;
  firstNameEn: string;
  firstNameAr: string;
  lastNameEn: string;
  lastNameAr: string;
  fullName: string;
  schoolClassId: string;
  schoolClassNameEn: string;
  schoolClassNameAr: string;
  sectionId: string;
  sectionNameEn: string;
  sectionNameAr: string;
  parentId?: string | null;
  parentName: string;
  parentPhone: string;
  parentEmail?: string | null;
  status: "Active" | "Inactive";
  createdAt: string;
}

export interface StudentListResponse {
  items: Student[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface StudentFiltersResponse {
  schoolClasses: Array<{
    id: string;
    nameEn: string;
    nameAr: string;
  }>;
  sections: Array<{
    id: string;
    nameEn: string;
    nameAr: string;
  }>;
  parents: Array<{
    id: string;
    name: string;
  }>;
}

export interface CreateStudentPayload {
  studentNumber?: string;
  firstNameEn: string;
  firstNameAr: string;
  lastNameEn: string;
  lastNameAr: string;
  schoolClassId: string;
  sectionId: string;
  parentId?: string;
  parentName: string;
  parentPhone: string;
  parentEmail?: string;
  isActive?: boolean;
}

export interface UpdateStudentPayload {
  studentNumber?: string;
  firstNameEn: string;
  firstNameAr: string;
  lastNameEn: string;
  lastNameAr: string;
  schoolClassId: string;
  sectionId: string;
  parentId?: string;
  parentName: string;
  parentPhone: string;
  parentEmail?: string;
  isActive: boolean;
}

function buildQuery(params: Record<string, string | number | undefined>) {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") search.set(key, String(value));
  }
  const qs = search.toString();
  return qs ? `?${qs}` : "";
}

export const studentsApi = {
  list: (params: {
    page?: number;
    pageSize?: number;
    schoolClassId?: string;
    sectionId?: string;
    search?: string;
    lang?: string;
  } = {}) =>
    apiFetch<StudentListResponse>(
      `/admin/students${buildQuery({
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 5,
        schoolClassId: params.schoolClassId,
        sectionId: params.sectionId,
        search: params.search,
        lang: params.lang,
      })}`
    ),

  getFilters: () =>
    apiFetch<StudentFiltersResponse>("/admin/students/filters"),

  getById: (id: string) => apiFetch<Student>(`/admin/students/${id}`),

  create: (payload: CreateStudentPayload) =>
    apiFetch<Student>("/admin/students", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  update: (id: string, payload: UpdateStudentPayload) =>
    apiFetch<Student>(`/admin/students/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  delete: (id: string) =>
    apiFetch<void>(`/admin/students/${id}`, { method: "DELETE" }),
};

export const whatsAppApi = {
  getConfig: () =>
    apiFetch<{ connected: boolean; phoneNumberId?: string }>(
      "/admin/whatsapp/config"
    ),

  sendMessage: (phone: string, message: string) =>
    apiFetch<{ messageId: string }>("/admin/whatsapp/send", {
      method: "POST",
      body: JSON.stringify({ phone, message }),
    }),

  getConversations: () =>
    apiFetch<
      Array<{
        id: string;
        phone: string;
        lastMessage: string;
        timestamp: string;
      }>
    >("/admin/whatsapp/conversations"),
};

export function getDashboardPath(roles: UserRole[], locale: string): string {
  if (roles.includes("SuperAdmin") || roles.includes("SchoolAdmin")) {
    return `/${locale}/admin`;
  }
  if (roles.includes("Teacher")) {
    return `/${locale}/teacher`;
  }
  return `/${locale}/parent`;
}

export function getAuthErrorMessage(
  err: unknown,
  t: (key: string) => string
): string {
  if (err instanceof ApiRequestError && err.translationKey) {
    const key = err.translationKey.replace(/^auth\./, "");
    try {
      return t(key);
    } catch {
      return err.message;
    }
  }
  return err instanceof Error ? err.message : "Request failed";
}
