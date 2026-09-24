import "server-only";

import type { BuildingSection, FacilityDetail, OpenFacility, Telemetry } from "@/types/api";

const API_URL = process.env.API_INTERNAL_URL ?? "http://localhost:5033";

// Veřejné stránky (landing, karta objektu) čtou bez přihlášení — vždy jako role Veřejnost, bez Kč.
// Krátká cache: hodnoty se přepočítávají po minutách, stránky se sdílí přes QR kódy.
const REVALIDATE_SECONDS = 60;

async function publicGet<T>(path: string): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, { next: { revalidate: REVALIDATE_SECONDS } });

  if (!response.ok) {
    throw new Error(`API ${path} vrátilo ${response.status}`);
  }

  return (await response.json()) as T;
}

export function getPublicFacilities() {
  return publicGet<OpenFacility[]>("/api/open/v1/facilities");
}

export async function getPublicFacilityByCode(code: string) {
  const facilities = await getPublicFacilities();
  return facilities.find((f) => f.code.toLowerCase() === code.toLowerCase()) ?? null;
}

export function getPublicFacilityDetail(id: string) {
  return publicGet<FacilityDetail>(`/api/open/v1/facilities/${id}`);
}

export function getPublicSection(id: string) {
  return publicGet<BuildingSection>(`/api/facilities/${id}/section`);
}

// CO₂ za posledních 24 h po hodinách — graf na veřejné kartě
export function getPublicCo2(id: string) {
  // Začátek zaokrouhlený na celou hodinu — stejná adresa po celou hodinu, cache se trefí
  const hour = Math.floor(Date.now() / 3600_000) * 3600_000;
  const from = new Date(hour - 24 * 3600_000).toISOString();
  return publicGet<Telemetry>(`/api/open/v1/facilities/${id}/telemetry?metric=Co2&bucket=Hour&from=${encodeURIComponent(from)}`);
}
