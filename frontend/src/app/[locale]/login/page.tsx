import { setRequestLocale } from "next-intl/server";
import { LoginPageContent } from "@/components/auth/login-page-content";

export default async function LoginPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale } = await params;
  setRequestLocale(locale);
  return <LoginPageContent />;
}
