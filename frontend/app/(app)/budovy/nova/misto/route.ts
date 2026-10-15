import { apiGetResult } from "@/lib/api";
import type { Place } from "@/types/api";

// Co víme o místě budovy (adresa, obec, katastr, památky, objekty v okolí) — podle adresy z našeptávače nebo bodu v mapě
export async function GET(request: Request) {
  const params = new URL(request.url).searchParams;
  const key = params.get("klic");
  const lat = Number(params.get("lat"));
  const lon = Number(params.get("lon"));

  let query: URLSearchParams;
  if (key && /^\d{1,12}$/.test(key)) {
    query = new URLSearchParams({ addressKey: key });
  } else if (Number.isFinite(lat) && Number.isFinite(lon)) {
    query = new URLSearchParams({ lat: lat.toFixed(7), lon: lon.toFixed(7) });
  } else {
    return Response.json({ message: "Zadejte adresu nebo vyberte místo v mapě" }, { status: 400 });
  }

  const result = await apiGetResult<Place>(`/api/facilities/locate?${query}`);

  return result.ok ? Response.json(result.data) : Response.json({ message: result.message }, { status: result.status });
}
