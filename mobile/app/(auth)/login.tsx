import { useState } from "react";
import {
  ActivityIndicator,
  Alert,
  KeyboardAvoidingView,
  Platform,
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
import { authApi } from "@/src/lib/api/client";
import { getDashboardPath } from "@/src/lib/api/types";
import { useAuthStore } from "@/src/lib/auth/store";
import { DEMO_USERS } from "@/src/lib/auth/demo-users";
import { setAppLocale, type AppLocale } from "@/src/i18n";
import { colors, radius, spacing } from "@/src/constants/theme";

type Tab = "email" | "phone";

export default function LoginScreen() {
  const { t, i18n } = useTranslation();
  const router = useRouter();
  const scheme = useColorScheme();
  const isDark = scheme === "dark";
  const setAuth = useAuthStore((s) => s.setAuth);

  const [tab, setTab] = useState<Tab>("email");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [phone, setPhone] = useState("");
  const [otp, setOtp] = useState("");
  const [otpStep, setOtpStep] = useState(false);
  const [phoneIdentifier, setPhoneIdentifier] = useState("");
  const [devOtpHint, setDevOtpHint] = useState("");
  const [loading, setLoading] = useState(false);

  const bg = isDark ? colors.backgroundDark : colors.background;
  const card = isDark ? colors.cardDark : colors.card;
  const text = isDark ? colors.textDark : colors.text;
  const border = isDark ? colors.borderDark : colors.border;

  const toggleLanguage = async () => {
    const next: AppLocale = i18n.language === "ar" ? "en" : "ar";
    await setAppLocale(next);
  };

  const handleEmailLogin = async () => {
    if (!email || !password) return;
    setLoading(true);
    try {
      const res = await authApi.login(email.trim(), password);
      setAuth(res.user, res.accessToken);
      router.replace(getDashboardPath(res.user.roles) as "/(app)/admin");
    } catch (e) {
      Alert.alert(t("common.error"), e instanceof Error ? e.message : t("auth.invalidCredentials"));
    } finally {
      setLoading(false);
    }
  };

  const handleSendOtp = async () => {
    if (!phone) return;
    setLoading(true);
    try {
      const res = await authApi.requestOtp(phone.trim());
      setPhoneIdentifier(phone.trim());
      setOtpStep(true);
      if ("devCode" in res && res.devCode) {
        setDevOtpHint(String(res.devCode));
      }
      Alert.alert(t("common.success"), t("auth.otpSent"));
    } catch (e) {
      Alert.alert(t("common.error"), e instanceof Error ? e.message : t("common.error"));
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyOtp = async () => {
    if (!otp || otp.length !== 6) return;
    setLoading(true);
    try {
      const res = await authApi.verifyOtp(phoneIdentifier, otp.trim());
      setAuth(res.user, res.accessToken);
      router.replace(getDashboardPath(res.user.roles) as "/(app)/admin");
    } catch (e) {
      Alert.alert(t("common.error"), e instanceof Error ? e.message : t("auth.invalidOtp"));
    } finally {
      setLoading(false);
    }
  };

  const loginAsAdmin = async () => {
    const admin = DEMO_USERS.find((u) => u.roles.includes("SchoolAdmin"));
    if (!admin) return;
    setEmail(admin.email);
    setPassword(admin.password);
    setLoading(true);
    try {
      const res = await authApi.login(admin.email, admin.password);
      setAuth(res.user, res.accessToken);
      router.replace("/(app)/admin");
    } catch (e) {
      Alert.alert(t("common.error"), e instanceof Error ? e.message : t("auth.invalidCredentials"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={[styles.safe, { backgroundColor: bg }]}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === "ios" ? "padding" : undefined}
      >
        <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
          <View style={styles.headerRow}>
            <View>
              <Text style={[styles.appName, { color: text }]}>{t("common.appName")}</Text>
              <Text style={[styles.tagline, { color: colors.muted }]}>{t("common.tagline")}</Text>
            </View>
            <Pressable onPress={toggleLanguage} style={[styles.langBtn, { borderColor: border }]}>
              <Text style={{ color: colors.primary, fontWeight: "600" }}>
                {i18n.language === "ar" ? "EN" : "AR"}
              </Text>
            </Pressable>
          </View>

          <View style={[styles.hero, { backgroundColor: colors.primary }]}>
            <Text style={styles.heroTitle}>{t("auth.welcomeBack")}</Text>
            <Text style={styles.heroSub}>{t("auth.signInSubtitle")}</Text>
          </View>

          <View style={[styles.card, { backgroundColor: card, borderColor: border }]}>
            <View style={[styles.tabs, { backgroundColor: isDark ? "#1e293b" : "#f1f5f9" }]}>
              <Pressable
                style={[styles.tab, tab === "email" && styles.tabActive]}
                onPress={() => { setTab("email"); setOtpStep(false); }}
              >
                <Text style={[styles.tabText, tab === "email" && styles.tabTextActive]}>
                  {t("auth.emailTab")}
                </Text>
              </Pressable>
              <Pressable
                style={[styles.tab, tab === "phone" && styles.tabActive]}
                onPress={() => setTab("phone")}
              >
                <Text style={[styles.tabText, tab === "phone" && styles.tabTextActive]}>
                  {t("auth.phoneTab")}
                </Text>
              </Pressable>
            </View>

            {tab === "email" ? (
              <View style={styles.form}>
                <Text style={[styles.label, { color: text }]}>{t("auth.email")}</Text>
                <TextInput
                  style={[styles.input, { color: text, borderColor: border, backgroundColor: bg }]}
                  value={email}
                  onChangeText={setEmail}
                  placeholder={t("auth.emailPlaceholder")}
                  placeholderTextColor={colors.muted}
                  autoCapitalize="none"
                  keyboardType="email-address"
                />
                <Text style={[styles.label, { color: text }]}>{t("auth.password")}</Text>
                <TextInput
                  style={[styles.input, { color: text, borderColor: border, backgroundColor: bg }]}
                  value={password}
                  onChangeText={setPassword}
                  placeholder={t("auth.passwordPlaceholder")}
                  placeholderTextColor={colors.muted}
                  secureTextEntry
                />
                <Pressable
                  style={[styles.primaryBtn, loading && styles.btnDisabled]}
                  onPress={handleEmailLogin}
                  disabled={loading}
                >
                  {loading ? (
                    <ActivityIndicator color="#fff" />
                  ) : (
                    <Text style={styles.primaryBtnText}>{t("auth.signIn")}</Text>
                  )}
                </Pressable>
              </View>
            ) : !otpStep ? (
              <View style={styles.form}>
                <Text style={[styles.label, { color: text }]}>{t("auth.phone")}</Text>
                <TextInput
                  style={[styles.input, { color: text, borderColor: border, backgroundColor: bg }]}
                  value={phone}
                  onChangeText={setPhone}
                  placeholder={t("auth.phonePlaceholder")}
                  placeholderTextColor={colors.muted}
                  keyboardType="phone-pad"
                />
                <Pressable
                  style={[styles.primaryBtn, loading && styles.btnDisabled]}
                  onPress={handleSendOtp}
                  disabled={loading}
                >
                  {loading ? (
                    <ActivityIndicator color="#fff" />
                  ) : (
                    <Text style={styles.primaryBtnText}>{t("auth.sendOtp")}</Text>
                  )}
                </Pressable>
              </View>
            ) : (
              <View style={styles.form}>
                <Text style={[styles.label, { color: text }]}>{t("auth.otp")}</Text>
                <TextInput
                  style={[styles.input, styles.otpInput, { color: text, borderColor: border, backgroundColor: bg }]}
                  value={otp}
                  onChangeText={setOtp}
                  placeholder={t("auth.otpPlaceholder")}
                  placeholderTextColor={colors.muted}
                  keyboardType="number-pad"
                  maxLength={6}
                />
                {devOtpHint ? (
                  <Text style={styles.devHint}>Dev OTP: {devOtpHint}</Text>
                ) : null}
                <Pressable
                  style={[styles.primaryBtn, loading && styles.btnDisabled]}
                  onPress={handleVerifyOtp}
                  disabled={loading}
                >
                  {loading ? (
                    <ActivityIndicator color="#fff" />
                  ) : (
                    <Text style={styles.primaryBtnText}>{t("auth.verifyAndSignIn")}</Text>
                  )}
                </Pressable>
              </View>
            )}

            <Pressable
              style={[styles.outlineBtn, { borderColor: border }, loading && styles.btnDisabled]}
              onPress={loginAsAdmin}
              disabled={loading}
            >
              <Text style={[styles.outlineBtnText, { color: colors.primary }]}>
                {t("auth.demoAdminLogin")}
              </Text>
            </Pressable>
            <Text style={styles.hint}>{t("auth.demoHint")}</Text>
            <Text style={styles.secure}>{t("auth.secureLogin")}</Text>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1 },
  flex: { flex: 1 },
  scroll: { padding: spacing.md, paddingBottom: spacing.xl },
  headerRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "flex-start",
    marginBottom: spacing.md,
  },
  appName: { fontSize: 22, fontWeight: "700" },
  tagline: { fontSize: 13, marginTop: 4 },
  langBtn: {
    borderWidth: 1,
    borderRadius: radius.sm,
    paddingHorizontal: 12,
    paddingVertical: 6,
  },
  hero: {
    borderRadius: radius.lg,
    padding: spacing.lg,
    marginBottom: spacing.md,
  },
  heroTitle: { color: "#fff", fontSize: 24, fontWeight: "700" },
  heroSub: { color: "#dbeafe", fontSize: 14, marginTop: 6 },
  card: {
    borderRadius: radius.lg,
    borderWidth: 1,
    padding: spacing.md,
  },
  tabs: {
    flexDirection: "row",
    borderRadius: radius.sm,
    padding: 4,
    marginBottom: spacing.md,
  },
  tab: {
    flex: 1,
    paddingVertical: 10,
    alignItems: "center",
    borderRadius: 6,
  },
  tabActive: { backgroundColor: "#fff" },
  tabText: { color: colors.muted, fontWeight: "600", fontSize: 14 },
  tabTextActive: { color: colors.text },
  form: { gap: spacing.sm },
  label: { fontSize: 14, fontWeight: "600", marginBottom: 4 },
  input: {
    borderWidth: 1,
    borderRadius: radius.sm,
    paddingHorizontal: 12,
    paddingVertical: 12,
    fontSize: 16,
    marginBottom: spacing.sm,
  },
  otpInput: { textAlign: "center", letterSpacing: 8, fontSize: 20 },
  primaryBtn: {
    backgroundColor: colors.primary,
    borderRadius: radius.sm,
    paddingVertical: 14,
    alignItems: "center",
    marginTop: spacing.sm,
  },
  primaryBtnText: { color: "#fff", fontWeight: "700", fontSize: 16 },
  outlineBtn: {
    borderWidth: 1,
    borderRadius: radius.sm,
    paddingVertical: 14,
    alignItems: "center",
    marginTop: spacing.md,
  },
  outlineBtnText: { fontWeight: "600", fontSize: 15 },
  btnDisabled: { opacity: 0.6 },
  hint: { textAlign: "center", color: colors.muted, fontSize: 12, marginTop: 8 },
  secure: { textAlign: "center", color: colors.muted, fontSize: 11, marginTop: 4 },
  devHint: { textAlign: "center", color: colors.primary, fontSize: 13, marginBottom: 4 },
});
