import { NextRequest, NextResponse } from "next/server";
import { proxyToBackend } from "@/lib/api/backend-proxy";
import {
  createDevToken,
  findUserByEmail,
  toAuthUser,
} from "@/lib/auth/demo-users";
import { jsonWithAuthCookie } from "@/lib/auth/login-response";

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const email = body.email?.trim();
    const password = body.password;

    if (!email || !password) {
      return NextResponse.json(
        { message: "Email and password are required" },
        { status: 400 }
      );
    }

    const backendResponse = await proxyToBackend(request, "/v1/auth/login", {
      method: "POST",
      body: { email, password },
    });

    if (backendResponse.status === 200) {
      const payload = await backendResponse.json();
      return jsonWithAuthCookie(payload);
    }

    const user = findUserByEmail(email);
    if (!user || user.password !== password) {
      return NextResponse.json(
        {
          success: false,
          errorCode: "AUTH_INVALID_CREDENTIALS",
          translationKey: "auth.invalidCredentials",
        },
        { status: 401 }
      );
    }

    return jsonWithAuthCookie({
      data: {
        accessToken: createDevToken(user),
        expiresIn: 3600,
        user: toAuthUser(user),
      },
    });
  } catch {
    return NextResponse.json({ message: "Invalid request" }, { status: 400 });
  }
}
