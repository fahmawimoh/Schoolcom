"use client";

import { DashboardShell } from "@/components/layout/dashboard-shell";
import { ChatWorkspace } from "@/components/chat/chat-workspace";
import { useRequireChatRole } from "@/lib/auth/require-chat-role";

export default function ParentConversationsPage() {
  useRequireChatRole("parent");

  return (
    <DashboardShell role="parent">
      <ChatWorkspace role="parent" />
    </DashboardShell>
  );
}
