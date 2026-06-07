import { NextRequest, NextResponse } from "next/server";
import { proxyToBackend } from "@/lib/api/backend-proxy";
import { clearAuthCookie, getAccessTokenFromRequest } from "@/lib/auth/cookies";
import { parseDevToken } from "@/lib/auth/dev-token";

export async function POST(request: NextRequest) {
  const token = getAccessTokenFromRequest(request);
  const isDevSession = token ? parseDevToken(token) !== null : false;

  if (!isDevSession) {
    await proxyToBackend(request, "/v1/auth/logout", {
      method: "POST",
    });
  }

  return clearAuthCookie(NextResponse.json({ data: null }));
}
