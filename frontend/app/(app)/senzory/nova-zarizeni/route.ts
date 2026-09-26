import { getNearbyDevices } from "@/lib/api";

// Nové gatewaye online pro kartu na /senzory — prohlížeč token nemá (httpOnly cookie), proto přes server
export async function GET() {
  try {
    return Response.json(await getNearbyDevices(), { headers: { "Cache-Control": "no-store" } });
  } catch {
    return Response.json({ message: "Seznam nových zařízení se nepodařilo načíst" }, { status: 502 });
  }
}
