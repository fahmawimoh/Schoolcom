import { useEffect } from "react";
import {
  Alert,
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

function StatCard({ label, value, isDark }: { label: string; value: string; isDark: boolean }) {
  return (
    <View
      style={[
        styles.statCard,
        {
          backgroundColor: isDark ? colors.cardDark : colors.card,
          borderColor: isDark ? colors.borderDark : colors.border,
        },
      ]}
    >
      <Text style={[styles.statLabel, { color: colors.muted }]}>{label}</Text>
      <Text style={[styles.statValue, { color: isDark ? colors.textDark : colors.text }]}>
        {value}
      </Text>
    </View>
  );
}

export default function AdminDashboard() {
  const { t } = useTranslation();
  const router = useRouter();
  const scheme = useColorScheme();
  const isDark = scheme === "dark";
  const { user, clearAuth, isAuthenticated } = useAuthStore();

  useEffect(() => {
    if (!isAuthenticated || !user) {
      router.replace("/(auth)/login");
    }
  }, [isAuthenticated, user, router]);

  const logout = async () => {
    try {
      await authApi.logout();
    } catch {
      /* ignore */
    }
    clearAuth();
    router.replace("/(auth)/login");
  };

  if (!user) return null;

  const name = `${user.firstName} ${user.lastName}`;

  return (
    <SafeAreaView
      style={[styles.safe, { backgroundColor: isDark ? colors.backgroundDark : colors.background }]}
      edges={["bottom"]}
    >
      <ScrollView contentContainerStyle={styles.scroll}>
        <Text style={[styles.welcome, { color: isDark ? colors.textDark : colors.text }]}>
          {t("dashboard.welcome", { name })}
        </Text>

        <View style={styles.statsGrid}>
          <StatCard label={t("dashboard.stats.pendingMessages")} value="12" isDark={isDark} />
          <StatCard label={t("dashboard.stats.activeConversations")} value="48" isDark={isDark} />
          <StatCard label={t("dashboard.stats.totalStudents")} value="1,240" isDark={isDark} />
          <StatCard label={t("dashboard.stats.notifications")} value="5" isDark={isDark} />
        </View>

        <Pressable
          style={[styles.actionBtn, { backgroundColor: colors.primary }]}
          onPress={() => router.push("/(app)/admin/whatsapp")}
        >
          <Text style={styles.actionBtnText}>{t("nav.whatsapp")}</Text>
        </Pressable>

        <View
          style={[
            styles.section,
            {
              backgroundColor: isDark ? colors.cardDark : colors.card,
              borderColor: isDark ? colors.borderDark : colors.border,
            },
          ]}
        >
          <Text style={[styles.sectionTitle, { color: isDark ? colors.textDark : colors.text }]}>
            {t("dashboard.quickActions")}
          </Text>
          <Text style={styles.bullet}>• Review pending moderation queue</Text>
          <Text style={styles.bullet}>• Send WhatsApp message to parents</Text>
          <Text style={styles.bullet}>• Export weekly activity report</Text>
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
  statsGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: spacing.sm,
    marginBottom: spacing.md,
  },
  statCard: {
    width: "48%",
    borderWidth: 1,
    borderRadius: radius.md,
    padding: spacing.md,
  },
  statLabel: { fontSize: 12, marginBottom: 4 },
  statValue: { fontSize: 24, fontWeight: "700" },
  actionBtn: {
    borderRadius: radius.sm,
    padding: spacing.md,
    alignItems: "center",
    marginBottom: spacing.md,
  },
  actionBtnText: { color: "#fff", fontWeight: "700", fontSize: 16 },
  section: {
    borderWidth: 1,
    borderRadius: radius.md,
    padding: spacing.md,
    marginBottom: spacing.lg,
  },
  sectionTitle: { fontSize: 16, fontWeight: "600", marginBottom: spacing.sm },
  bullet: { color: colors.muted, fontSize: 14, marginBottom: 4 },
  logoutBtn: { alignItems: "center", padding: spacing.md },
  logoutText: { color: colors.destructive, fontWeight: "600" },
});
