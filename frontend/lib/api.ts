import "server-only";

import { cookies } from "next/headers";
import type {
  AiAuditState,
  ApiStatus,
  AuditStatus,
  BuildingSection,
  DataSourcesResponse,
  DemoOverview,
  FacilityContracts,
  FacilityDetail,
  FacilityListItem,
  GuideAnswer,
  GuideContext,
  GatewayDetail,
  GatewaysResponse,
  Heatmap,
  NearbyDevice,
  NearbyNodes,
  OpenDataSync,
  SensorMetric,
  Session,
  Telemetry,
} from "@/types/api";

const API_URL = process.env.API_INTERNAL_URL ?? "http://localhost:5033";

export const SESSION_COOKIE = "sa_session";

export async function getSession(): Promise<Session | null> {
  const raw = (await cookies()).get(SESSION_COOKIE)?.value;
  if (!raw) return null;

  try {
    return JSON.parse(raw) as Session;
  } catch {
    return null;
  }
}

// Serverové volání API s tokenem přihlášeného uživatele (token nikdy neopouští server)
async function apiGet<T>(path: string): Promise<T> {
  const session = await getSession();

  const response = await fetch(`${API_URL}${path}`, {
    headers: session ? { Authorization: `Bearer ${session.token}` } : undefined,
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error(`API ${path} vrátilo ${response.status}`);
  }

  return (await response.json()) as T;
}

// Čtení, u kterého chceme českou hlášku z API místo výjimky (našeptávač, dohledání místa) — pro route handlery
export async function apiGetResult<T>(path: string): Promise<{ ok: true; data: T } | { ok: false; status: number; message: string }> {
  const session = await getSession();

  const response = await fetch(`${API_URL}${path}`, {
    headers: session ? { Authorization: `Bearer ${session.token}` } : undefined,
    cache: "no-store",
  });

  if (!response.ok) {
    const error = (await response.json().catch(() => null)) as { message?: string } | null;
    return { ok: false, status: response.status, message: error?.message ?? "Chyba na straně serveru" };
  }

  return { ok: true, data: (await response.json()) as T };
}

// Soubor z API (PDF report) s tokenem ze session — prohlížeč token nemá, je v httpOnly cookie
export async function apiGetFile(path: string): Promise<Response> {
  const session = await getSession();

  return fetch(`${API_URL}${path}`, {
    headers: session ? { Authorization: `Bearer ${session.token}` } : undefined,
    cache: "no-store",
  });
}

export type FacilityFilter = {
  q?: string;
  district?: string;
  onlyIssues?: boolean;
  status?: AuditStatus;
};

export function getFacilities(filter: FacilityFilter = {}) {
  const params = new URLSearchParams();
  if (filter.q) params.set("q", filter.q);
  if (filter.district) params.set("district", filter.district);
  if (filter.onlyIssues) params.set("onlyIssues", "true");
  if (filter.status) params.set("status", filter.status);

  const query = params.toString();
  return apiGet<FacilityListItem[]>(`/api/facilities${query ? `?${query}` : ""}`);
}

export function getFacilityDetail(id: string) {
  return apiGet<FacilityDetail>(`/api/facilities/${id}`);
}

export function getTelemetry(id: string, metric: SensorMetric, options: { bucket?: "Hour" | "Day"; hours?: number } = {}) {
  const from = new Date(Date.now() - (options.hours ?? 48) * 3600_000).toISOString();
  const params = new URLSearchParams({ metric, bucket: options.bucket ?? "Hour", from });

  return apiGet<Telemetry>(`/api/facilities/${id}/telemetry?${params}`);
}

export function getHeatmap(id: string, metric: SensorMetric) {
  return apiGet<Heatmap>(`/api/facilities/${id}/heatmap?metric=${metric}`);
}

export function getContracts(id: string) {
  return apiGet<FacilityContracts>(`/api/facilities/${id}/contracts`);
}

export function getAiAudit(id: string) {
  return apiGet<AiAuditState>(`/api/facilities/${id}/ai-audit`);
}

export async function apiPostAuthorized<T>(path: string): Promise<{ ok: true; data: T } | { ok: false; message: string }> {
  const session = await getSession();

  const response = await fetch(`${API_URL}${path}`, {
    method: "POST",
    headers: session ? { Authorization: `Bearer ${session.token}` } : undefined,
    cache: "no-store",
  });

  if (!response.ok) {
    const error = (await response.json().catch(() => null)) as { message?: string } | null;
    return { ok: false, message: error?.message ?? "Chyba na straně serveru" };
  }

  return { ok: true, data: (await response.json()) as T };
}

// Zápis s tokenem přihlášeného uživatele — vrací data nebo českou hlášku z API
export async function apiSendAuthorized<T>(method: "POST" | "PUT" | "DELETE", path: string, body?: unknown): Promise<{ ok: true; data: T } | { ok: false; message: string }> {
  const session = await getSession();
  const headers: Record<string, string> = session ? { Authorization: `Bearer ${session.token}` } : {};
  if (body !== undefined) headers["Content-Type"] = "application/json";

  const response = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
    cache: "no-store",
  });

  if (!response.ok) {
    const error = (await response.json().catch(() => null)) as { message?: string } | null;
    return { ok: false, message: error?.message ?? "Chyba na straně serveru" };
  }

  return { ok: true, data: (await response.json()) as T };
}

