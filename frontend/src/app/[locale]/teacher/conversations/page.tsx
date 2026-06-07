"use client";

import { DashboardShell } from "@/components/layout/dashboard-shell";
import { ChatWorkspace } from "@/components/chat/chat-workspace";
import { useRequireChatRole } from "@/lib/auth/require-chat-role";

export default function TeacherConversationsPage() {
  useRequireChatRole("teacher");

  return (
    <DashboardShell role="teacher">
      <ChatWorkspace role="teacher" />
    </DashboardShell>
  );
}
