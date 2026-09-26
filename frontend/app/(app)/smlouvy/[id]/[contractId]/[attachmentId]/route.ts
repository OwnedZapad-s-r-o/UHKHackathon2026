import { apiGetFile } from "@/lib/api";
import { pdfResponse } from "../../../../reporty/pdf-response";

const GUID = /^[0-9a-f-]{36}$/i;

// GET /smlouvy/{objekt}/{smlouva}/{příloha} → originální PDF z Registru smluv (inline, prohlížeč ho rovnou zobrazí)
export async function GET(_request: Request, { params }: { params: Promise<{ id: string; contractId: string; attachmentId: string }> }) {
  const { id, contractId, attachmentId } = await params;

  if (!GUID.test(id) || !GUID.test(contractId) || !/^\d+$/.test(attachmentId)) {
    return new Response("Neplatný odkaz na smlouvu.", { status: 400 });
  }

  return pdfResponse(
    await apiGetFile(`/api/facilities/${id}/contracts/${contractId}/attachments/${attachmentId}`),
    "Smlouvu se nepodařilo otevřít — přihlaste se jako kraj nebo správce objektu.",
  );
}
