export const SESSION_DURATION_SECONDS = 3600;
export const SESSION_WARNING_5_MIN_MS = 5 * 60 * 1000;
export const SESSION_WARNING_1_MIN_MS = 60 * 1000;

export function computeSessionExpiresAt(
  accessToken: string,
  expiresInSeconds: number
): number {
  const jwtExpiry = parseJwtExpiryMs(accessToken);
  if (jwtExpiry !== null) return jwtExpiry;
  return Date.now() + expiresInSeconds * 1000;
}

function parseJwtExpiryMs(accessToken: string): number | null {
  const parts = accessToken.split(".");
  if (parts.length !== 3) return null;
  try {
    const payload = JSON.parse(
      atob(parts[1].replace(/-/g, "+").replace(/_/g, "/"))
    ) as { exp?: number };
    if (typeof payload.exp === "number") return payload.exp * 1000;
  } catch {
    return null;
  }
  return null;
}

export function formatRemainingTime(ms: number, locale: string): string {
  if (ms <= 0) return locale === "ar" ? "0:00" : "0:00";
  const totalSeconds = Math.floor(ms / 1000);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  const pad = (n: number) => String(n).padStart(2, "0");
  if (hours > 0) return `${hours}:${pad(minutes)}:${pad(seconds)}`;
  return `${minutes}:${pad(seconds)}`;
}

export function isTokenExpired(sessionExpiresAt: number): boolean {
  return sessionExpiresAt <= Date.now();
}

export function clearAllAuthStorage(): void {
  if (typeof window === "undefined") return;

  localStorage.removeItem("accessToken");
  localStorage.removeItem("schoolcom-auth");
  sessionStorage.clear();

  void fetch("/api/v1/auth/clear-session", {
    method: "POST",
    credentials: "include",
  }).catch(() => undefined);
}
