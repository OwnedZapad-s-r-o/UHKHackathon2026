import Link from "next/link";
import { ChevronRight, RadioTower } from "lucide-react";
import { formatAgo } from "@/lib/sensor-meta";
import { cn } from "@/lib/utils";
import type { GatewayListItem } from "@/types/api";

export type GatewayGroup = "attention" | "ok" | "virtual";

// Zásah potřebuje gateway offline, bez objevených čidel nebo s čidly mimo místnosti
export function needsAttention(gateway: GatewayListItem) {
  return !gateway.online || gateway.channelCount === 0 || gateway.assignedCount < gateway.channelCount;
}

// Virtuální gatewaye mají vlastní skupinu, aby skutečné zařízení nezapadlo mezi demo
export function gatewayGroup(gateway: GatewayListItem): GatewayGroup {
  if (gateway.isVirtual) return "virtual";
  return needsAttention(gateway) ? "attention" : "ok";
}

export function GatewayRow({ gateway }: { gateway: GatewayListItem }) {
  return (
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
      <GatewayStatus gateway={gateway} />
      <span className="hidden shrink-0 text-right font-mono text-[11px] text-slate-500 md:block">{formatAgo(gateway.lastSeenAt)}</span>
      <ChevronRight className="size-4 shrink-0 text-slate-400" aria-hidden />
    </Link>
  );
}

// Poctivé označení: data posílá aplikace místo hardwaru
export function VirtualBadge() {
  return (
    <span className="shrink-0 rounded-full border border-hk-blue/25 bg-[#EFF4FA] px-2 py-0.5 text-[10.5px] font-bold text-hk-blue" title="Gateway bez hardwaru — odpovídá za ni aplikace přes MQTT">
      virtuální
    </span>
  );
}

// Co s gatewayí je — stejná slova, podle kterých se řadí do „Vyžaduje pozornost“
function GatewayStatus({ gateway }: { gateway: GatewayListItem }) {
  const { online, channelCount: total, assignedCount: assigned } = gateway;
  const done = online && total > 0 && assigned === total;
  const label = !online
    ? total === 0
      ? "čeká na gateway"
      : "offline"
    : total === 0
      ? "hledá čidla"
      : done
        ? "vše v místnostech"
        : `${assigned}/${total} čidel v místnostech`;

  return (
    <span
      className={cn(
        "hidden shrink-0 rounded-full px-2.5 py-1 text-[11.5px] font-bold sm:inline-flex",
        done ? "bg-emerald-50 text-emerald-800" : !online && total > 0 ? "bg-red-50 text-red-800" : "bg-amber-50 text-amber-800",
      )}
    >
      {label}
    </span>
  );
}
