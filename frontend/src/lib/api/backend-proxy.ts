import { NextRequest, NextResponse } from "next/server";
import { getBearerAuthorizationHeader } from "@/lib/auth/cookies";

const BACKEND_URL =
  process.env.BACKEND_API_URL ?? "http://localhost:5000";

function buildBackendUrl(backendPath: string, search: string) {
  const path = backendPath.startsWith("/") ? backendPath : `/${backendPath}`;
  return `${BACKEND_URL}${path}${search}`;
}

async function buildForwardHeaders(request: NextRequest) {
  const headers = new Headers();
  const auth = getBearerAuthorizationHeader(request);
  if (auth) headers.set("Authorization", auth);

  const contentType = request.headers.get("content-type");
  if (contentType) headers.set("Content-Type", contentType);

  return headers;
}

async function buildForwardBody(request: NextRequest) {
  if (request.method === "GET" || request.method === "HEAD") return undefined;

  const contentType = request.headers.get("content-type") ?? "";
  if (contentType.includes("multipart/form-data")) {
    return request.arrayBuffer();
  }
  return request.text();
}

export async function proxyRequestToBackend(
  request: NextRequest,
  backendPath: string
) {
  const url = buildBackendUrl(backendPath, request.nextUrl.search);
  const headers = await buildForwardHeaders(request);
  const body = await buildForwardBody(request);

  try {
    const response = await fetch(url, {
      method: request.method,
      headers,
      body,
    });

    const isDownload =
      backendPath.includes("/attachments/") &&
      backendPath.endsWith("/download");

    if (isDownload) {
      if (!response.ok) {
        const text = await response.text();
        return NextResponse.json(
          { message: text || "Download failed" },
          { status: response.status }
        );
      }
      const blob = await response.blob();
      const outHeaders = new Headers();
      const ct = response.headers.get("content-type");
      const cd = response.headers.get("content-disposition");
      if (ct) outHeaders.set("Content-Type", ct);
      if (cd) outHeaders.set("Content-Disposition", cd);
      return new NextResponse(blob, { status: 200, headers: outHeaders });
    }

    const text = await response.text();
    let data: unknown = null;
    if (text) {
      try {
        data = JSON.parse(text);
      } catch {
        data = { message: text };
      }
    }

    return NextResponse.json(data ?? {}, { status: response.status });
  } catch {
    return NextResponse.json(
      {
        message:
          "Backend API unavailable. Start the API server on port 5000.",
      },
      { status: 503 }
    );
  }
}

/** @deprecated Use proxyRequestToBackend for new routes */
export async function proxyToBackend(
  request: NextRequest,
  backendPath: string,
  options: { method?: string; body?: unknown } = {}
) {
  const headers: HeadersInit = {
    "Content-Type": "application/json",
  };

  const auth = getBearerAuthorizationHeader(request);
  if (auth) (headers as Record<string, string>)["Authorization"] = auth;

  const url = buildBackendUrl(backendPath, request.nextUrl.search);

  try {
    const response = await fetch(url, {
      method: options.method ?? request.method,
      headers,
      body:
        options.body !== undefined
          ? JSON.stringify(options.body)
          : request.method !== "GET" && request.method !== "HEAD"
            ? await request.text()
            : undefined,
    });

    const text = await response.text();
    let data: unknown = null;
    if (text) {
      try {
        data = JSON.parse(text);
      } catch {
        data = { message: text };
      }
    }

    return NextResponse.json(data ?? {}, { status: response.status });
  } catch {
    return NextResponse.json(
      {
        message:
          "Backend API unavailable. Start the API server on port 5000.",
      },
      { status: 503 }
    );
  }
}
