import { useEffect } from "react";
import {
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
  useColorScheme,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useRouter } from "expo-router";
import { useTranslation } from "react-i18next";
import { useAuthStore } from "@/src/lib/auth/store";
import { authApi } from "@/src/lib/api/client";
import { colors, radius, spacing } from "@/src/constants/theme";

export default function ParentDashboard() {
  const { t } = useTranslation();
  const router = useRouter();
  const scheme = useColorScheme();
  const isDark = scheme === "dark";
  const { user, clearAuth, isAuthenticated } = useAuthStore();

  useEffect(() => {
    if (!isAuthenticated || !user) router.replace("/(auth)/login");
  }, [isAuthenticated, user, router]);

  if (!user) return null;

  const logout = async () => {
    try { await authApi.logout(); } catch { /* ignore */ }
    clearAuth();
    router.replace("/(auth)/login");
  };

  return (
    <SafeAreaView style={[styles.safe, { backgroundColor: isDark ? colors.backgroundDark : colors.background }]} edges={["bottom"]}>
      <ScrollView contentContainerStyle={styles.scroll}>
        <Text style={[styles.welcome, { color: isDark ? colors.textDark : colors.text }]}>
          {t("dashboard.welcome", { name: `${user.firstName} ${user.lastName}` })}
        </Text>
        <View style={[styles.card, { backgroundColor: isDark ? colors.cardDark : colors.card, borderColor: isDark ? colors.borderDark : colors.border }]}>
          <Text style={[styles.label, { color: colors.muted }]}>{t("dashboard.stats.activeConversations")}</Text>
          <Text style={[styles.value, { color: isDark ? colors.textDark : colors.text }]}>2</Text>
        </View>
        <View style={[styles.card, { backgroundColor: isDark ? colors.cardDark : colors.card, borderColor: isDark ? colors.borderDark : colors.border }]}>
          <Text style={[styles.label, { color: colors.muted }]}>{t("dashboard.stats.notifications")}</Text>
          <Text style={[styles.value, { color: isDark ? colors.textDark : colors.text }]}>1</Text>
        </View>
        <Pressable style={styles.logoutBtn} onPress={logout}>
          <Text style={styles.logoutText}>{t("nav.logout")}</Text>
        </Pressable>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1 },
  scroll: { padding: spacing.md },
  welcome: { fontSize: 20, fontWeight: "700", marginBottom: spacing.md },
  card: { borderWidth: 1, borderRadius: radius.md, padding: spacing.md, marginBottom: spacing.sm },
  label: { fontSize: 13 },
  value: { fontSize: 28, fontWeight: "700", marginTop: 4 },
  logoutBtn: { alignItems: "center", padding: spacing.lg },
  logoutText: { color: colors.destructive, fontWeight: "600" },
});
