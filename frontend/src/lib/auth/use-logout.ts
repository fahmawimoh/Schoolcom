"use client";

import { useCallback, useState } from "react";
import { useLocale, useTranslations } from "next-intl";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { authApi } from "@/lib/api/client";
import { useAuthStore } from "@/lib/auth/store";

export function useLogout() {
  const t = useTranslations();
  const locale = useLocale();
  const router = useRouter();
  const clearAuth = useAuthStore((state) => state.clearAuth);
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  const logout = useCallback(async () => {
    if (isLoggingOut) return;

    setIsLoggingOut(true);
    try {
      await authApi.logout();
    } catch {
      /* Always clear local session even if the API is unreachable */
    } finally {
      clearAuth();
      router.replace(`/${locale}/login`);
      toast.success(t("auth.logoutSuccess"));
      setIsLoggingOut(false);
    }
  }, [clearAuth, isLoggingOut, locale, router, t]);

  return { logout, isLoggingOut };
}
