import "server-only";

import { headers } from "next/headers";
import type { Broker } from "@/types/api";

// Adresa brokeru pro návod: nastavená IQRF_BROKER_PUBLIC_HOST, jinak počítač, na kterém uživatel otevřel web
// (broker běží v docker-compose vedle aplikace)
export async function brokerHost(broker: Broker) {
  if (broker.publicHost) return broker.publicHost;

  const list = await headers();
  const host = list.get("x-forwarded-host") ?? list.get("host") ?? "localhost";

  return host.replace(/:\d+$/, "");
}
