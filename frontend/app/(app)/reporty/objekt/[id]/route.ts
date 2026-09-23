import { apiGetFile } from "@/lib/api";
import { pdfResponse } from "../../pdf-response";

// GET /reporty/objekt/{id} → PDF audit objektu
export async function GET(_request: Request, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;

  if (!/^[0-9a-f-]{36}$/i.test(id)) {
    return new Response("Neplatné id objektu.", { status: 400 });
  }

  return pdfResponse(await apiGetFile(`/api/reports/facility/${id}.pdf`));
}
