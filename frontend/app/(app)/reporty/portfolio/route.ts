import type { NextRequest } from "next/server";
import { apiGetFile } from "@/lib/api";
import { DISTRICTS } from "@/lib/facility-meta";
import { pdfResponse } from "../pdf-response";

// GET /reporty/portfolio?okres=CZ0525&zjisteni=1 → PDF přehled portfolia
export async function GET(request: NextRequest) {
  const district = request.nextUrl.searchParams.get("okres");
  const onlyIssues = request.nextUrl.searchParams.get("zjisteni") === "1";

  const params = new URLSearchParams();
  if (district && DISTRICTS.some((d) => d.code === district)) params.set("district", district);
  if (onlyIssues) params.set("onlyIssues", "true");

  return pdfResponse(await apiGetFile(`/api/reports/portfolio.pdf?${params}`));
}
