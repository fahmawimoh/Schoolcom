"use client";

import { Loader2 } from "lucide-react";
import { useTranslations } from "next-intl";

export function AuthLoading() {
  const t = useTranslations("common");

  return (
    <div className="min-h-screen flex flex-col items-center justify-center gap-3 bg-background">
      <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      <p className="text-sm text-muted-foreground">{t("loading")}</p>
    </div>
  );
}
