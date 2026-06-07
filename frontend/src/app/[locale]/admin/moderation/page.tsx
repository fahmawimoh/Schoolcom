import { redirect } from "next/navigation";

type PageProps = {
  params: Promise<{ locale: string }>;
};

/** Legacy URL — canonical admin moderation UI lives at message-approval. */
export default async function AdminModerationRedirectPage({ params }: PageProps) {
  const { locale } = await params;
  redirect(`/${locale}/admin/message-approval`);
}
