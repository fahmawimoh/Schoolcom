import { DEMO_USERS, type DemoUser } from "@/lib/auth/demo-users";

export function parseDevToken(accessToken: string): DemoUser | null {
  if (!accessToken.startsWith("dev.")) return null;

  try {
    const encoded = accessToken.slice(4);
    const payload = JSON.parse(
      Buffer.from(encoded, "base64url").toString("utf8")
    ) as { sub?: string; exp?: number };

    if (typeof payload.exp === "number" && payload.exp * 1000 <= Date.now()) {
      return null;
    }

    if (!payload.sub) return null;

    return DEMO_USERS.find((user) => user.id === payload.sub) ?? null;
  } catch {
    return null;
  }
}

export function getDevTokenExpiresIn(accessToken: string): number {
  if (!accessToken.startsWith("dev.")) return 3600;

  try {
    const encoded = accessToken.slice(4);
    const payload = JSON.parse(
      Buffer.from(encoded, "base64url").toString("utf8")
    ) as { exp?: number };
    if (typeof payload.exp === "number") {
      return Math.max(payload.exp - Math.floor(Date.now() / 1000), 0);
    }
  } catch {
    /* ignore */
  }

  return 3600;
}
