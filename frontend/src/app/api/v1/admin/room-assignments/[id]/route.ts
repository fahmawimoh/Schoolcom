import { NextRequest } from "next/server";
import { proxyToBackend } from "@/lib/api/backend-proxy";

export async function PUT(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  return proxyToBackend(request, `/v1/admin/room-assignments/${id}`);
}

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  return proxyToBackend(request, `/v1/admin/room-assignments/${id}`);
}
