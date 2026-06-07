"use client";

import { useEffect, useState } from "react";
import { useLocale, useTranslations } from "next-intl";
import { Clock } from "lucide-react";
import { useAuthStore } from "@/lib/auth/store";
import { formatRemainingTime } from "@/lib/auth/session";

export function SessionCountdown() {
  const t = useTranslations("auth");
  const locale = useLocale();
  const sessionExpiresAt = useAuthStore((s) => s.sessionExpiresAt);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const [remainingMs, setRemainingMs] = useState<number | null>(null);

  useEffect(() => {
    if (!isAuthenticated || !sessionExpiresAt) {
      setRemainingMs(null);
      return;
    }

    const update = () => {
      setRemainingMs(Math.max(0, sessionExpiresAt - Date.now()));
    };

    update();
    const interval = setInterval(update, 1000);
    return () => clearInterval(interval);
  }, [isAuthenticated, sessionExpiresAt]);

  if (remainingMs === null || remainingMs <= 0) return null;

  const formattedTime = formatRemainingTime(remainingMs, locale);
  const label = t("sessionRemaining", { time: formattedTime });

  return (
    <div
      className="hidden sm:flex items-center gap-1.5 text-xs text-muted-foreground tabular-nums"
      title={label}
    >
      <Clock className="h-3.5 w-3.5 shrink-0" />
      <span>{label}</span>
    </div>
  );
}
