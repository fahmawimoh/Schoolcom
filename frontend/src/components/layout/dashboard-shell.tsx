"use client";

import { useEffect, useMemo, useState } from "react";
import { useTranslations } from "next-intl";
import { usePathname, useRouter } from "next/navigation";
import { useLocale } from "next-intl";
import {
  BookMarked,
  BookOpen,
  Building2,
  CalendarClock,
  GraduationCap,
  Layers,
  LayoutDashboard,
  Menu,
  MessageSquare,
  Phone,
  ShieldCheck,
  Users,
  X,
} from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { LogoutButton } from "@/components/auth/logout-button";
import { useAuthStore } from "@/lib/auth/store";
import { getDashboardPath } from "@/lib/api/client";
import {
  LanguageSwitcher,
  ThemeToggle,
} from "@/components/layout/theme-toggle";
import { SessionCountdown } from "@/components/auth/session-countdown";
import { AuthLoading } from "@/components/auth/auth-loading";
import { isSessionExpiryInProgress } from "@/lib/auth/session-events";
import { isTokenExpired } from "@/lib/auth/session";
import { cn } from "@/lib/utils";

interface DashboardShellProps {
  children: React.ReactNode;
  role: "admin" | "teacher" | "parent";
}

type NavItem = {
  href: string;
  icon: typeof LayoutDashboard;
  label: string;
  shortLabel?: string;
};

