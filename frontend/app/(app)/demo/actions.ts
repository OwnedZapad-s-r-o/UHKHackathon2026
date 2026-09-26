"use server";

import { revalidatePath } from "next/cache";
import { apiSendAuthorized, getSession } from "@/lib/api";
import type { DemoBehavior } from "@/types/api";

export type DemoCreated = { id: string; serial: string; code: string };

type DemoResult = { error: string } | { created: DemoCreated };

const ID_PATTERN = /^[0-9a-f-]{36}$/i;
const TEMPLATE_PATTERN = /^[a-z0-9]{1,20}$/;
const BEHAVIORS: DemoBehavior[] = ["Setback", "Overheating"];

async function canUseDemo() {
  const session = await getSession();
  return session !== null && session.role !== "Verejnost";
}

// „Vyrobí“ gatewayi z výroby — do 10 s se ohlásí v Nových zařízeních online na stránce Senzory
export async function createDemoGateway(templates: string[], behavior: DemoBehavior): Promise<DemoResult> {
  if (!(await canUseDemo())) return { error: "Demo správu může použít jen kraj nebo správce budovy." };
  if (templates.length < 1 || templates.length > 3 || !templates.every((t) => TEMPLATE_PATTERN.test(t))) return { error: "Vyberte 1 až 3 uzly." };
  if (!BEHAVIORS.includes(behavior)) return { error: "Neznámé chování zařízení." };

  const result = await apiSendAuthorized<DemoCreated>("POST", "/api/demo/gateways", { templates, behavior });
  if (!result.ok) return { error: result.message };

  revalidatePath("/demo");
  return { created: result.data };
}

// „Položí“ čidlo do dosahu virtuální gatewaye — objeví se v Čidlech v dosahu na detailu gatewaye
export async function createDemoNode(gatewayId: string, template: string, behavior: DemoBehavior): Promise<DemoResult> {
  if (!(await canUseDemo())) return { error: "Demo správu může použít jen kraj nebo správce budovy." };
  if (!ID_PATTERN.test(gatewayId)) return { error: "Vyberte gatewayi." };
  if (!TEMPLATE_PATTERN.test(template)) return { error: "Vyberte druh čidla." };
  if (!BEHAVIORS.includes(behavior)) return { error: "Neznámé chování zařízení." };

  const result = await apiSendAuthorized<DemoCreated>("POST", "/api/demo/nodes", { gatewayId, template, behavior });
  if (!result.ok) return { error: result.message };

  revalidatePath("/demo");
  return { created: result.data };
}

export async function deleteDemoDevice(id: string): Promise<{ error: string } | null> {
  if (!(await canUseDemo())) return { error: "Demo správu může použít jen kraj nebo správce budovy." };
  if (!ID_PATTERN.test(id)) return { error: "Neplatné id zařízení." };

  const result = await apiSendAuthorized<boolean>("DELETE", `/api/demo/${id}`);
  if (!result.ok) return { error: result.message };

  revalidatePath("/demo");
  return null;
}
