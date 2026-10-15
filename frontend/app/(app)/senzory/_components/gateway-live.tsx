"use client";

import { useEffect, useState, useTransition, type ReactNode } from "react";
import Link from "next/link";
import { ArrowDownLeft, ArrowUpRight, Check, ChevronDown, Loader2, MapPin, Trash2 } from "lucide-react";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { METRIC_META, METRICS, formatAgo, formatSensorValue } from "@/lib/sensor-meta";
import { cn } from "@/lib/utils";
import type { GatewayChannel, GatewayDetail, SensorMetric } from "@/types/api";
import { assignChannel, deleteGateway } from "../actions";

// Průvodce obnovuje stav každé 2 s — hodnoty z gatewaye jsou vidět hned, jak přijdou
const REFRESH_MS = 2000;

const WHOLE_BUILDING = "objekt";

export function GatewayLive({ initial, guide }: { initial: GatewayDetail; guide: ReactNode }) {
  const [gateway, setGateway] = useState(initial);
  const [now, setNow] = useState(() => Date.now());

  async function refresh() {
    const response = await fetch(`/senzory/${initial.id}/stav`, { cache: "no-store" }).catch(() => null);
    if (response?.ok) setGateway((await response.json()) as GatewayDetail);
    setNow(Date.now());
  }

  useEffect(() => {
    const timer = setInterval(refresh, REFRESH_MS);
    return () => clearInterval(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initial.id]);

  const nodes = [...new Set(gateway.channels.map((c) => c.nodeAddress))].sort((a, b) => a - b);

  return (
    <div className="space-y-4">
      <header className="flex flex-wrap items-start justify-between gap-4 rounded-2xl border border-slate-200 bg-white p-5">
        <div className="min-w-0">
          <p className="font-mono text-[11px] text-hk-blue">gateway/{gateway.gatewayId}/iqrf</p>
          <h1 className="mt-1 text-[28px] leading-tight font-bold tracking-tight">{gateway.name}</h1>
          <Link href={`/mapa?objekt=${gateway.facility.id}`} className="mt-1 inline-flex items-center gap-1.5 text-[13px] text-slate-600 hover:text-hk-red hover:underline">
            <MapPin className="size-3.5" aria-hidden />
            {gateway.facility.code} · {gateway.facility.name}
          </Link>
        </div>
        <div className="flex flex-col items-end gap-2">
          <StatusPill online={gateway.online} lastSeenAt={gateway.lastSeenAt} now={now} />
          <p className="font-mono text-[11px] text-slate-500">
            uzly {gateway.nodes.length > 0 ? gateway.nodes.join(", ") : "—"} · dotaz každých {gateway.pollSeconds} s
          </p>
          <DeleteGatewayButton gatewayId={gateway.id} />
        </div>
      </header>

      {gateway.channels.length === 0 ? (
        <section className="grid gap-4 lg:grid-cols-[22rem_1fr]">
          <div className="h-fit rounded-2xl border border-slate-200 bg-white p-5">
            <p className="flex items-center gap-2 text-[15px] font-bold">
              <Loader2 className="size-4 animate-spin text-hk-red" aria-hidden />
              Čekám na první data…
            </p>
            <ul className="mt-4 space-y-2.5 text-[13px]">
              <Check_ done={gateway.broker.connected} label="Aplikace je připojená k brokeru MQTT" hint={gateway.broker.error ?? undefined} />
              <Check_ done={gateway.lastSeenAt !== null} label="Gateway odpověděla" hint={gateway.lastSeenAt ? formatAgo(gateway.lastSeenAt, now) : "zatím ne — zkontrolujte topic a adresu brokeru"} />
              <Check_ done={gateway.nodes.length > 0} label="Nalezené uzly se senzory" hint={gateway.nodes.length > 0 ? gateway.nodes.join(", ") : undefined} />
            </ul>
          </div>
          <div className="rounded-2xl border border-slate-200 bg-white p-5">{guide}</div>
        </section>
      ) : (
        <section aria-labelledby="channels" className="rounded-2xl border border-slate-200 bg-white">
          <div className="flex flex-wrap items-baseline justify-between gap-2 border-b border-slate-100 px-5 py-3.5">
            <h2 id="channels" className="text-[11px] font-bold tracking-wide text-slate-500 uppercase">
              Senzory na uzlech
            </h2>
            <p className="text-xs text-slate-500">Přiřazený kanál se ukládá a nahradí simulaci v dané místnosti.</p>
          </div>
          {nodes.map((node) => (
            <div key={node} className="border-b border-slate-100 last:border-b-0">
              <p className="bg-slate-50 px-5 py-1.5 font-mono text-[11px] text-slate-500">uzel {node}</p>
              <ul className="divide-y divide-slate-100">
                {gateway.channels
                  .filter((c) => c.nodeAddress === node)
                  .map((channel) => (
                    <ChannelRow key={channel.id} gatewayId={gateway.id} channel={channel} zones={gateway.zones} onAssigned={refresh} now={now} />
                  ))}
              </ul>
            </div>
          ))}
        </section>
      )}

      <MessageLog messages={gateway.messages} />
    </div>
  );
}

function StatusPill({ online, lastSeenAt, now }: { online: boolean; lastSeenAt: string | null; now: number }) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-2 rounded-full border px-3 py-1 text-[12px] font-bold",
        online ? "border-emerald-200 bg-emerald-50 text-emerald-800" : "border-slate-200 bg-slate-50 text-slate-600",
      )}
    >
      <span className={cn("size-2 rounded-full", online ? "animate-pulse bg-emerald-500" : "bg-slate-300")} aria-hidden />
      {online ? "Online" : lastSeenAt ? "Neodpovídá" : "Čeká na gateway"}
      <span className="font-normal opacity-75">{formatAgo(lastSeenAt, now)}</span>
    </span>
  );
}

