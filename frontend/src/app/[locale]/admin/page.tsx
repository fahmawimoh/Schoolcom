"use client";

import { useTranslations } from "next-intl";
import { MessageSquare, Shield, Users, Bell } from "lucide-react";
import { DashboardShell } from "@/components/layout/dashboard-shell";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

const stats = [
  { key: "pendingMessages", icon: Shield, value: "12" },
  { key: "activeConversations", icon: MessageSquare, value: "48" },
  { key: "totalStudents", icon: Users, value: "1,240" },
  { key: "notifications", icon: Bell, value: "5" },
] as const;

export default function AdminDashboardPage() {
  const t = useTranslations("dashboard");

  return (
    <DashboardShell role="admin">
      <div className="space-y-6">
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {stats.map((stat) => (
            <Card key={stat.key}>
              <CardHeader className="flex flex-row items-center justify-between pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  {t(`stats.${stat.key}`)}
                </CardTitle>
                <stat.icon className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <p className="text-2xl font-bold">{stat.value}</p>
              </CardContent>
            </Card>
          ))}
        </div>

        <div className="grid gap-6 lg:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle>{t("quickActions")}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2 text-sm text-muted-foreground">
              <p>• Review pending moderation queue</p>
              <p>• Send WhatsApp message to parents</p>
              <p>• Export weekly activity report</p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle>{t("recentActivity")}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3 text-sm">
              <div className="flex justify-between">
                <span>Parent message approved</span>
                <span className="text-muted-foreground">2m ago</span>
              </div>
              <div className="flex justify-between">
                <span>New conversation started</span>
                <span className="text-muted-foreground">15m ago</span>
              </div>
              <div className="flex justify-between">
                <span>WhatsApp message delivered</span>
                <span className="text-muted-foreground">1h ago</span>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </DashboardShell>
  );
}
