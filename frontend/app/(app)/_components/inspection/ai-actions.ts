"use server";

import { apiPostAuthorized, getSession } from "@/lib/api";
import type { AiAnalysis } from "@/types/api";

export type GenerateAiAuditResult = { analysis: AiAnalysis } | { error: string };

// Spustí AI analýzu objektu; oprávnění (kraj / správce vlastního objektu) hlídá API
export async function generateAiAudit(facilityId: string): Promise<GenerateAiAuditResult> {
  if (!/^[0-9a-f-]{36}$/i.test(facilityId)) {
    return { error: "Neplatné id objektu." };
  }

  const session = await getSession();
  if (!session || session.role === "Verejnost") {
    return { error: "AI analýza je dostupná jen pro kraj a správce budovy." };
  }

  const result = await apiPostAuthorized<AiAnalysis>(`/api/facilities/${facilityId}/ai-audit`);

  return result.ok ? { analysis: result.data } : { error: result.message };
}
