import "server-only";

import { headers } from "next/headers";

// Adresa, na které uživatel aplikaci právě otevřel — pro absolutní odkazy (ukázky API, QR kódy)
export async function requestOrigin() {
  const list = await headers();
  const host = list.get("x-forwarded-host") ?? list.get("host") ?? "localhost:3000";
  const protocol = list.get("x-forwarded-proto") ?? (host.startsWith("localhost") ? "http" : "https");

  return `${protocol}://${host}`;
}
