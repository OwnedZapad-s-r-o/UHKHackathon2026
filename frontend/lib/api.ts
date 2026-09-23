import "server-only";

import { cookies } from "next/headers";
import type {
  ApiStatus,
  AuditStatus,
  BuildingSection,
  DataSourcesResponse,
  FacilityDetail,
  FacilityListItem,
  Footprint,
  Heatmap,
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

export function getFootprint(id: string) {
  return apiGet<Footprint>(`/api/facilities/${id}/footprint`);
}

export function getSection(id: string) {
  return apiGet<BuildingSection>(`/api/facilities/${id}/section`);
}

export function getDataSources() {
  return apiGet<DataSourcesResponse>("/api/data-sources");
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