function Check_({ done, label, hint }: { done: boolean; label: string; hint?: string }) {
  return (
    <li className="flex gap-2.5">
      <span
        className={cn(
          "mt-0.5 flex size-4.5 shrink-0 items-center justify-center rounded-full border",
          done ? "border-emerald-500 bg-emerald-500 text-white" : "border-slate-300 bg-white",
        )}
        aria-hidden
      >
        {done && <Check className="size-3" strokeWidth={3} />}
      </span>
      <span>
        <span className={done ? "text-hk-navy" : "text-slate-500"}>{label}</span>
        {hint && <span className="block text-[11.5px] text-slate-500">{hint}</span>}
      </span>
    </li>
  );
}

type ChannelRowProps = {
  gatewayId: string;
  channel: GatewayChannel;
  zones: GatewayDetail["zones"];
  onAssigned: () => void;
  now: number;
};

function ChannelRow({ gatewayId, channel, zones, onAssigned, now }: ChannelRowProps) {
  const [zone, setZone] = useState<string>(channel.zoneId ?? (channel.sensorId ? WHOLE_BUILDING : ""));
  const [metric, setMetric] = useState<SensorMetric>(channel.metric);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const zoneItems = [{ value: WHOLE_BUILDING, label: "Celý objekt" }, ...zones.map((z) => ({ value: z.id, label: `${z.name} · ${z.floor === 0 ? "přízemí" : `${z.floor}. NP`}` }))];
  const metricItems = METRICS.map((m) => ({ value: m, label: METRIC_META[m].label }));
  const assigned = channel.sensorId !== null;
  const changed = zone !== (channel.zoneId ?? (assigned ? WHOLE_BUILDING : "")) || metric !== channel.metric;

  function save() {
    if (!zone) return;
    setError(null);
    startTransition(async () => {
      const result = await assignChannel(gatewayId, channel.id, zone === WHOLE_BUILDING ? null : zone, metric);
      if (result?.error) setError(result.error);
      else onAssigned();
    });
  }

  return (
    <li className="grid items-center gap-3 px-5 py-3 md:grid-cols-[minmax(0,1.2fr)_8rem_7.5rem_minmax(0,1.6fr)]">
      <div className="min-w-0">
        <p className="truncate text-[13.5px] font-bold">{METRIC_META[channel.metric].label}</p>
        <p className="truncate font-mono text-[11px] text-slate-500">
          {channel.name} · {channel.channelKey}
        </p>
      </div>

      <div>
        <p className="font-mono text-[18px] font-bold text-hk-navy tabular-nums">{formatSensorValue(channel.lastValue, channel.metric, channel.unit)}</p>
        <p className="text-[11px] text-slate-500">{formatAgo(channel.lastSeenAt, now)}</p>
      </div>

      <Sparkline values={channel.samples.map((s) => s.value)} />

      <div className="flex flex-wrap items-center gap-2">
        {channel.metric === "Other" && (
          <Select items={metricItems} value={metric} onValueChange={(next) => next && setMetric(next as SensorMetric)}>
            <SelectTrigger aria-label="Veličina" className="h-9 w-40 border-slate-300 bg-white text-[13px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent alignItemWithTrigger={false} className="min-w-(--anchor-width) text-[13px]">
              {metricItems.map((item) => (
                <SelectItem key={item.value} value={item.value}>
                  {item.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        )}
        <Select items={zoneItems} value={zone || null} onValueChange={(next) => setZone((next as string | null) ?? "")}>
          <SelectTrigger aria-label="Místnost" className="h-9 min-w-44 flex-1 border-slate-300 bg-white text-[13px]">
            <SelectValue placeholder="Vyberte místnost…" />
          </SelectTrigger>
          <SelectContent alignItemWithTrigger={false} className="w-max min-w-(--anchor-width) text-[13px]">
            {zoneItems.map((item) => (
              <SelectItem key={item.value} value={item.value}>
                {item.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        {assigned && !changed ? (
          <span className="inline-flex h-9 items-center gap-1.5 rounded-lg bg-emerald-50 px-2.5 text-[12px] font-bold text-emerald-800">
            <span className="size-1.5 animate-pulse rounded-full bg-emerald-500" aria-hidden />
            Ukládá se
          </span>
        ) : (
          <button
            type="button"
            onClick={save}
            disabled={!zone || isPending}
            className="inline-flex h-9 items-center gap-1.5 rounded-lg bg-hk-accent px-3 text-[12.5px] font-bold text-white transition-colors hover:bg-hk-red disabled:opacity-50"
          >
            {isPending && <Loader2 className="size-3.5 animate-spin" aria-hidden />}
            {assigned ? "Přesunout" : "Přiřadit"}
          </button>
        )}
        {error && (
          <p role="alert" className="w-full text-[12px] text-red-700">
            {error}
          </p>
        )}
      </div>
    </li>
  );
}

// Průběh posledních hodnot kanálu — jedna čára bez os, jen tvar
function Sparkline({ values }: { values: number[] }) {
  const width = 120;
  const height = 32;

  if (values.length < 2) return <span className="text-[11px] text-slate-400">sbírám hodnoty…</span>;

  const recent = values.slice(-60);
  const min = Math.min(...recent);
  const max = Math.max(...recent);
  const range = max - min || 1;
  const points = recent.map((v, i) => [(i / (recent.length - 1)) * width, height - 3 - ((v - min) / range) * (height - 6)]);

  return (
    <svg viewBox={`0 0 ${width} ${height}`} width={width} height={height} aria-hidden className="overflow-visible">
      <polyline points={points.map(([x, y]) => `${x.toFixed(1)},${y.toFixed(1)}`).join(" ")} fill="none" stroke="#1e3e62" strokeWidth={1.8} strokeLinejoin="round" strokeLinecap="round" />
      <circle cx={points.at(-1)![0]} cy={points.at(-1)![1]} r={2.6} fill="#e63946" />
    </svg>
  );
}

function MessageLog({ messages }: { messages: GatewayDetail["messages"] }) {
  const [open, setOpen] = useState(false);

  return (
    <section className="rounded-2xl border border-slate-200 bg-white">
      <button type="button" onClick={() => setOpen(!open)} aria-expanded={open} className="flex w-full items-center justify-between px-5 py-3.5 text-left">
        <span className="text-[11px] font-bold tracking-wide text-slate-500 uppercase">Komunikace s gatewayí ({messages.length})</span>
        <ChevronDown className={cn("size-4 text-slate-400 transition-transform", open && "rotate-180")} aria-hidden />
      </button>
      {open && (
        <ul className="max-h-96 divide-y divide-slate-100 overflow-y-auto border-t border-slate-100">
          {messages.length === 0 && <li className="px-5 py-4 text-[13px] text-slate-500">Zatím žádné zprávy.</li>}
          {messages.map((message, index) => (
            <li key={`${message.timestamp}-${index}`} className="px-5 py-2.5">
              <p className="flex items-center gap-2 text-[12px]">
                {message.direction === "Request" ? (
                  <ArrowUpRight className="size-3.5 text-hk-blue" aria-label="dotaz aplikace" />
                ) : (
                  <ArrowDownLeft className="size-3.5 text-emerald-600" aria-label="odpověď gatewaye" />
                )}
                <code className="font-mono font-bold">{message.messageType}</code>
                <span className="ml-auto font-mono text-[11px] text-slate-400">{new Date(message.timestamp).toLocaleTimeString("cs-CZ")}</span>
              </p>
              <pre className="mt-1 max-h-24 overflow-auto rounded-md bg-slate-50 p-2 font-mono text-[10.5px] leading-snug text-slate-600">{message.payload}</pre>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

// Odebrání ve dvou krocích — smaže i uložená měření skutečných čidel
function DeleteGatewayButton({ gatewayId }: { gatewayId: string }) {
  const [confirming, setConfirming] = useState(false);
  const [isPending, startTransition] = useTransition();

  return (
    <button
      type="button"
      disabled={isPending}
      onClick={() => (confirming ? startTransition(async () => void (await deleteGateway(gatewayId))) : setConfirming(true))}
      onBlur={() => setConfirming(false)}
      className={cn(
        "inline-flex h-8 items-center gap-1.5 rounded-lg border px-2.5 text-[12px] font-bold transition-colors",
        confirming ? "border-red-300 bg-red-50 text-red-800" : "border-slate-200 text-slate-600 hover:bg-slate-50",
      )}
    >
      <Trash2 className="size-3.5" aria-hidden />
      {confirming ? "Opravdu odebrat i s měřeními?" : "Odebrat gateway"}
    </button>
  );
}
