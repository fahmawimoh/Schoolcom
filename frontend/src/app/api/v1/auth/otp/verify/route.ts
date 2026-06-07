import { NextRequest, NextResponse } from "next/server";
import {
  createDevToken,
  findUserByIdentifier,
  toAuthUser,
  verifyOtp,
} from "@/lib/auth/demo-users";
import { jsonWithAuthCookie } from "@/lib/auth/login-response";

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const identifier = body.identifier?.trim();
    const code = body.code?.trim();

    if (!identifier || !code) {
      return NextResponse.json(
        { message: "Identifier and code are required" },
        { status: 400 }
      );
    }

    if (!verifyOtp(identifier, code)) {
      return NextResponse.json(
        {
          success: false,
          errorCode: "AUTH_INVALID_OTP",
          translationKey: "auth.invalidOtp",
        },
        { status: 401 }
      );
    }

    const user = findUserByIdentifier(identifier);
    if (!user) {
      return NextResponse.json({ message: "User not found" }, { status: 401 });
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
