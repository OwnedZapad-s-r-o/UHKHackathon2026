"use server";

import { revalidatePath } from "next/cache";
import { apiSendAuthorized, getSession } from "@/lib/api";

export type BuildingActionResult = { error: string } | null;

// Obrys budovy jako GeoJSON Polygon (jen vnější hrana) — z podkladové mapy nebo z ruční úpravy
export type OutlinePolygon = { type: "Polygon"; coordinates: [number, number][][] };

const ID_PATTERN = /^[0-9a-f-]{36}$/i;
const MAX_POINTS = 500;

async function canEditBuildings() {
  const session = await getSession();
  return session !== null && session.role !== "Verejnost";
}

function isOutline(geometry: OutlinePolygon) {
  const ring = geometry?.coordinates?.[0];
  return (
    geometry?.type === "Polygon" &&
    Array.isArray(ring) &&
    ring.length >= 4 &&
    ring.length <= MAX_POINTS &&
    ring.every((point) => Array.isArray(point) && point.length === 2 && point.every((value) => Number.isFinite(value)))
  );
}

// Po změně budovy se přepočítá audit a mapa musí načíst nový seznam objektů (s novou verzí obrysu)
function refreshMap() {
  revalidatePath("/mapa");
  revalidatePath("/portfolio");
}

export async function addBuildingPart(facilityId: string, geometry: OutlinePolygon, drawn: boolean): Promise<BuildingActionResult> {
  if (!(await canEditBuildings())) return { error: "Budovu může upravit jen kraj nebo správce budovy." };
  if (!ID_PATTERN.test(facilityId)) return { error: "Neplatný objekt." };
  if (!isOutline(geometry)) return { error: "Obrys budovy musí mít aspoň 3 body." };

  const result = await apiSendAuthorized("POST", `/api/facilities/${facilityId}/building-parts`, { geometry, drawn });
  if (!result.ok) return { error: result.message };

  refreshMap();
  return null;
}

// Výměna doplněné budovy za jinou z mapy, nebo uložení ručně upraveného obrysu
export async function replaceBuildingPart(facilityId: string, partId: string, geometry: OutlinePolygon, drawn: boolean): Promise<BuildingActionResult> {
  if (!(await canEditBuildings())) return { error: "Budovu může upravit jen kraj nebo správce budovy." };
  if (!ID_PATTERN.test(facilityId) || !ID_PATTERN.test(partId)) return { error: "Neplatná budova." };
  if (!isOutline(geometry)) return { error: "Obrys budovy musí mít aspoň 3 body." };

  const result = await apiSendAuthorized("PUT", `/api/facilities/${facilityId}/building-parts/${partId}`, { geometry, drawn });
  if (!result.ok) return { error: result.message };

  refreshMap();
  return null;
}

export async function deleteBuildingPart(facilityId: string, partId: string): Promise<BuildingActionResult> {
  if (!(await canEditBuildings())) return { error: "Budovu může upravit jen kraj nebo správce budovy." };
  if (!ID_PATTERN.test(facilityId) || !ID_PATTERN.test(partId)) return { error: "Neplatná budova." };

  const result = await apiSendAuthorized("DELETE", `/api/facilities/${facilityId}/building-parts/${partId}`);
  if (!result.ok) return { error: result.message };

  refreshMap();
  return null;
}

export type BuildingDataState = { error: string } | { saved: true } | null;

// Oprava údajů pro výpočet ztráty; prázdné pole = vrátit údaj z ČÚZK
export async function updateBuildingData(facilityId: string, _state: BuildingDataState, formData: FormData): Promise<BuildingDataState> {
  if (!(await canEditBuildings())) return { error: "Údaje budovy může opravit jen kraj nebo správce budovy." };
  if (!ID_PATTERN.test(facilityId)) return { error: "Neplatný objekt." };

  const area = optionalNumber(formData.get("area"));
  const floors = optionalNumber(formData.get("floors"));
  const year = optionalNumber(formData.get("year"));

  if (Number.isNaN(area) || Number.isNaN(floors) || Number.isNaN(year)) return { error: "Zadejte čísla (nebo pole nechte prázdné)." };
  if (floors !== null && !Number.isInteger(floors)) return { error: "Počet podlaží musí být celé číslo." };
  if (year !== null && !Number.isInteger(year)) return { error: "Rok výstavby musí být celé číslo." };

  const result = await apiSendAuthorized("PUT", `/api/facilities/${facilityId}/building`, { builtUpArea: area, floors, constructionYear: year });
  if (!result.ok) return { error: result.message };

  refreshMap();
  return { saved: true };
}

// Prázdné pole = null; „1 380“ i „1380,5“ jde zapsat po česku
function optionalNumber(value: FormDataEntryValue | null) {
  const text = String(value ?? "").replace(/\s/g, "").replace(",", ".");
  return text === "" ? null : Number(text);
}
