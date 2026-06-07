"use client";

import { ShieldAlert } from "lucide-react";
import { useTranslations } from "next-intl";
import { DashboardShell } from "@/components/layout/dashboard-shell";
import { MessageApprovalWorkspace } from "@/components/admin/message-approval-workspace";
import { useRequireAdminRole } from "@/lib/auth/require-admin-role";
import {
  Card,
  CardContent,
} from "@/components/ui/card";

export default function AdminMessageApprovalPage() {
  const t = useTranslations("messageApproval");
  const { isAdmin } = useRequireAdminRole();

  if (!isAdmin) {
    return (
      <DashboardShell role="admin">
        <Card className="max-w-lg mx-auto mt-12">
          <CardContent className="flex flex-col items-center gap-4 py-12">
            <ShieldAlert className="h-12 w-12 text-destructive" />
            <p className="text-center text-muted-foreground">{t("adminOnly")}</p>
          </CardContent>
        </Card>
      </DashboardShell>
    );
  }

  return (
    <DashboardShell role="admin">
      <MessageApprovalWorkspace />
    </DashboardShell>
  );
}
