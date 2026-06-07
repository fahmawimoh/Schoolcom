import { NextRequest, NextResponse } from "next/server";
import { proxyToBackend } from "@/lib/api/backend-proxy";
import { getAccessTokenFromRequest } from "@/lib/auth/cookies";
import { getDevTokenExpiresIn, parseDevToken } from "@/lib/auth/dev-token";
import { mapLoginUserToAuthUser } from "@/lib/auth/map-user";
import { toAuthUser } from "@/lib/auth/demo-users";

export async function GET(request: NextRequest) {
  const token = getAccessTokenFromRequest(request);
  if (!token) {
    return NextResponse.json(
      {
        success: false,
        errorCode: "AUTH_UNAUTHORIZED",
        translationKey: "auth.unauthorized",
      },
      { status: 401 }
    );
  }

  const devUser = parseDevToken(token);
  if (devUser) {
    return NextResponse.json({
      data: {
        user: toAuthUser(devUser),
        expiresIn: getDevTokenExpiresIn(token),
      },
    });
  }

  const backendResponse = await proxyToBackend(request, "/v1/auth/me", {
    method: "GET",
  });

  if (backendResponse.status !== 200) {
    return backendResponse;
  }

  const body = (await backendResponse.json()) as {
    data?: {
      user: {
        id: string;
        email?: string | null;
        phone?: string | null;
        firstName: string;
        lastName: string;
        roles: string[];
        preferredLanguage: string;
        tenantId: string;
      };
      expiresIn: number;
    };
  };

  if (!body.data?.user) {
    return NextResponse.json(
      {
        success: false,
        errorCode: "AUTH_UNAUTHORIZED",
        translationKey: "auth.unauthorized",
      },
      { status: 401 }
    );
  }

  return NextResponse.json({
    data: {
      user: mapLoginUserToAuthUser(body.data.user),
      expiresIn: body.data.expiresIn,
    },
  });
}
