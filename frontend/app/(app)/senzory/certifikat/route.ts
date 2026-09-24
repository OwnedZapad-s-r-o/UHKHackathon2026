import { apiGetFile } from "@/lib/api";

// GET /senzory/certifikat → veřejný certifikát CA brokeru pro IQRF gateway (TLS).
// Prohlížeč token nemá (httpOnly cookie), proto přes server.
export async function GET() {
  const response = await apiGetFile("/api/sensors/broker/ca.crt");

  if (!response.ok) return Response.json({ message: "Certifikát brokeru zatím není k dispozici" }, { status: response.status });

  return new Response(response.body, {
    headers: {
      "Content-Type": "application/x-x509-ca-cert",
      "Content-Disposition": 'attachment; filename="smartaudit-ca.crt"',
      "Cache-Control": "no-store",
    },
  });
}
