import type { Metadata } from "next";
import Link from "next/link";
import { ChevronRight, Radio, RadioTower } from "lucide-react";
import { getFacilities, getGateways, getSession } from "@/lib/api";
import { formatAgo } from "@/lib/sensor-meta";
import { readParam } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import { TopBar } from "../_components/top-bar";
import { brokerHost } from "./broker-host";
import { ConnectGatewayForm } from "./_components/connect-gateway-form";
import { SetupGuide } from "./_components/setup-guide";

export const metadata: Metadata = {
  title: "Senzory · SmartAudit KHK",
  description: "Připojení skutečných čidel IQRF — gateway, uzly, přiřazení do místností.",
};

export default async function SensorsPage({ searchParams }: PageProps<"/senzory">) {
  const session = await getSession();

  if (session?.role === "Verejnost") {
    return (
      <>
        <TopBar title="Senzory" />
        <main className="min-h-0 flex-1 overflow-y-auto p-6">
          <p className="max-w-xl rounded-2xl border border-slate-200 bg-white p-5 text-sm text-slate-600">
            Připojování čidel je pro kraj a správce budov. Naměřené hodnoty vidíte v mapě u každého objektu.
          </p>
        </main>
      </>
    );
  }

  const params = await searchParams;
  const [data, facilities] = await Promise.all([getGateways(), getFacilities()]);
  const host = await brokerHost(data.broker);

  const options = facilities
    .toSorted((a, b) => a.name.localeCompare(b.name, "cs"))
    .map((f) => ({ value: f.id, label: `${f.code} · ${f.name}`, district: f.districtName }));

  return (
    <>
      <TopBar title="Senzory" />

      <main className="min-h-0 flex-1 overflow-y-auto p-6">
        <div className="flex flex-wrap items-end justify-between gap-3">
          <div>
            <h1 className="text-[32px] leading-tight font-bold tracking-tight">Skutečná čidla IQRF</h1>
            <p className="mt-1.5 max-w-2xl text-sm leading-normal text-slate-600">
              IQRF gateway v budově posílá přes MQTT hodnoty ze senzorů. Aplikace sama najde uzly a veličiny, vy jen přiřadíte místnost —
              grafy, audit i PDF pak místo simulace ukazují skutečná měření.
            </p>
          </div>
          <BrokerBadge connected={data.broker.connected} error={data.broker.error} address={`${host}:${data.broker.publicPort}`} />
        </div>

        <div className="mt-6 grid gap-4 xl:grid-cols-[1fr_26rem]">
          <div className="min-w-0 space-y-4">
            <section aria-labelledby="gateways" className="rounded-2xl border border-slate-200 bg-white">
              <h2 id="gateways" className="border-b border-slate-100 px-5 py-3.5 text-[11px] font-bold tracking-wide text-slate-500 uppercase">
                Připojené gatewaye
              </h2>
              {data.gateways.length === 0 ? (
                <p className="px-5 py-8 text-center text-sm text-slate-500">Zatím žádná gateway. Připojte první formulářem vpravo.</p>
              ) : (
                <ul className="divide-y divide-slate-100">
                  {data.gateways.map((gateway) => (
                    <li key={gateway.id}>
                      <Link href={`/senzory/${gateway.id}`} className="flex items-center gap-4 px-5 py-3.5 transition-colors hover:bg-slate-50">
                        <span className="relative flex size-9 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-hk-navy">
                          <RadioTower className="size-4.5" strokeWidth={1.75} aria-hidden />
                          <span
                            className={cn("absolute -top-0.5 -right-0.5 size-2.5 rounded-full border-2 border-white", gateway.online ? "bg-emerald-500" : "bg-slate-300")}
                            aria-label={gateway.online ? "online" : "offline"}
                          />
                        </span>
                        <span className="min-w-0 flex-1">
                          <span className="block truncate text-[13.5px] font-bold">{gateway.name}</span>
                          <span className="block truncate text-xs text-slate-500">
                            {gateway.facilityCode} · {gateway.facilityName}
                          </span>
                        </span>
                        <span className="hidden shrink-0 text-right font-mono text-[11px] text-slate-500 sm:block">
                          <span className="block">
                            {gateway.assignedCount}/{gateway.channelCount} kanálů v místnostech
                          </span>
                          <span className="block">{gateway.online ? "online" : "offline"} · {formatAgo(gateway.lastSeenAt)}</span>
                        </span>
                        <ChevronRight className="size-4 shrink-0 text-slate-400" aria-hidden />
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
            </section>

            {data.unknown.length > 0 && (
              <section aria-labelledby="unknown" className="rounded-2xl border border-amber-200 bg-amber-50 p-5">
                <h2 id="unknown" className="text-[13.5px] font-bold text-amber-900">
                  Ozývají se, ale nejsou připojené
                </h2>
                <p className="mt-0.5 text-xs text-amber-800">Na brokeru posílají odpovědi — stačí je přiřadit k objektu.</p>
                <ul className="mt-3 space-y-2">
                  {data.unknown.map((gateway) => (
                    <li key={gateway.gatewayId} className="flex items-center gap-3 rounded-lg bg-white px-3 py-2">
                      <Radio className="size-4 text-amber-600" aria-hidden />
                      <code className="flex-1 font-mono text-[12.5px] font-bold">{gateway.gatewayId}</code>
                      <span className="font-mono text-[11px] text-slate-500">{formatAgo(gateway.lastSeenAt)}</span>
                      <Link
                        href={`/senzory?gateway=${encodeURIComponent(gateway.gatewayId)}#pripojit`}
                        className="rounded-md border border-amber-300 px-2 py-1 text-[12px] font-bold text-amber-900 hover:bg-amber-100"
                      >
                        Připojit
                      </Link>
                    </li>
                  ))}
                </ul>
              </section>
            )}

            <section aria-labelledby="how" className="rounded-2xl border border-slate-200 bg-white p-5">
              <h2 id="how" className="mb-4 text-[11px] font-bold tracking-wide text-slate-500 uppercase">
                Jak připojit IQRF gateway
              </h2>
              <SetupGuide brokerHost={host} brokerPort={data.broker.publicPort} gatewayId={readParam(params, "gateway")} />
            </section>
          </div>

          <section id="pripojit" aria-labelledby="connect" className="h-fit rounded-2xl border border-slate-200 bg-white p-5 xl:sticky xl:top-0">
            <div className="flex items-center gap-2.5">
              <RadioTower className="size-5 text-hk-blue" aria-hidden />
              <h2 id="connect" className="text-lg font-bold tracking-tight">
                Připojit IQRF gateway
              </h2>
            </div>
            <p className="mt-1.5 text-[13px] text-slate-600">Id gatewaye je část MQTT topicu, kterou má gateway nastavenou v IQRF Gateway Daemonu.</p>
            <ConnectGatewayForm facilities={options} suggestedGatewayId={readParam(params, "gateway")} />
          </section>
        </div>
      </main>
    </>
  );
}

function BrokerBadge({ connected, error, address }: { connected: boolean; error: string | null; address: string }) {
  return (
    <p
      className={cn(
        "inline-flex items-center gap-2 rounded-lg border px-3 py-2 text-[12.5px]",
        connected ? "border-emerald-200 bg-emerald-50 text-emerald-900" : "border-red-200 bg-red-50 text-red-900",
      )}
      title={error ?? undefined}
    >
      <span className={cn("size-2 rounded-full", connected ? "animate-pulse bg-emerald-500" : "bg-red-500")} aria-hidden />
      {connected ? "Broker MQTT běží" : "Broker MQTT nedostupný"}
      <code className="font-mono text-[11.5px] opacity-80">{address}</code>
    </p>
  );
}
