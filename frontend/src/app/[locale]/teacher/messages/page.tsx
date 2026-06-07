import { redirect } from "next/navigation";

export default async function TeacherMessagesRedirect({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale } = await params;
  redirect(`/${locale}/teacher/conversations`);
}
