"use client";

import { useEffect } from "react";
import { useLocale } from "next-intl";
import { useRouter } from "next/navigation";
import { getDashboardPath } from "@/lib/api/client";
import { useAuthStore } from "@/lib/auth/store";

export function useRequireChatRole(role: "teacher" | "parent") {
  const router = useRouter();
  const locale = useLocale();
  const { user, authReady, isAuthenticated } = useAuthStore();

  useEffect(() => {
    if (!authReady) return;

    if (!isAuthenticated || !user) {
      router.replace(`/${locale}/login`);
      return;
    }

    const required = role === "teacher" ? "Teacher" : "Parent";
    if (!user.roles.includes(required)) {
      router.replace(getDashboardPath(user.roles, locale));
    }
  }, [authReady, isAuthenticated, user, role, router, locale]);
}
