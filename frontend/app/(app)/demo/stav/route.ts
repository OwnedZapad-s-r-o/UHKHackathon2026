import { getDemoOverview } from "@/lib/api";

// Živý stav demo správy (párování a oprava ztráty) — prohlížeč token nemá (httpOnly cookie), proto přes server
export async function GET() {
  try {
    return Response.json(await getDemoOverview(), { headers: { "Cache-Control": "no-store" } });
  } catch {
    return Response.json({ message: "Stav demo správy se nepodařilo načíst" }, { status: 502 });
  }
}
