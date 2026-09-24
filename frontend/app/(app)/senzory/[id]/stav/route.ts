import { getGateway } from "@/lib/api";

// Živý stav gatewaye pro průvodce — prohlížeč token nemá (httpOnly cookie), proto přes server
export async function GET(_request: Request, { params }: RouteContext<"/senzory/[id]/stav">) {
  const { id } = await params;
  if (!/^[0-9a-f-]{36}$/i.test(id)) return Response.json({ message: "Neplatné id gatewaye" }, { status: 400 });

  try {
    return Response.json(await getGateway(id), { headers: { "Cache-Control": "no-store" } });
  } catch {
    return Response.json({ message: "Gateway se nepodařilo načíst" }, { status: 502 });
  }
}
