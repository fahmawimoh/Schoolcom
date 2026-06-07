"use client";

import { useCallback, useEffect, useRef } from "react";
import { usePathname } from "next/navigation";
import { authApi, ApiRequestError } from "@/lib/api/client";
import { useAuthStore } from "@/lib/auth/store";
import { isTokenExpired } from "@/lib/auth/session";

const SESSION_CHECK_TIMEOUT_MS = 10_000;

function hasValidPersistedSession(
  isAuthenticated: boolean,
  sessionExpiresAt: number | null
): boolean {
  return (
    isAuthenticated &&
    sessionExpiresAt !== null &&
    !isTokenExpired(sessionExpiresAt)
  );
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const bootstrapGeneration = useRef(0);

  const { setHydrated, setAuthReady, restoreSession, clearAuth, isHydrated } =
    useAuthStore();

  const runBootstrap = useCallback(async () => {
    const generation = ++bootstrapGeneration.current;
    const isStale = () => generation !== bootstrapGeneration.current;

    setAuthReady(false);

    try {
      const session = await Promise.race([
        authApi.getSession({ suppressSessionExpired: true }),
        new Promise<never>((_, reject) => {
          setTimeout(
            () => reject(new Error("Session check timed out")),
            SESSION_CHECK_TIMEOUT_MS
          );
        }),
      ]);

      if (isStale()) return;

      restoreSession(session.user, session.expiresIn);
      if (typeof window !== "undefined") {
        localStorage.removeItem("accessToken");
      }
    } catch (error) {
      if (isStale()) return;

      const isUnauthorized =
        error instanceof ApiRequestError &&
        (error.status === 401 || error.isSessionExpired);

      if (isUnauthorized) {
        clearAuth();
        return;
      }

      const state = useAuthStore.getState();
      const localExpired =
        state.sessionExpiresAt !== null &&
        isTokenExpired(state.sessionExpiresAt);

      if (!hasValidPersistedSession(state.isAuthenticated, state.sessionExpiresAt)) {
        if (localExpired || !state.isAuthenticated) {
          clearAuth();
        }
      }
    } finally {
      if (!isStale()) {
        setAuthReady(true);
      }
    }
  }, [clearAuth, restoreSession, setAuthReady]);

  useEffect(() => {
    const finishHydration = () => setHydrated(true);

    if (useAuthStore.persist.hasHydrated()) {
      finishHydration();
    }

    return useAuthStore.persist.onFinishHydration(finishHydration);
  }, [setHydrated]);

  useEffect(() => {
    if (!isHydrated) return;

    void runBootstrap();

    return () => {
      bootstrapGeneration.current += 1;
    };
  }, [isHydrated, runBootstrap]);

  useEffect(() => {
    if (!isHydrated) return;

    const handlePageShow = (event: PageTransitionEvent) => {
      if (!event.persisted) return;
      void runBootstrap();
    };

    window.addEventListener("pageshow", handlePageShow);
    return () => window.removeEventListener("pageshow", handlePageShow);
  }, [isHydrated, runBootstrap]);

  useEffect(() => {
    if (!isHydrated) return;

    const { authReady } = useAuthStore.getState();
    if (!authReady) {
      void runBootstrap();
    }
  }, [pathname, isHydrated, runBootstrap]);

  return <>{children}</>;
}
