import { getAiAudit, getSession } from "@/lib/api";
import { AiAuditCard } from "./ai-audit-card";

// AI analýza obsahuje Kč a návrh dopisu — veřejnosti se nezobrazuje
export async function AiAuditSection({ facilityId }: { facilityId: string }) {
  const session = await getSession();
  if (!session || session.role === "Verejnost") return null;

  const state = await getAiAudit(facilityId).catch(() => null);
  if (!state) return null;

  return <AiAuditCard facilityId={facilityId} configured={state.configured} initialAnalysis={state.analysis} />;
}
