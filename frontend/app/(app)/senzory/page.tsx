import type { Metadata } from "next";
import Link from "next/link";
import { ChevronRight, Cpu, Radio, RadioTower } from "lucide-react";
import { getFacilities, getGateways, getSession } from "@/lib/api";
import { DEMO_NETWORK_GATEWAYS, formatAgo } from "@/lib/sensor-meta";
import { readParam } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import { TopBar } from "../_components/top-bar";
import { brokerHost } from "./broker-host";
import { ConnectGatewayForm } from "./_components/connect-gateway-form";
import { DemoNetworkPanel } from "./_components/demo-network-panel";
import { SensorTourLink } from "./_components/sensor-tour-link";
import { SENSOR_STEPS, SensorSteps } from "./_components/sensor-steps";
import { Code, emulatorCommand } from "./_components/setup-guide";

export const metadata: Metadata = {
  title: "Senzory · SmartAudit KHK",
  description: "Připojení skutečných čidel IQRF — gateway, uzly, přiřazení do místností.",
};

// Id pro emulátor, když uživatel nemá hardware — stejné musí zadat do formuláře
const EMULATOR_ID = "ucebna-1";

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
  const suggestedId = readParam(params, "gateway");

  const options = facilities
    .toSorted((a, b) => a.name.localeCompare(b.name, "cs"))
    .map((f) => ({ value: f.id, label: `${f.code} · ${f.name}`, district: f.districtName }));

  return (
    <>
      <TopBar title="Senzory">
        <SensorTourLink href="/senzory?pruvodce=1" />
      </TopBar>

      <main className="min-h-0 flex-1 overflow-y-auto p-6">
        <div className="flex flex-wrap items-end justify-between gap-3">
          <div>
            <h1 className="text-[32px] leading-tight font-bold tracking-tight">Skutečná čidla IQRF</h1>
            <p className="mt-1.5 max-w-2xl text-sm leading-normal text-slate-600">
              IQRF gateway v budově posílá přes MQTT hodnoty z čidel. Aplikace sama najde uzly a veličiny, vy jen vyberete místnost — grafy,
              audit i PDF pak místo simulace ukazují skutečná měření.
            </p>
          </div>
          <BrokerBadge connected={data.broker.connected} error={data.broker.error} address={`${host}:${data.broker.publicPort}`} />
        </div>

        <SensorSteps current={1} className="mt-6" />

        {session?.role === "Kraj" && (
          <div className="mt-4">
            <DemoNetworkPanel running={DEMO_NETWORK_GATEWAYS.every((id) => data.gateways.some((g) => g.gatewayId === id))} />
          </div>
        )}

        <div className="mt-4 grid gap-4 xl:grid-cols-[1fr_26rem]">
          <div className="min-w-0 space-y-4">
            {data.unknown.length > 0 && (
              <section aria-labelledby="unknown" className="rounded-2xl border border-amber-200 bg-amber-50 p-5">
                <h2 id="unknown" className="flex items-center gap-2 text-[14px] font-bold text-amber-900">
                  <span className="size-2 animate-pulse rounded-full bg-amber-500" aria-hidden />
                  Našli jsme gateway, která ještě není připojená
                </h2>
                <p className="mt-0.5 text-[13px] text-amber-800">Už posílá data na broker. Klikněte na Připojit a ve formuláři doplňte budovu.</p>
                <ul className="mt-3 space-y-2">
                  {data.unknown.map((gateway) => (
                    <li key={gateway.gatewayId} className="flex items-center gap-3 rounded-lg bg-white px-3 py-2">
                      <Radio className="size-4 text-amber-600" aria-hidden />
                      <code className="flex-1 font-mono text-[12.5px] font-bold">{gateway.gatewayId}</code>
                      <span className="font-mono text-[11px] text-slate-500">{formatAgo(gateway.lastSeenAt)}</span>
                      <Link
                        href={`/senzory?gateway=${encodeURIComponent(gateway.gatewayId)}#pripojit`}
                        className="rounded-md bg-amber-500 px-2.5 py-1 text-[12px] font-bold text-white hover:bg-amber-600"
                      >
                        Připojit
                      </Link>
                    </li>
                  ))}
                </ul>
              </section>
            )}

            <section data-tour="senzory-seznam" aria-labelledby="gateways" className="rounded-2xl border border-slate-200 bg-white">
              <h2 id="gateways" className="border-b border-slate-100 px-5 py-3.5 text-[11px] font-bold tracking-wide text-slate-500 uppercase">
                Připojené gatewaye
              </h2>
              {data.gateways.length === 0 ? (
                <div className="flex items-start gap-4 px-5 py-6">
                  <span className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-slate-100 text-slate-500">
                    <RadioTower className="size-5" aria-hidden />
                  </span>
                  <div className="text-[13.5px]">
                    <p className="font-bold">Zatím tu žádná gateway není</p>
                    <p className="mt-0.5 text-slate-600">
                      Začněte krokem 1 — formulářem <span className="xl:hidden">níže</span>
                      <span className="hidden xl:inline">vpravo</span>. Nemáte hardware? Použijte emulátor, postup je dole.
                    </p>
                  </div>
                </div>
              ) : (
                <ul className="divide-y divide-slate-100">
                  {data.gateways.map((gateway) => (
                    <li key={gateway.id}>
                      <Link
                        href={`/senzory/${gateway.id}`}
                        data-tour-gateway={gateway.channelCount > 0 && gateway.assignedCount < gateway.channelCount ? "neprirazena" : "ok"}
                        className="flex items-center gap-4 px-5 py-3.5 transition-colors hover:bg-slate-50"
                      >
                        <span className="relative flex size-9 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-hk-navy">
                          <RadioTower className="size-4.5" strokeWidth={1.75} aria-hidden />
                          <span
                            className={cn("absolute -top-0.5 -right-0.5 size-2.5 rounded-full border-2 border-white", gateway.online ? "bg-emerald-500" : "bg-slate-300")}
                            aria-label={gateway.online ? "online" : "offline"}
                          />
                        </span>
                        <span className="min-w-0 flex-1">
                          <span className="flex items-center gap-2">
                            <span className="truncate text-[13.5px] font-bold">{gateway.name}</span>
                            {gateway.isVirtual && <VirtualBadge />}
                          </span>
                          <span className="block truncate text-xs text-slate-500">
                            {gateway.facilityCode} · {gateway.facilityName}
                          </span>
                        </span>
                        <GatewayProgress assigned={gateway.assignedCount} total={gateway.channelCount} online={gateway.online} />
                        <span className="hidden shrink-0 text-right font-mono text-[11px] text-slate-500 md:block">{formatAgo(gateway.lastSeenAt)}</span>
                        <ChevronRight className="size-4 shrink-0 text-slate-400" aria-hidden />
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
            </section>

            <section aria-labelledby="next" className="rounded-2xl border border-slate-200 bg-white p-5">
              <h2 id="next" className="text-[11px] font-bold tracking-wide text-slate-500 uppercase">
                Co bude následovat
              </h2>
              <ol className="mt-3 grid gap-3 md:grid-cols-3">
                {SENSOR_STEPS.slice(1, 4).map((step, index) => (
                  <li key={step.title} className="rounded-xl bg-slate-50 p-3.5">
                    <span className="font-mono text-[11px] font-bold text-hk-red">krok {index + 2}</span>
                    <p className="mt-0.5 text-[13.5px] font-bold">{step.title}</p>
                    <p className="mt-1 text-[12.5px] leading-snug text-slate-600">{NEXT_TEXT[index]}</p>
                  </li>
                ))}
              </ol>
            </section>

            <section aria-labelledby="emulator" className="rounded-2xl border border-slate-200 bg-white p-5">
              <h2 id="emulator" className="flex items-center gap-2 text-[14px] font-bold">
                <Cpu className="size-4.5 text-hk-blue" aria-hidden />
                Nemáte hardware? Vyzkoušejte to s emulátorem
              </h2>
              <p className="mt-1 text-[13px] text-slate-600">
                Emulátor se chová jako IQRF gateway se dvěma uzly (teplota, CO₂, vlhkost, osvětlení a pohyb). Ve formuláři zadejte id{" "}
                <code className="rounded bg-slate-100 px-1 font-mono text-[12px] font-bold">{EMULATOR_ID}</code> — broker pustí jen připojené gatewaye.
                Heslo a hotový příkaz pak najdete na detailu gatewaye.
              </p>
              <Code code={emulatorCommand(host, EMULATOR_ID, { tls: data.broker.publicTls, port: data.broker.publicPort })} />
            </section>
          </div>

          <section id="pripojit" data-tour="senzory-formular" aria-labelledby="connect" className="h-fit scroll-mt-4 rounded-2xl border-2 border-hk-red/20 bg-white p-5 xl:sticky xl:top-0">
            <p className="font-mono text-[11px] font-bold text-hk-red">krok 1</p>
            <h2 id="connect" className="mt-0.5 flex items-center gap-2.5 text-lg font-bold tracking-tight">
              <RadioTower className="size-5 text-hk-blue" aria-hidden />
              Zadejte gateway do aplikace
            </h2>
            <p className="mt-1.5 text-[13px] text-slate-600">
              Vyberte budovu, kde gateway je, a zadejte její id — to je část MQTT topicu <code className="font-mono text-[12px]">gateway/&#123;id&#125;/iqrf</code>.
              Po uložení vám ukážeme, co nastavit v gateway, a počkáme na první data.
            </p>
            <ConnectGatewayForm facilities={options} suggestedGatewayId={suggestedId} />
          </section>
        </div>
      </main>
    </>
  );
}

const NEXT_TEXT = [
  "Ukážeme adresu brokeru a hotové nastavení pro IQRF Gateway Webapp — stačí zkopírovat.",
  "Aplikace se sama zeptá na spárované uzly a jejich čidla. Uvidíte je naživo.",
  "U každého čidla vyberete místnost. Skutečná měření pak nahradí simulaci.",
];

// Poctivé označení: data posílá aplikace místo hardwaru
function VirtualBadge() {
  return (
    <span className="shrink-0 rounded-full border border-hk-blue/25 bg-[#EFF4FA] px-2 py-0.5 text-[10.5px] font-bold text-hk-blue" title="Gateway bez hardwaru — odpovídá za ni aplikace přes MQTT">
      virtuální
    </span>
  );
}

function GatewayProgress({ assigned, total, online }: { assigned: number; total: number; online: boolean }) {
  const done = total > 0 && assigned === total;
  const label = total === 0 ? (online ? "hledá čidla" : "čeká na gateway") : done ? "vše v místnostech" : `${assigned}/${total} čidel v místnostech`;

  return (
    <span
      className={cn(
        "hidden shrink-0 rounded-full px-2.5 py-1 text-[11.5px] font-bold sm:inline-flex",
        done ? "bg-emerald-50 text-emerald-800" : "bg-amber-50 text-amber-800",
      )}
    >
      {label}
    </span>
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
