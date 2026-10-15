"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { apiSendAuthorized, getSession } from "@/lib/api";
import type { SensorMetric } from "@/types/api";

export type SensorActionState = { error: string } | null;

const ID_PATTERN = /^[0-9a-f-]{36}$/i;

async function canManageSensors() {
  const session = await getSession();
  return session !== null && session.role !== "Verejnost";
}

// Průvodce „Připojit IQRF gateway“ — po založení rovnou na detail, kde se čeká na první data
export async function createGateway(_state: SensorActionState, formData: FormData): Promise<SensorActionState> {
  if (!(await canManageSensors())) return { error: "Senzory může připojit jen kraj nebo správce budovy." };

  const facilityId = String(formData.get("facilityId") ?? "");
  const name = String(formData.get("name") ?? "").trim();
  const gatewayId = String(formData.get("gatewayId") ?? "").trim();
  const pollSeconds = Number(formData.get("pollSeconds") ?? 15);
  const nodes = String(formData.get("nodes") ?? "")
    .split(/[\s,;]+/)
    .map(Number)
    .filter((n) => Number.isInteger(n) && n > 0);

  if (!ID_PATTERN.test(facilityId)) return { error: "Vyberte objekt, kde gateway je." };
  if (!name) return { error: "Zadejte název gatewaye." };
  if (!gatewayId) return { error: "Zadejte id gatewaye (část topicu gateway/{id}/iqrf)." };

  const result = await apiSendAuthorized<{ id: string }>("POST", "/api/sensors/gateways", { facilityId, name, gatewayId, pollSeconds, nodes });
  if (!result.ok) return { error: result.message };

  revalidatePath("/senzory");
  redirect(`/senzory/${result.data.id}`);
}

// Testovací síť kraje: pět virtuálních gatewayí v pěti okresech, čtyři předem přiřazené do místností
export async function createDemoNetwork(): Promise<DemoNetworkState> {
  const session = await getSession();
  if (session?.role !== "Kraj") return { error: "Testovací síť celého kraje může spustit jen kraj." };

  const result = await apiSendAuthorized<{ created: number; gateways: { gatewayId: string }[] }>("POST", "/api/sensors/demo-network");
  if (!result.ok) return { error: result.message };

  revalidatePath("/senzory");
  revalidatePath("/mapa");
  return { created: result.data.created, total: result.data.gateways.length };
}

export type DemoNetworkState = { error: string } | { created: number; total: number } | null;

// Jednoklikové demo bez hardwaru: virtuální učebna ve vybrané budově (nebo v první budově uživatele) → rovnou na detail
export async function createVirtualGateway(facilityId: string | null): Promise<SensorActionState> {
  if (!(await canManageSensors())) return { error: "Senzory může připojit jen kraj nebo správce budovy." };
  if (facilityId !== null && !ID_PATTERN.test(facilityId)) return { error: "Neplatný objekt." };

  const result = await apiSendAuthorized<{ id: string }>("POST", "/api/sensors/virtual-gateways", { facilityId });
  if (!result.ok) return { error: result.message };

  revalidatePath("/senzory");
  redirect(`/senzory/${result.data.id}`);
}

const SERIAL_PATTERN = /^[0-9a-f]{8}$/i;

// Párování nové gatewaye heslem z nálepky — po úspěchu rovnou na detail, kde se čeká na první data
export async function claimGateway(serial: string, password: string, facilityId: string, name: string): Promise<SensorActionState> {
  if (!(await canManageSensors())) return { error: "Gateway může připojit jen kraj nebo správce budovy." };
  if (!SERIAL_PATTERN.test(serial)) return { error: "Neplatné sériové číslo zařízení." };
  if (!ID_PATTERN.test(facilityId)) return { error: "Vyberte objekt, kam gateway patří." };
  if (password.replace(/[^0-9a-z]/gi, "").length === 0) return { error: "Zadejte heslo z nálepky." };

  const result = await apiSendAuthorized<{ id: string }>("POST", "/api/sensors/claims", { serial, password, facilityId, name: name.trim() || null });
  if (!result.ok) return { error: result.message };

  revalidatePath("/senzory");
  redirect(`/senzory/${result.data.id}`);
}

// Přidání čidla z dosahu do sítě gatewaye kódem z nálepky (IQRF SmartConnect)
export async function bondNode(gatewayId: string, code: string): Promise<{ error: string } | { address: number }> {
  if (!(await canManageSensors())) return { error: "Čidlo může přidat jen kraj nebo správce budovy." };
  if (!ID_PATTERN.test(gatewayId)) return { error: "Neplatné id gatewaye." };
  if (code.replace(/[^0-9a-z]/gi, "").length < 8) return { error: "Kód z nálepky má 8 znaků." };

  const result = await apiSendAuthorized<{ address: number }>("POST", `/api/sensors/gateways/${gatewayId}/nodes`, { code });
  if (!result.ok) return { error: result.message };

  revalidatePath(`/senzory/${gatewayId}`);
  return { address: result.data.address };
}

export async function assignChannel(gatewayId: string, channelId: string, zoneId: string | null, metric: SensorMetric): Promise<SensorActionState> {
  if (!(await canManageSensors())) return { error: "Kanál může přiřadit jen kraj nebo správce budovy." };
  if (!ID_PATTERN.test(gatewayId) || !ID_PATTERN.test(channelId) || (zoneId !== null && !ID_PATTERN.test(zoneId))) return { error: "Neplatný výběr." };

  const result = await apiSendAuthorized<{ sensorId: string }>("POST", `/api/sensors/gateways/${gatewayId}/channels/${channelId}/assign`, { zoneId, metric });

  return result.ok ? null : { error: result.message };
}

// Hromadné přiřazení: všechna čidla jednoho uzlu (krabičky) do jedné místnosti. Kanály s neznámou veličinou se přeskočí —
// u nich musí uživatel veličinu vybrat sám.
export async function assignNode(
  gatewayId: string,
  channels: { id: string; metric: SensorMetric }[],
  zoneId: string | null,
): Promise<{ error: string } | { assigned: number; skipped: number }> {
  if (!(await canManageSensors())) return { error: "Kanály může přiřadit jen kraj nebo správce budovy." };
  if (!ID_PATTERN.test(gatewayId) || (zoneId !== null && !ID_PATTERN.test(zoneId)) || channels.some((c) => !ID_PATTERN.test(c.id))) return { error: "Neplatný výběr." };

  const known = channels.filter((c) => c.metric !== "Other");
  for (const channel of known) {
    const result = await apiSendAuthorized<{ sensorId: string }>("POST", `/api/sensors/gateways/${gatewayId}/channels/${channel.id}/assign`, { zoneId, metric: channel.metric });
    if (!result.ok) return { error: result.message };
  }

  return { assigned: known.length, skipped: channels.length - known.length };
}

export async function deleteGateway(gatewayId: string): Promise<SensorActionState> {
  if (!(await canManageSensors())) return { error: "Gateway může odebrat jen kraj nebo správce budovy." };
  if (!ID_PATTERN.test(gatewayId)) return { error: "Neplatné id gatewaye." };

  const result = await apiSendAuthorized<boolean>("DELETE", `/api/sensors/gateways/${gatewayId}`);
  if (!result.ok) return { error: result.message };

  revalidatePath("/senzory");
  redirect("/senzory");
}
