import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ChevronLeft } from "lucide-react";
import { getGateway } from "@/lib/api";
import { TopBar } from "../../_components/top-bar";
import { brokerHost } from "../broker-host";
import { GatewayLive } from "../_components/gateway-live";
import { SensorTourLink } from "../_components/sensor-tour-link";
import { SetupGuide } from "../_components/setup-guide";

export const metadata: Metadata = {
  title: "Gateway · Senzory · SmartAudit KHK",
};

export default async function GatewayPage({ params }: PageProps<"/senzory/[id]">) {
  const { id } = await params;
  if (!/^[0-9a-f-]{36}$/i.test(id)) notFound();

  const gateway = await getGateway(id).catch(() => null);
  if (!gateway) notFound();

  const host = await brokerHost(gateway.broker);

  return (
    <>
      <TopBar title="Senzory">
        <Link href="/senzory" className="inline-flex items-center gap-1 text-[13px] text-slate-600 hover:text-hk-navy">
          <ChevronLeft className="size-4" aria-hidden />
          Všechny gatewaye
        </Link>
        <SensorTourLink href={`/senzory/${gateway.id}?pruvodce=1`} />
      </TopBar>

      <main className="min-h-0 flex-1 overflow-y-auto p-6">
        <GatewayLive
          initial={gateway}
          guide={<SetupGuide brokerHost={host} broker={gateway.broker} gatewayId={gateway.gatewayId} credentials={gateway.credentials} />}
        />
      </main>
    </>
  );
}
