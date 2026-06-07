import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AuthUser } from "@/lib/api/client";
import {
  clearAllAuthStorage,
  computeSessionExpiresAt,
} from "@/lib/auth/session";

export type AuthReadyState = boolean;

interface AuthState {
  user: AuthUser | null;
  sessionExpiresAt: number | null;
  isAuthenticated: boolean;
  isHydrated: boolean;
  authReady: boolean;
  setAuth: (user: AuthUser, accessToken: string, expiresIn: number) => void;
  restoreSession: (user: AuthUser, expiresIn: number) => void;
  clearAuth: () => void;
  setHydrated: (value: boolean) => void;
  setAuthReady: (value: boolean) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      sessionExpiresAt: null,
      isAuthenticated: false,
      isHydrated: false,
      authReady: false,
      setAuth: (user, accessToken, expiresIn) => {
        const sessionExpiresAt = computeSessionExpiresAt(
          accessToken,
          expiresIn
        );
        set({
          user,
          sessionExpiresAt,
          isAuthenticated: true,
          authReady: true,
        });
      },
      restoreSession: (user, expiresIn) => {
        set({
          user,
          sessionExpiresAt: Date.now() + expiresIn * 1000,
          isAuthenticated: true,
          authReady: true,
        });
      },
      clearAuth: () => {
        clearAllAuthStorage();
        set({
          user: null,
          sessionExpiresAt: null,
          isAuthenticated: false,
          authReady: true,
        });
      },
      setHydrated: (value) => set({ isHydrated: value }),
      setAuthReady: (value) => set({ authReady: value }),
    }),
    {
      name: "schoolcom-auth",
      partialize: (state) => ({
        user: state.user,
        sessionExpiresAt: state.sessionExpiresAt,
        isAuthenticated: state.isAuthenticated,
      }),
      onRehydrateStorage: () => (state) => {
        state?.setHydrated(true);
      },
    }
  )
);
