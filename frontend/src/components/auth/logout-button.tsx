"use client";

import { Loader2, LogOut } from "lucide-react";
import { useTranslations } from "next-intl";
import { Button } from "@/components/ui/button";
import { useLogout } from "@/lib/auth/use-logout";
import { useAuthStore } from "@/lib/auth/store";
import { cn } from "@/lib/utils";

type LogoutButtonProps = {
  variant?: "ghost" | "outline";
  className?: string;
  showLabel?: boolean;
  onLoggedOut?: () => void;
};

export function LogoutButton({
  variant = "ghost",
  className,
  showLabel = true,
  onLoggedOut,
}: LogoutButtonProps) {
  const t = useTranslations();
  const { logout, isLoggingOut } = useLogout();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  if (!isAuthenticated) return null;

  const handleClick = () => {
    onLoggedOut?.();
    void logout();
  };

  return (
    <Button
      type="button"
      variant={variant}
      className={cn(className)}
      onClick={handleClick}
      disabled={isLoggingOut}
      aria-busy={isLoggingOut}
    >
      {isLoggingOut ? (
        <Loader2 className="h-4 w-4 animate-spin" />
      ) : (
        <LogOut className="h-4 w-4" />
      )}
      {showLabel ? (
        <span>
          {isLoggingOut ? t("auth.loggingOut") : t("nav.logout")}
        </span>
      ) : (
        <span className="sr-only">{t("nav.logout")}</span>
      )}
    </Button>
  );
}
