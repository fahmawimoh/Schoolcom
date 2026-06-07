import { useEffect, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
  useColorScheme,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useRouter } from "expo-router";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { useAuthStore } from "@/src/lib/auth/store";
import { whatsAppApi } from "@/src/lib/api/client";
import { isAdmin } from "@/src/lib/api/types";
import { colors, radius, spacing } from "@/src/constants/theme";

export default function AdminWhatsAppScreen() {
  const { t } = useTranslation();
  const router = useRouter();
  const scheme = useColorScheme();
  const isDark = scheme === "dark";
  const { user, isAuthenticated } = useAuthStore();
  const [phone, setPhone] = useState("");
  const [message, setMessage] = useState("");
  const [sending, setSending] = useState(false);

  const { data: config } = useQuery({
    queryKey: ["whatsapp-config"],
    queryFn: () => whatsAppApi.getConfig(),
    enabled: !!user && isAdmin(user.roles),
  });

  const { data: conversations } = useQuery({
    queryKey: ["whatsapp-conversations"],
    queryFn: () => whatsAppApi.getConversations(),
    enabled: !!user && isAdmin(user.roles),
  });

  useEffect(() => {
    if (!isAuthenticated || !user) {
      router.replace("/(auth)/login");
    } else if (!isAdmin(user.roles)) {
      router.replace("/(app)/admin");
    }
  }, [isAuthenticated, user, router]);

  const send = async () => {
    if (!phone || !message) return;
    setSending(true);
    try {
      await whatsAppApi.sendMessage(phone.trim(), message.trim());
      Alert.alert(t("common.success"), t("whatsapp.sent"));
      setMessage("");
    } catch (e) {
      Alert.alert(t("common.error"), e instanceof Error ? e.message : t("common.error"));
    } finally {
      setSending(false);
    }
  };

  if (!user || !isAdmin(user.roles)) return null;

  const card = isDark ? colors.cardDark : colors.card;
  const border = isDark ? colors.borderDark : colors.border;
  const text = isDark ? colors.textDark : colors.text;
  const bg = isDark ? colors.backgroundDark : colors.background;

  return (
    <SafeAreaView style={[styles.safe, { backgroundColor: bg }]} edges={["bottom"]}>
      <ScrollView contentContainerStyle={styles.scroll}>
        <Text style={[styles.title, { color: text }]}>{t("whatsapp.title")}</Text>
        <Text style={styles.subtitle}>{t("whatsapp.subtitle")}</Text>

        <View style={[styles.card, { backgroundColor: card, borderColor: border }]}>
          <Text style={[styles.status, { color: text }]}>
            {config?.connected ? t("whatsapp.connected") : t("whatsapp.disconnected")}
          </Text>
          <Text style={styles.meta}>{config?.phoneNumberId ?? t("whatsapp.configure")}</Text>
        </View>

        <View style={[styles.card, { backgroundColor: card, borderColor: border }]}>
          <Text style={[styles.sectionTitle, { color: text }]}>{t("whatsapp.sendMessage")}</Text>
          <Text style={[styles.label, { color: text }]}>{t("whatsapp.recipientPhone")}</Text>
          <TextInput
            style={[styles.input, { color: text, borderColor: border, backgroundColor: bg }]}
            value={phone}
            onChangeText={setPhone}
            placeholder="+966501234567"
            placeholderTextColor={colors.muted}
            keyboardType="phone-pad"
          />
          <Text style={[styles.label, { color: text }]}>{t("whatsapp.messageBody")}</Text>
          <TextInput
            style={[styles.textarea, { color: text, borderColor: border, backgroundColor: bg }]}
            value={message}
            onChangeText={setMessage}
            placeholder={t("whatsapp.messagePlaceholder")}
            placeholderTextColor={colors.muted}
            multiline
            numberOfLines={4}
          />
          <Pressable
            style={[styles.sendBtn, sending && styles.disabled]}
            onPress={send}
            disabled={sending}
          >
            {sending ? (
              <ActivityIndicator color="#fff" />
            ) : (
              <Text style={styles.sendBtnText}>{t("whatsapp.send")}</Text>
            )}
          </Pressable>
        </View>

        <View style={[styles.card, { backgroundColor: card, borderColor: border }]}>
          <Text style={[styles.sectionTitle, { color: text }]}>{t("whatsapp.conversations")}</Text>
          {conversations && conversations.length > 0 ? (
            conversations.map((c) => (
              <View key={c.id} style={[styles.convItem, { borderColor: border }]}>
                <Text style={[styles.convPhone, { color: text }]}>{c.phone}</Text>
                <Text style={styles.convMsg} numberOfLines={1}>{c.lastMessage}</Text>
              </View>
            ))
          ) : (
            <Text style={styles.empty}>{t("whatsapp.noConversations")}</Text>
          )}
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1 },
  scroll: { padding: spacing.md },
  title: { fontSize: 22, fontWeight: "700" },
  subtitle: { color: colors.muted, marginTop: 4, marginBottom: spacing.md },
  card: {
    borderWidth: 1,
    borderRadius: radius.md,
    padding: spacing.md,
    marginBottom: spacing.md,
  },
  status: { fontSize: 16, fontWeight: "600" },
  meta: { color: colors.muted, marginTop: 4, fontSize: 13 },
  sectionTitle: { fontSize: 16, fontWeight: "600", marginBottom: spacing.sm },
  label: { fontSize: 14, fontWeight: "600", marginBottom: 4, marginTop: spacing.sm },
  input: {
    borderWidth: 1,
    borderRadius: radius.sm,
    padding: 12,
    fontSize: 16,
  },
  textarea: {
    borderWidth: 1,
    borderRadius: radius.sm,
    padding: 12,
    fontSize: 16,
    minHeight: 100,
    textAlignVertical: "top",
  },
  sendBtn: {
    backgroundColor: colors.whatsapp,
    borderRadius: radius.sm,
    padding: 14,
    alignItems: "center",
    marginTop: spacing.md,
  },
  sendBtnText: { color: "#fff", fontWeight: "700" },
  disabled: { opacity: 0.6 },
  convItem: { borderWidth: 1, borderRadius: radius.sm, padding: 12, marginTop: 8 },
  convPhone: { fontWeight: "600" },
  convMsg: { color: colors.muted, marginTop: 2 },
  empty: { color: colors.muted, textAlign: "center", padding: spacing.md },
});