export function getGateways() {
  return apiGet<GatewaysResponse>("/api/sensors/gateways");
}

export function getGateway(id: string) {
  return apiGet<GatewayDetail>(`/api/sensors/gateways/${id}`);
}

export function getNearbyDevices() {
  return apiGet<NearbyDevice[]>("/api/sensors/nearby");
}

export function getNearbyNodes(gatewayId: string) {
  return apiGet<NearbyNodes>(`/api/sensors/gateways/${gatewayId}/nearby`);
}

export function getDemoOverview() {
  return apiGet<DemoOverview>("/api/demo");
}

export function getSection(id: string) {
  return apiGet<BuildingSection>(`/api/facilities/${id}/section`);
}

export function getDataSources() {
  return apiGet<DataSourcesResponse>("/api/data-sources");
}

export function getOpenDataStatus() {
  return apiGet<OpenDataSync>("/api/opendata/status");
}

export function getApiStatus() {
  return apiGet<ApiStatus>("/api/status");
}

export async function apiPost<T>(path: string, body: unknown): Promise<{ ok: true; data: T } | { ok: false; message: string }> {
  const response = await fetch(`${API_URL}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
    cache: "no-store",
  });

  if (!response.ok) {
    const error = (await response.json().catch(() => null)) as { message?: string } | null;
    return { ok: false, message: error?.message ?? "Chyba na straně serveru" };
  }

  return { ok: true, data: (await response.json()) as T };
}

// Dotaz na průvodce — s tokenem, když je uživatel přihlášený (jinak jako veřejnost).
// Adresu návštěvníka posíláme dál, aby limit dotazů platil na člověka, ne na celý server Next.js.
export async function askGuide(body: { question: string; context: GuideContext; history: string[] }, clientIp: string | null) {
  const session = await getSession();
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (session) headers.Authorization = `Bearer ${session.token}`;
  if (clientIp) headers["X-Forwarded-For"] = clientIp;

  const response = await fetch(`${API_URL}/api/guide/ask`, { method: "POST", headers, body: JSON.stringify(body), cache: "no-store" });

  if (!response.ok) {
    const error = (await response.json().catch(() => null)) as { message?: string } | null;
    return { ok: false as const, status: response.status, message: error?.message ?? "Průvodce teď neodpovídá, zkuste to prosím za chvíli." };
  }

  return { ok: true as const, data: (await response.json()) as GuideAnswer };
}
