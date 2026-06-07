import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

export const ACCESS_TOKEN_COOKIE = "accessToken";

/** HttpOnly cookies with `secure` are not stored over plain HTTP (e.g. local Docker). */
export function useSecureAuthCookies(): boolean {
  if (process.env.COOKIE_SECURE === "true") return true;
  if (process.env.COOKIE_SECURE === "false") return false;
  const appUrl = process.env.NEXT_PUBLIC_APP_URL ?? "";
  if (appUrl.startsWith("https://")) return true;
  return false;
}

export function attachAuthCookie(
  response: NextResponse,
  accessToken: string,
  maxAgeSeconds: number
): NextResponse {
  response.cookies.set(ACCESS_TOKEN_COOKIE, accessToken, {
    httpOnly: true,
    secure: useSecureAuthCookies(),
    sameSite: "lax",
    path: "/",
    maxAge: maxAgeSeconds,
  });
  return response;
}

export function clearAuthCookie(response: NextResponse): NextResponse {
  response.cookies.set(ACCESS_TOKEN_COOKIE, "", {
    httpOnly: true,
    secure: useSecureAuthCookies(),
    sameSite: "lax",
    path: "/",
    maxAge: 0,
  });
  return response;
}

export function getAccessTokenFromRequest(request: NextRequest): string | null {
  const cookieToken = request.cookies.get(ACCESS_TOKEN_COOKIE)?.value;
  if (cookieToken) return cookieToken;

  const authHeader = request.headers.get("authorization");
  if (!authHeader) return null;

  const match = authHeader.match(/^Bearer\s+(.+)$/i);
  return match?.[1]?.trim() ?? null;
}

export function getBearerAuthorizationHeader(
  request: NextRequest
): string | null {
  const token = getAccessTokenFromRequest(request);
  return token ? `Bearer ${token}` : null;
}
