"use client";

import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import type { ChatMessage, UserNotification } from "@/lib/api/messaging";

const HUB_URL =
  (process.env.NEXT_PUBLIC_BACKEND_WS_URL ?? "http://localhost:5000") +
  "/hubs/chat";

export function useChatHub(
  conversationId: string | null,
  onMessage?: (msg: ChatMessage) => void,
  onNotification?: (n: UserNotification) => void
) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [connected, setConnected] = useState(false);

  useEffect(() => {
    const token =
      typeof window !== "undefined"
        ? localStorage.getItem("accessToken")
        : null;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(token ? `${HUB_URL}?access_token=${token}` : HUB_URL, {
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .build();

    connection.on("messageReceived", (msg: ChatMessage) => {
      if (msg?.id && msg.moderationStatus === "Approved") onMessage?.(msg);
    });
    connection.on("moderationUpdated", () => onMessage?.({} as ChatMessage));
    connection.on("notificationReceived", (n: UserNotification) =>
      onNotification?.(n)
    );

    connection
      .start()
      .then(() => setConnected(true))
      .catch(() => setConnected(false));

    connectionRef.current = connection;

    return () => {
      void connection.stop();
      connectionRef.current = null;
      setConnected(false);
    };
  }, [onMessage, onNotification]);

  useEffect(() => {
    const connection = connectionRef.current;
    if (!connection || connection.state !== signalR.HubConnectionState.Connected)
      return;

    const join = async () => {
      if (conversationId) {
        await connection.invoke("JoinConversation", conversationId);
      }
    };
    void join();

    return () => {
      if (conversationId && connection.state === signalR.HubConnectionState.Connected) {
        void connection.invoke("LeaveConversation", conversationId);
      }
    };
  }, [conversationId, connected]);

  return { connected };
}
