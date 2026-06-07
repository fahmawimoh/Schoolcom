import { NextResponse } from "next/server";
import { clearAuthCookie } from "@/lib/auth/cookies";

export async function POST() {
  return clearAuthCookie(NextResponse.json({ data: null }));
}
