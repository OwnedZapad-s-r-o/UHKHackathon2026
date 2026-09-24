import { apiGetFile } from "@/lib/api";
import { pdfResponse } from "../pdf-response";

// GET /reporty/souhrn → jednostránkový souhrn pro Radu kraje (PDF, jen role Kraj — hlídá API)
export async function GET() {
  return pdfResponse(await apiGetFile("/api/reports/executive-summary.pdf"));
}
