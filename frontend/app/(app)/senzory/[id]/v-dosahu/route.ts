import { getNearbyNodes } from "@/lib/api";

// Čidla v dosahu gatewaye mimo její síť — dotaz jde přes MQTT na gatewayi, proto až na vyžádání z detailu
export async function GET(_request: Request, { params }: RouteContext<"/senzory/[id]/v-dosahu">) {
  const { id } = await params;
  if (!/^[0-9a-f-]{36}$/i.test(id)) return Response.json({ message: "Neplatné id gatewaye" }, { status: 400 });

  try {
    return Response.json(await getNearbyNodes(id), { headers: { "Cache-Control": "no-store" } });
  } catch {
    return Response.json({ message: "Čidla v dosahu se nepodařilo načíst" }, { status: 502 });
  }
}
