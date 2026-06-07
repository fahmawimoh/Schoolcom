"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useTranslations, useLocale } from "next-intl";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { useAuthStore } from "@/lib/auth/store";
import {
  registerSessionExpiredHandler,
  resetSessionExpiryInProgress,
} from "@/lib/auth/session-events";
import {
  SESSION_WARNING_1_MIN_MS,
  SESSION_WARNING_5_MIN_MS,
} from "@/lib/auth/session";

type AuthMessageKey =
  | "sessionExpired"
  | "sessionExpireIn5Minutes"
  | "sessionExpireIn1Minute"
  | "loginAgain";

function resolveAuthMessage(
  t: ReturnType<typeof useTranslations<"auth">>,
  key: AuthMessageKey
): string {
  return t(key);
}

export function SessionProvider({ children }: { children: React.ReactNode }) {
  const t = useTranslations("auth");
  const locale = useLocale();
  const router = useRouter();
  const { isAuthenticated, sessionExpiresAt, clearAuth } = useAuthStore();

  const [expiredOpen, setExpiredOpen] = useState(false);
  const [expiredMessageKey, setExpiredMessageKey] =
    useState<AuthMessageKey>("sessionExpired");
  const warned5Min = useRef(false);
  const warned1Min = useRef(false);
  const expiryHandled = useRef(false);

  const handleSessionExpired = useCallback(
    (translationKey?: string) => {
      if (expiryHandled.current) return;
      expiryHandled.current = true;

      const key = translationKey?.replace("auth.", "") as AuthMessageKey | undefined;
      setExpiredMessageKey(
        key === "sessionExpired" ? "sessionExpired" : "sessionExpired"
      );
      clearAuth();
      setExpiredOpen(true);
    },
    [clearAuth]
  );

  useEffect(() => {
    return registerSessionExpiredHandler(handleSessionExpired);
  }, [handleSessionExpired]);

  useEffect(() => {
    if (
      isAuthenticated &&
      sessionExpiresAt &&
      sessionExpiresAt <= Date.now()
    ) {
      handleSessionExpired("auth.sessionExpired");
    }
  }, [isAuthenticated, sessionExpiresAt, handleSessionExpired]);

  useEffect(() => {
    if (!isAuthenticated || !sessionExpiresAt) {
      warned5Min.current = false;
      warned1Min.current = false;
      expiryHandled.current = false;
      return;
    }

    const tick = () => {
      const remaining = sessionExpiresAt - Date.now();

      if (remaining <= 0) {
        handleSessionExpired("auth.sessionExpired");
        return;
      }

      if (remaining <= SESSION_WARNING_5_MIN_MS && !warned5Min.current) {
        warned5Min.current = true;
        toast.warning(t("sessionExpireIn5Minutes"), { duration: 8000 });
      }

      if (remaining <= SESSION_WARNING_1_MIN_MS && !warned1Min.current) {
        warned1Min.current = true;
        toast.warning(t("sessionExpireIn1Minute"), { duration: 8000 });
      }
    };

    tick();
    const interval = setInterval(tick, 1000);
    return () => clearInterval(interval);
  }, [isAuthenticated, sessionExpiresAt, handleSessionExpired, t]);

  const onExpiredConfirm = () => {
    setExpiredOpen(false);
    resetSessionExpiryInProgress();
    router.replace(`/${locale}/login`);
  };

  return (
    <>
      {children}
      <Dialog open={expiredOpen} onOpenChange={() => {}}>
        <DialogContent
          className="sm:max-w-md"
          onPointerDownOutside={(e) => e.preventDefault()}
          onEscapeKeyDown={(e) => e.preventDefault()}
        >
          <DialogHeader>
            <DialogTitle>{t("sessionExpiredTitle")}</DialogTitle>
            <DialogDescription>
              {resolveAuthMessage(t, expiredMessageKey)}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button onClick={onExpiredConfirm} className="w-full sm:w-auto">
              {t("loginAgain")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
