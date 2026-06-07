"use client";

import { useEffect } from "react";
import { useTranslations, useLocale } from "next-intl";
import { useRouter } from "next/navigation";
import { GraduationCap } from "lucide-react";
import { LoginForm } from "@/components/auth/login-form";
import { AuthLoading } from "@/components/auth/auth-loading";
import {
  LanguageSwitcher,
  ThemeToggle,
} from "@/components/layout/theme-toggle";
import { useAuthStore } from "@/lib/auth/store";
import { getDashboardPath } from "@/lib/api/client";
import { isTokenExpired } from "@/lib/auth/session";

export function LoginPageContent() {
  const t = useTranslations();
  const tAuth = useTranslations("auth");
  const locale = useLocale();
  const router = useRouter();
  const { authReady, isAuthenticated, user, sessionExpiresAt } = useAuthStore();

  useEffect(() => {
    if (!authReady || !isAuthenticated || !user) return;
    if (sessionExpiresAt && isTokenExpired(sessionExpiresAt)) return;
    router.replace(getDashboardPath(user.roles, locale));
  }, [authReady, isAuthenticated, user, sessionExpiresAt, router, locale]);

  if (!authReady) {
    return <AuthLoading />;
  }

  return (
    <div className="min-h-screen flex">
      <div className="hidden lg:flex lg:w-1/2 relative overflow-hidden bg-gradient-to-br from-primary via-blue-600 to-indigo-800">
        <div className="absolute inset-0 opacity-50 bg-[radial-gradient(circle_at_30%_20%,rgba(255,255,255,0.15),transparent_50%)]" />
        <div className="relative z-10 flex flex-col justify-center px-16 text-white">
          <div className="flex items-center gap-3 mb-8">
            <div className="p-3 rounded-2xl bg-white/10 backdrop-blur-sm">
              <GraduationCap className="h-10 w-10" />
            </div>
            <div>
              <h1 className="text-3xl font-bold">{t("common.appName")}</h1>
              <p className="text-blue-100">{t("common.tagline")}</p>
            </div>
          </div>
          <blockquote className="text-lg text-blue-50 leading-relaxed max-w-md">
            &ldquo;Connecting families and educators through secure, moderated
            communication.&rdquo;
          </blockquote>
        </div>
      </div>

      <div className="flex-1 flex flex-col">
        <header className="flex items-center justify-between p-4 lg:p-6">
          <div className="flex items-center gap-2 lg:hidden">
            <GraduationCap className="h-7 w-7 text-primary" />
            <span className="font-semibold">{t("common.appName")}</span>
          </div>
          <div className="flex items-center gap-1 ms-auto">
            <LanguageSwitcher />
            <ThemeToggle />
          </div>
        </header>

        <main className="flex-1 flex items-center justify-center px-4 pb-12">
          <div className="w-full max-w-md animate-fade-in">
            <div className="mb-8 text-center lg:text-start">
              <h2 className="text-2xl font-bold tracking-tight">
                {tAuth("welcomeBack")}
              </h2>
              <p className="text-muted-foreground mt-2">
                {tAuth("signInSubtitle")}
              </p>
            </div>
            <LoginForm />
          </div>
        </main>
      </div>
    </div>
  );
}
