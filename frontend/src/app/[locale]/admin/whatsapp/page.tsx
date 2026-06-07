"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useTranslations } from "next-intl";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { MessageCircle, Send, Loader2, ShieldAlert } from "lucide-react";
import { toast } from "sonner";
import { DashboardShell } from "@/components/layout/dashboard-shell";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { whatsAppApi } from "@/lib/api/client";
import {
  whatsAppSendSchema,
  type WhatsAppSendForm,
} from "@/lib/validations/auth";
import { useAuthStore } from "@/lib/auth/store";

export default function AdminWhatsAppPage() {
  const t = useTranslations("whatsapp");
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();
  const [isLoading, setIsLoading] = useState(false);

  const isAdmin =
    user?.roles.includes("SchoolAdmin") ||
    user?.roles.includes("SuperAdmin");

  const { data: config } = useQuery({
    queryKey: ["whatsapp-config"],
    queryFn: () => whatsAppApi.getConfig(),
    enabled: isAdmin,
  });

  const { data: conversations } = useQuery({
    queryKey: ["whatsapp-conversations"],
    queryFn: () => whatsAppApi.getConversations(),
    enabled: isAdmin,
  });

  const form = useForm<WhatsAppSendForm>({
    resolver: zodResolver(whatsAppSendSchema),
    defaultValues: { phone: "", message: "" },
  });

  const sendMutation = useMutation({
    mutationFn: (data: WhatsAppSendForm) =>
      whatsAppApi.sendMessage(data.phone, data.message),
    onSuccess: () => {
      toast.success(t("sent"));
      form.reset();
      queryClient.invalidateQueries({ queryKey: ["whatsapp-conversations"] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

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

  const onSubmit = (data: WhatsAppSendForm) => {
    sendMutation.mutate(data);
  };

  return (
    <DashboardShell role="admin">
      <div className="max-w-4xl mx-auto space-y-6">
        <div>
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <MessageCircle className="h-7 w-7 text-green-600" />
            {t("title")}
          </h1>
          <p className="text-muted-foreground mt-1">{t("subtitle")}</p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">
              {config?.connected ? t("connected") : t("disconnected")}
            </CardTitle>
            <CardDescription>
              {config?.phoneNumberId ?? t("configure")}
            </CardDescription>
          </CardHeader>
        </Card>

        <div className="grid gap-6 lg:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle>{t("sendMessage")}</CardTitle>
            </CardHeader>
            <CardContent>
              <form
                onSubmit={form.handleSubmit(onSubmit)}
                className="space-y-4"
              >
                <div className="space-y-2">
                  <Label htmlFor="wa-phone">{t("recipientPhone")}</Label>
                  <Input
                    id="wa-phone"
                    type="tel"
                    dir="ltr"
                    placeholder="+966501234567"
                    {...form.register("phone")}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="wa-message">{t("messageBody")}</Label>
                  <textarea
                    id="wa-message"
                    rows={4}
                    className="flex w-full rounded-lg border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                    placeholder={t("messagePlaceholder")}
                    {...form.register("message")}
                  />
                </div>
                <Button
                  type="submit"
                  className="w-full bg-green-600 hover:bg-green-700"
                  disabled={sendMutation.isPending}
                >
                  {sendMutation.isPending ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <>
                      <Send className="h-4 w-4" />
                      {t("send")}
                    </>
                  )}
                </Button>
              </form>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>{t("conversations")}</CardTitle>
            </CardHeader>
            <CardContent>
              {conversations && conversations.length > 0 ? (
                <ul className="space-y-3">
                  {conversations.map((conv) => (
                    <li
                      key={conv.id}
                      className="flex flex-col gap-1 rounded-lg border p-3 text-sm"
                    >
                      <span className="font-medium" dir="ltr">
                        {conv.phone}
                      </span>
                      <span className="text-muted-foreground truncate">
                        {conv.lastMessage}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        {conv.timestamp}
                      </span>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="text-sm text-muted-foreground text-center py-8">
                  {t("noConversations")}
                </p>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </DashboardShell>
  );
}
