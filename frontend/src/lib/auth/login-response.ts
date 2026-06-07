import { NextResponse } from "next/server";
import { attachAuthCookie } from "@/lib/auth/cookies";

export function jsonWithAuthCookie(
  payload: {
    data?: {
      accessToken?: string;
      expiresIn?: number;
      user?: unknown;
    };
  },
  status = 200
) {
  const response = NextResponse.json(payload, { status });
  const token = payload.data?.accessToken;
  const expiresIn = payload.data?.expiresIn ?? 3600;

  if (token) {
    attachAuthCookie(response, token, expiresIn);
  }

  return response;
}
