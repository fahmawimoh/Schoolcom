import { NextRequest } from "next/server";
import { proxyRequestToBackend } from "@/lib/api/backend-proxy";

type RouteContext = { params: Promise<{ path: string[] }> };

async function proxyMessaging(request: NextRequest, context: RouteContext) {
  const { path } = await context.params;
  const segments = path?.length ? path.join("/") : "";
  const backendPath = `/v1/messaging/${segments}`;
  return proxyRequestToBackend(request, backendPath);
}

export async function GET(request: NextRequest, context: RouteContext) {
  return proxyMessaging(request, context);
}

export async function POST(request: NextRequest, context: RouteContext) {
  return proxyMessaging(request, context);
}

export async function PUT(request: NextRequest, context: RouteContext) {
  return proxyMessaging(request, context);
}

export async function PATCH(request: NextRequest, context: RouteContext) {
  return proxyMessaging(request, context);
}

export async function DELETE(request: NextRequest, context: RouteContext) {
  return proxyMessaging(request, context);
}
