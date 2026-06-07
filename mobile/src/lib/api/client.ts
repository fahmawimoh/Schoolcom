import type { LoginResponse } from "./types";
import {
  loginWithEmail,
  requestOtp,
  verifyOtpLogin,
} from "@/src/lib/auth/demo-users";

const API_BASE = process.env.EXPO_PUBLIC_API_URL?.trim() ?? "";

async function remoteFetch<T>(
  path: string,
  options: RequestInit = {}
): Promise<T | null> {
  if (!API_BASE) return null;

  try {
    const response = await fetch(`${API_BASE}${path}`, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...(options.headers ?? {}),
      },
    });
    const body = await response.json().catch(() => ({}));
    if (!response.ok) {
      throw new Error(body?.message ?? "Request failed");
    }
    return (body.data ?? body) as T;
  } catch {
    return null;
  }
}

export const authApi = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const remote = await remoteFetch<LoginResponse>("/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
    if (remote) return remote;
    return loginWithEmail(email, password);
  },

  requestOtp: async (identifier: string) => {
    const remote = await remoteFetch<{ message: string; expiresInSeconds: number }>(
      "/auth/otp/request",
      {
        method: "POST",
        body: JSON.stringify({ identifier, channel: "sms" }),
      }
    );
    if (remote) return remote;
    return requestOtp(identifier);
  },

  verifyOtp: async (identifier: string, code: string): Promise<LoginResponse> => {
    const remote = await remoteFetch<LoginResponse>("/auth/otp/verify", {
      method: "POST",
      body: JSON.stringify({ identifier, code }),
    });
    if (remote) return remote;
    return verifyOtpLogin(identifier, code);
  },

  logout: async () => {
    await remoteFetch<void>("/auth/logout", { method: "POST" });
  },
};

export const whatsAppApi = {
  getConfig: async () => {
    const remote = await remoteFetch<{ connected: boolean; phoneNumberId?: string }>(
      "/admin/whatsapp/config"
    );
    return remote ?? { connected: true, phoneNumberId: "dev-phone-id" };
  },

  sendMessage: async (phone: string, message: string) => {
    const remote = await remoteFetch<{ messageId: string }>("/admin/whatsapp/send", {
      method: "POST",
      body: JSON.stringify({ phone, message }),
    });
    return remote ?? { messageId: `dev-${Date.now()}` };
  },

  getConversations: async () => {
    const remote = await remoteFetch<
      Array<{ id: string; phone: string; lastMessage: string; timestamp: string }>
    >("/admin/whatsapp/conversations");
    return remote ?? [];
  },
};