export function DashboardShell({ children, role }: DashboardShellProps) {
  const t = useTranslations();
  const locale = useLocale();
  const router = useRouter();
  const pathname = usePathname();
  const { user, isAuthenticated, sessionExpiresAt, authReady } =
    useAuthStore();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const items = useMemo((): NavItem[] => {
    const navItems: Record<"admin" | "teacher" | "parent", NavItem[]> = {
      admin: [
        { href: `/${locale}/admin`, icon: LayoutDashboard, label: t("nav.dashboard") },
        {
          href: `/${locale}/admin/message-approval`,
          icon: ShieldCheck,
          label: t("nav.messageApproval"),
          shortLabel: t("nav.messageApprovalShort"),
        },
        {
          href: `/${locale}/admin/whatsapp`,
          icon: Phone,
          label: t("nav.whatsappBroadcast"),
          shortLabel: t("nav.whatsappShort"),
        },
        {
          href: `/${locale}/admin/teachers`,
          icon: BookOpen,
          label: t("nav.teachers"),
        },
        { href: `/${locale}/admin/students`, icon: Users, label: t("nav.students") },
        {
          href: `/${locale}/admin/school-classes`,
          icon: GraduationCap,
          label: t("nav.schoolClasses"),
          shortLabel: t("nav.schoolClassesShort"),
        },
        {
          href: `/${locale}/admin/sections`,
          icon: Layers,
          label: t("nav.sections"),
        },
        {
          href: `/${locale}/admin/subjects`,
          icon: BookMarked,
          label: t("nav.subjects"),
        },
        {
          href: `/${locale}/admin/rooms`,
          icon: Building2,
          label: t("nav.rooms"),
        },
        {
          href: `/${locale}/admin/timetable`,
          icon: CalendarClock,
          label: t("nav.timetable"),
          shortLabel: t("nav.timetableShort"),
        },
      ],
      teacher: [
        { href: `/${locale}/teacher`, icon: LayoutDashboard, label: t("nav.dashboard") },
        {
          href: `/${locale}/teacher/conversations`,
          icon: MessageSquare,
          label: t("nav.conversations"),
          shortLabel: t("nav.conversationsShort"),
        },
      ],
      parent: [
        { href: `/${locale}/parent`, icon: LayoutDashboard, label: t("nav.dashboard") },
        {
          href: `/${locale}/parent/conversations`,
          icon: MessageSquare,
          label: t("nav.conversations"),
          shortLabel: t("nav.conversationsShort"),
        },
      ],
    };
    return navItems[role];
  }, [locale, role, t]);

  const activeNavItem = useMemo(
    () =>
      items.find(
        (item) =>
          pathname === item.href ||
          (item.href !== `/${locale}/${role}` && pathname.startsWith(item.href))
      ),
    [items, pathname, locale, role]
  );

  useEffect(() => {
    setMobileMenuOpen(false);
  }, [pathname]);

  useEffect(() => {
    if (!authReady || isSessionExpiryInProgress()) return;

    const sessionValid =
      sessionExpiresAt === null || !isTokenExpired(sessionExpiresAt);

    if (!isAuthenticated || !user || !sessionValid) {
      router.replace(`/${locale}/login`);
    }
  }, [
    authReady,
    isAuthenticated,
    user,
    sessionExpiresAt,
    router,
    locale,
  ]);

  const hasValidSession =
    isAuthenticated &&
    user &&
    (sessionExpiresAt === null || !isTokenExpired(sessionExpiresAt));

  if (!authReady) {
    return <AuthLoading />;
  }

  if (!hasValidSession) {
    return null;
  }

  const userRole = user.roles[0];
  const roleMismatch =
    (role === "admin" &&
      !user.roles.some((r) => r === "SchoolAdmin" || r === "SuperAdmin")) ||
    (role === "teacher" && userRole !== "Teacher") ||
    (role === "parent" && userRole !== "Parent");

  if (roleMismatch) {
    router.replace(getDashboardPath(user.roles, locale));
    return null;
  }

  const isNavActive = (item: NavItem) =>
    pathname === item.href ||
    (item.href !== `/${locale}/${role}` && pathname.startsWith(item.href));

  return (
    <div className="min-h-screen flex bg-background">
      <aside className="hidden md:flex w-64 flex-col border-e bg-card">
        <div className="p-6 border-b">
          <h1 className="font-bold text-lg">{t("common.appName")}</h1>
          <p className="text-xs text-muted-foreground mt-1">
            {user.roles[0] ? t(`roles.${user.roles[0]}` as "roles.Parent") : ""}
          </p>
        </div>
        <nav className="flex-1 p-4 space-y-1">
          {items.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
                isNavActive(item)
                  ? "bg-accent text-accent-foreground"
                  : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
              )}
            >
              <item.icon className="h-4 w-4" />
              {item.label}
            </Link>
          ))}
        </nav>
        <div className="p-4 border-t">
          <LogoutButton className="w-full justify-start gap-2" />
        </div>
      </aside>

      <div className="flex-1 flex flex-col min-w-0">
        <header className="flex items-center justify-between border-b bg-card/50 backdrop-blur-sm px-4 md:px-6 h-14 gap-2">
          <div className="flex items-center gap-2 min-w-0 flex-1">
            <Button
              type="button"
              variant="ghost"
              size="icon"
              className="md:hidden shrink-0"
              aria-label={t("nav.menu")}
              onClick={() => setMobileMenuOpen((open) => !open)}
            >
              {mobileMenuOpen ? (
                <X className="h-5 w-5" />
              ) : (
                <Menu className="h-5 w-5" />
              )}
            </Button>
            <div className="min-w-0">
              {activeNavItem && (
                <p className="text-sm font-semibold truncate md:hidden">
                  {activeNavItem.label}
                </p>
              )}
              <p
                className={cn(
                  "truncate text-muted-foreground",
                  activeNavItem ? "text-xs md:text-sm" : "text-sm font-medium"
                )}
              >
                {t("dashboard.welcome", {
                  name: `${user.firstName} ${user.lastName}`,
                })}
              </p>
            </div>
          </div>
          <div className="flex items-center gap-2 shrink-0">
            <SessionCountdown />
            <LogoutButton className="gap-2 h-9 px-3" />
            <LanguageSwitcher />
            <ThemeToggle />
          </div>
        </header>

        {mobileMenuOpen && (
          <>
            <button
              type="button"
              className="fixed inset-0 z-40 bg-black/40 md:hidden"
              aria-label={t("nav.closeMenu")}
              onClick={() => setMobileMenuOpen(false)}
            />
            <nav className="fixed top-14 inset-x-0 z-50 border-b bg-card shadow-lg md:hidden max-h-[min(70vh,24rem)] overflow-y-auto">
              <div className="p-2 space-y-0.5">
                {items.map((item) => (
                  <Link
                    key={item.href}
                    href={item.href}
                    onClick={() => setMobileMenuOpen(false)}
                    className={cn(
                      "flex items-center gap-3 rounded-lg px-3 py-3 text-sm font-medium",
                      isNavActive(item)
                        ? "bg-accent text-accent-foreground"
                        : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                    )}
                  >
                    <item.icon className="h-4 w-4 shrink-0" />
                    {item.label}
                  </Link>
                ))}
                <LogoutButton
                  className="w-full justify-start gap-3 px-3 py-3 h-auto text-sm font-medium text-muted-foreground"
                  onLoggedOut={() => setMobileMenuOpen(false)}
                />
              </div>
            </nav>
          </>
        )}
        <main className="flex-1 p-4 md:p-6 pb-20 md:pb-6 overflow-auto">
          {children}
        </main>
      </div>

      <nav className="md:hidden fixed bottom-0 inset-x-0 z-30 border-t bg-card/95 backdrop-blur-sm safe-area-pb">
        <div className="flex items-stretch justify-around px-1 py-1 gap-0.5 overflow-x-auto">
          {items.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex flex-1 flex-col items-center gap-0.5 rounded-lg px-1 py-2 text-[10px] font-medium min-w-[4.5rem] max-w-[6rem]",
                isNavActive(item) ? "text-primary" : "text-muted-foreground"
              )}
            >
              <item.icon className="h-5 w-5 shrink-0" />
              <span className="truncate w-full text-center">
                {item.shortLabel ?? item.label}
              </span>
            </Link>
          ))}
        </div>
      </nav>
    </div>
  );
}
