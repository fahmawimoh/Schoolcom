import { NextRequest, NextResponse } from "next/server";
import {
  findUserByIdentifier,
  generateOtp,
  storeOtp,
} from "@/lib/auth/demo-users";

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const identifier = body.identifier?.trim();

    if (!identifier) {
      return NextResponse.json(
        { message: "Phone or email is required" },
        { status: 400 }
      );
    }

    const user = findUserByIdentifier(identifier);
    if (!user) {
      return NextResponse.json({ message: "User not found" }, { status: 401 });
    }

    const code = generateOtp();
    storeOtp(identifier, code);

    // Dev only: log OTP to server console
    console.log(`[DEV OTP] ${identifier}: ${code}`);

    return NextResponse.json({
      data: {
        message: "Verification code sent",
        expiresInSeconds: 300,
      },
    });
  } catch {
    return NextResponse.json({ message: "Invalid request" }, { status: 400 });
  }
}
