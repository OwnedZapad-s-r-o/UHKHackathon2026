import { apiGetResult } from "@/lib/api";
import type { AddressSuggestion } from "@/types/api";

// Našeptávač adres pro přidání budovy — prohlížeč token nemá (httpOnly cookie), proto přes server
export async function GET(request: Request) {
  const query = new URL(request.url).searchParams.get("q")?.trim() ?? "";
  if (query.length < 3) return Response.json([]);

  const result = await apiGetResult<AddressSuggestion[]>(`/api/facilities/address-suggestions?q=${encodeURIComponent(query.slice(0, 120))}`);

  return result.ok
    ? Response.json(result.data, { headers: { "Cache-Control": "private, max-age=300" } })
    : Response.json({ message: result.message }, { status: result.status });
}
