"use client";

import { useEffect } from "react";
import { useLocale } from "next-intl";
import { useRouter } from "next/navigation";
import { getDashboardPath, type UserRole } from "@/lib/api/client";
import { useAuthStore } from "@/lib/auth/store";

const ADMIN_ROLES: UserRole[] = ["SchoolAdmin", "SuperAdmin"];

export function useRequireAdminRole() {
  const router = useRouter();
  const locale = useLocale();
  const { user, authReady, isAuthenticated } = useAuthStore();

  const isAdmin =
    user?.roles.some((r) => ADMIN_ROLES.includes(r)) ?? false;

  useEffect(() => {
    if (!authReady) return;

    if (!isAuthenticated || !user) {
      router.replace(`/${locale}/login`);
      return;
    }

    if (!isAdmin) {
      router.replace(getDashboardPath(user.roles, locale));
    }
  }, [authReady, isAuthenticated, user, isAdmin, router, locale]);

  return { isAdmin, user };
}
