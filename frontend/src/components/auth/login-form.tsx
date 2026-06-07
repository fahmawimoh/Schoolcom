"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useTranslations, useLocale } from "next-intl";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import { Mail, Phone, Lock, Shield, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  emailLoginSchema,
  phoneLoginSchema,
  otpVerifySchema,
  type EmailLoginForm,
  type PhoneLoginForm,
  type OtpVerifyForm,
} from "@/lib/validations/auth";
import { authApi, getAuthErrorMessage, getDashboardPath } from "@/lib/api/client";
import { useAuthStore } from "@/lib/auth/store";
import { DEMO_USERS } from "@/lib/auth/demo-users";

export function LoginForm() {
  const t = useTranslations("auth");
  const locale = useLocale();
  const router = useRouter();
  const setAuth = useAuthStore((s) => s.setAuth);

  const [otpStep, setOtpStep] = useState(false);
  const [phoneNumber, setPhoneNumber] = useState("");
  const [otpCooldown, setOtpCooldown] = useState(0);
  const [isLoading, setIsLoading] = useState(false);

  const emailForm = useForm<EmailLoginForm>({
    resolver: zodResolver(emailLoginSchema),
    defaultValues: { email: "", password: "", rememberMe: false },
  });

  const phoneForm = useForm<PhoneLoginForm>({
    resolver: zodResolver(phoneLoginSchema),
    defaultValues: { phone: "" },
  });

  const otpForm = useForm<OtpVerifyForm>({
    resolver: zodResolver(otpVerifySchema),
    defaultValues: { otp: "" },
  });

  const startCooldown = (seconds: number) => {
    setOtpCooldown(seconds);
    const interval = setInterval(() => {
      setOtpCooldown((prev) => {
        if (prev <= 1) {
          clearInterval(interval);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  };

  const onEmailSubmit = async (data: EmailLoginForm) => {
    setIsLoading(true);
    try {
      const response = await authApi.login(data.email, data.password);
      setAuth(response.user, response.accessToken, response.expiresIn);
      router.push(getDashboardPath(response.user.roles, locale));
    } catch (err) {
      toast.error(getAuthErrorMessage(err, t));
    } finally {
      setIsLoading(false);
    }
  };

  const loginAsAdmin = async () => {
    const admin = DEMO_USERS.find((u) => u.roles.includes("SchoolAdmin"));
    if (!admin) return;

    emailForm.setValue("email", admin.email);
    emailForm.setValue("password", admin.password);
    setIsLoading(true);
    try {
      const response = await authApi.login(admin.email, admin.password);
      setAuth(response.user, response.accessToken, response.expiresIn);
      router.push(getDashboardPath(response.user.roles, locale));
    } catch (err) {
      toast.error(getAuthErrorMessage(err, t));
    } finally {
      setIsLoading(false);
    }
  };

  const onPhoneSubmit = async (data: PhoneLoginForm) => {
    setIsLoading(true);
    try {
      const response = await authApi.requestOtp(data.phone, "sms");
      setPhoneNumber(data.phone);
      setOtpStep(true);
      startCooldown(response.expiresInSeconds ?? 60);
      toast.success(t("otpSent"));
    } catch (err) {
      toast.error(err instanceof Error ? err.message : t("error"));
    } finally {
      setIsLoading(false);
    }
  };

  const onOtpSubmit = async (data: OtpVerifyForm) => {
    setIsLoading(true);
    try {
      const response = await authApi.verifyOtp(phoneNumber, data.otp);
      setAuth(response.user, response.accessToken, response.expiresIn);
      router.push(getDashboardPath(response.user.roles, locale));
    } catch (err) {
      toast.error(getAuthErrorMessage(err, t));
    } finally {
      setIsLoading(false);
    }
  };

  const resendOtp = async () => {
    if (otpCooldown > 0) return;
    setIsLoading(true);
    try {
      const response = await authApi.requestOtp(phoneNumber, "sms");
      startCooldown(response.expiresInSeconds ?? 60);
      toast.success(t("otpSent"));
    } catch (err) {
      toast.error(err instanceof Error ? err.message : t("error"));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4 }}
      className="w-full max-w-md"
    >
      <Tabs defaultValue="email" className="w-full">
        <TabsList>
          <TabsTrigger value="email" className="gap-2">
            <Mail className="h-4 w-4" />
            {t("emailTab")}
          </TabsTrigger>
          <TabsTrigger value="phone" className="gap-2">
            <Phone className="h-4 w-4" />
            {t("phoneTab")}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="email">
          <form
            onSubmit={emailForm.handleSubmit(onEmailSubmit)}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label htmlFor="email">{t("email")}</Label>
              <Input
                id="email"
                type="email"
                autoComplete="email"
                placeholder={t("emailPlaceholder")}
                {...emailForm.register("email")}
              />
              {emailForm.formState.errors.email && (
                <p className="text-sm text-destructive">
                  {emailForm.formState.errors.email.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="password">{t("password")}</Label>
              <Input
                id="password"
                type="password"
                autoComplete="current-password"
                placeholder={t("passwordPlaceholder")}
                {...emailForm.register("password")}
              />
              {emailForm.formState.errors.password && (
                <p className="text-sm text-destructive">
                  {emailForm.formState.errors.password.message}
                </p>
              )}
            </div>

            <div className="flex items-center justify-between">
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  className="rounded border-input"
                  {...emailForm.register("rememberMe")}
                />
                {t("rememberMe")}
              </label>
              <button
                type="button"
                className="text-sm text-primary hover:underline"
              >
                {t("forgotPassword")}
              </button>
            </div>

            <Button type="submit" className="w-full" disabled={isLoading}>
              {isLoading ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <>
                  <Lock className="h-4 w-4" />
                  {t("signIn")}
                </>
              )}
            </Button>
          </form>
        </TabsContent>

        <TabsContent value="phone">
          {!otpStep ? (
            <form
              onSubmit={phoneForm.handleSubmit(onPhoneSubmit)}
              className="space-y-4"
            >
              <div className="space-y-2">
                <Label htmlFor="phone">{t("phone")}</Label>
                <Input
                  id="phone"
                  type="tel"
                  autoComplete="tel"
                  placeholder={t("phonePlaceholder")}
                  dir="ltr"
                  {...phoneForm.register("phone")}
                />
                {phoneForm.formState.errors.phone && (
                  <p className="text-sm text-destructive">
                    {phoneForm.formState.errors.phone.message}
                  </p>
                )}
              </div>

              <Button type="submit" className="w-full" disabled={isLoading}>
                {isLoading ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  t("sendOtp")
                )}
              </Button>
            </form>
          ) : (
            <form
              onSubmit={otpForm.handleSubmit(onOtpSubmit)}
              className="space-y-4"
            >
              <div className="space-y-2">
                <Label htmlFor="otp">{t("otp")}</Label>
                <Input
                  id="otp"
                  type="text"
                  inputMode="numeric"
                  maxLength={6}
                  placeholder={t("otpPlaceholder")}
                  dir="ltr"
                  className="text-center text-lg tracking-widest"
                  {...otpForm.register("otp")}
                />
                {otpForm.formState.errors.otp && (
                  <p className="text-sm text-destructive">
                    {otpForm.formState.errors.otp.message}
                  </p>
                )}
              </div>

              <Button type="submit" className="w-full" disabled={isLoading}>
                {isLoading ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  t("verifyAndSignIn")
                )}
              </Button>

              <Button
                type="button"
                variant="ghost"
                className="w-full"
                disabled={otpCooldown > 0 || isLoading}
                onClick={resendOtp}
              >
                {otpCooldown > 0
                  ? t("resendIn", { seconds: otpCooldown })
                  : t("resendOtp")}
              </Button>
            </form>
          )}
        </TabsContent>
      </Tabs>

      <div className="mt-6 flex items-center justify-center gap-2 text-xs text-muted-foreground">
        <Shield className="h-3.5 w-3.5" />
        <span>{t("secureLogin")}</span>
      </div>

      <div className="mt-4 space-y-2">
        <Button
          type="button"
          variant="outline"
          className="w-full"
          disabled={isLoading}
          onClick={loginAsAdmin}
        >
          {isLoading ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            t("demoAdminLogin")
          )}
        </Button>
        <p className="text-center text-xs text-muted-foreground">{t("demoHint")}</p>
      </div>
    </motion.div>
  );
}
