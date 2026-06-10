import { NextRequest } from "next/server";
import { proxyToBackend } from "@/lib/api/backend-proxy";

export async function GET(request: NextRequest) {
  return proxyToBackend(request, "/v1/admin/room-assignments/class-section-options");
}
