import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ChevronLeft, Landmark, QrCode, Wind } from "lucide-react";
import { LiveBadge } from "@/components/live-badge";
import { AIR_META, airLevel } from "@/lib/air-quality";
import { formatNumber } from "@/lib/facility-meta";
import { getPublicCo2, getPublicFacilityByCode, getPublicFacilityDetail, getPublicSection } from "@/lib/public-api";
import { cn } from "@/lib/utils";
import type { Telemetry, TelemetryPoint } from "@/types/api";
import { Co2Chart } from "./_components/co2-chart";

export async function generateMetadata({ params }: PageProps<"/objekty/[kod]">): Promise<Metadata> {
  const { kod } = await params;
  const facility = await getPublicFacilityByCode(kod);

  return {
    title: facility ? `${facility.name} · Jak se tu dýchá` : "Objekt nenalezen · SmartAudit KHK",
    description: facility ? `Kvalita vzduchu, teplota a kdy větrat — ${facility.name}, ${facility.municipalityName}.` : undefined,
  };
}

// Veřejná karta objektu — pro rodiče, žáky, seniory a jejich blízké; bez přihlášení a bez částek v Kč
export default async function PublicFacilityPage({ params }: PageProps<"/objekty/[kod]">) {
  const { kod } = await params;
  const facility = await getPublicFacilityByCode(kod);
  if (!facility) notFound();

  const [detail, section, co2] = await Promise.all([getPublicFacilityDetail(facility.id), getPublicSection(facility.id), getPublicCo2(facility.id)]);

  const level = airLevel(facility.co2);
  const findings = detail.assessment?.findings ?? [];
  const hoursOverLimit = detail.assessment?.co2HoursOverLimit ?? 0;

  return (
    <main className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
      <Link href="/objekty" className="inline-flex items-center gap-1 text-[13px] text-slate-500 hover:text-hk-red">
        <ChevronLeft className="size-4" aria-hidden />
        Všechny objekty
      </Link>

      <div className="mt-4 flex flex-wrap items-start justify-between gap-4">
        <div className="min-w-0">
          <p className="text-[13px] text-slate-500">
            {facility.kind} · okres {facility.districtName}
          </p>
          <h1 className="mt-1 text-[30px] leading-tight font-bold tracking-tight sm:text-[38px]">{facility.name}</h1>
          <p className="mt-1 text-[14px] text-slate-600">{detail.address.label}</p>
        </div>
        <Link
          href={`/objekty/${facility.code}/qr`}
          className="inline-flex h-10 items-center gap-2 rounded-lg border border-slate-300 px-3.5 text-[13px] font-bold transition-colors hover:bg-slate-50"
        >
          <QrCode className="size-4" aria-hidden />
          QR kód na dveře
        </Link>
      </div>

      {/* Hlavní odpověď na „jak se tu dýchá“ */}
      <section
        className={cn("mt-6 rounded-3xl border p-6 sm:p-8", level ? AIR_META[level].pill : "border-slate-200 bg-slate-50")}
        aria-labelledby="air-now"
      >
        <p className="flex items-center gap-2 text-[13px] font-bold opacity-80">
          <Wind className="size-4" aria-hidden />
          Jak se tu dýchá
          {facility.hasLiveSensors ? <LiveBadge /> : <span className="font-normal">· modelová data, čidla se instalují</span>}
        </p>
        <h2 id="air-now" className="mt-2 text-[32px] leading-tight font-bold tracking-tight sm:text-[40px]">
          {level ? AIR_META[level].label : "Zatím bez měření"}
        </h2>
        <p className="mt-2 max-w-2xl text-[15px] leading-relaxed">{level ? AIR_META[level].advice : "Hodnoty se objeví po prvním měření."}</p>
        <dl className="mt-5 flex flex-wrap gap-x-8 gap-y-3">
          <Figure label="CO₂ v nejhorší místnosti" value={formatNumber(facility.co2, "ppm")} />
          <Figure label="Průměrná teplota" value={formatNumber(facility.temperature, "°C")} />
          <Figure label="Hodin nad 1 500 ppm za týden" value={formatNumber(hoursOverLimit, "h")} />
        </dl>
      </section>

      <div className="mt-6 grid gap-4 lg:grid-cols-[1fr_1.2fr]">
        <section className="rounded-2xl border border-slate-200 p-5" aria-labelledby="rooms">
          <h2 id="rooms" className="text-lg font-bold tracking-tight">
            Místnosti teď
          </h2>
          <ul className="mt-3 divide-y divide-slate-100">
            {section.zones.map((zone) => {
              const zoneLevel = airLevel(zone.co2);

              return (
                <li key={zone.id} className="flex items-center gap-3 py-2.5">
                  <span className={cn("size-2.5 shrink-0 rounded-full", zoneLevel ? AIR_META[zoneLevel].dot : "bg-slate-300")} aria-hidden />
                  <span className="min-w-0 flex-1">
                    <span className="flex items-center gap-1.5 text-[14px] font-bold">
                      <span className="truncate">{zone.name}</span>
                      {zone.live && <LiveBadge />}
                    </span>
                    <span className="block text-[12px] text-slate-500">{zoneLevel ? AIR_META[zoneLevel].label : "bez měření"}</span>
                  </span>
                  <span className="shrink-0 text-right font-mono text-[12.5px] tabular-nums">
                    <span className="block">{formatNumber(zone.co2, "ppm")}</span>
                    <span className="block text-slate-500">
                      {formatNumber(zone.temperature, "°C")}
                      {zone.humidity !== null && ` · ${formatNumber(zone.humidity, "%")}`}
                    </span>
                  </span>
                </li>
              );
            })}
          </ul>
        </section>

        <section className="rounded-2xl border border-slate-200 p-5" aria-labelledby="co2-day">
          <h2 id="co2-day" className="text-lg font-bold tracking-tight">
            CO₂ za posledních 24 hodin
          </h2>
          <p className="mt-0.5 text-[12.5px] text-slate-500">Nejhorší místnost v každé hodině. Nad čárkovanou čarou 1 500 ppm je vzduch vydýchaný.</p>
          <div className="mt-3">
            <Co2Chart points={worstPerHour(co2)} />
          </div>
        </section>
      </div>

      <section className="mt-4 rounded-2xl border border-slate-200 p-5" aria-labelledby="findings">
        <h2 id="findings" className="text-lg font-bold tracking-tight">
          Co tu kraj řeší
        </h2>
        {findings.length === 0 ? (
          <p className="mt-2 text-[14px] text-slate-600">Energetický audit v budově nenašel problém s vytápěním ani větráním.</p>
        ) : (
          <ul className="mt-3 space-y-3">
            {findings.map((finding) => (
              <li key={finding.title} className="rounded-xl bg-slate-50 p-4">
                <p className="text-[14px] font-bold">{finding.title}</p>
                <p className="mt-1 text-[13.5px] leading-relaxed text-slate-600">{finding.detail}</p>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="mt-4 grid gap-4 rounded-2xl border border-slate-200 p-5 sm:grid-cols-3" aria-label="O budově">
        <Fact label="Zřizovatel" value={detail.department || "Královéhradecký kraj"} />
        <Fact label="Rok výstavby (ČÚZK)" value={detail.building?.constructionYear?.toString() ?? "neuveden"} />
        <Fact
          label="Památková ochrana"
          value={detail.heritage.status === "None" ? "bez ochrany" : (detail.heritage.name ?? "chráněná budova")}
          icon={detail.heritage.status !== "None"}
        />
      </section>

      <p className="mt-6 text-[12px] leading-relaxed text-slate-500">
        Zdroje: otevřená data Královéhradeckého kraje (Data KHK), ČÚZK, Open-Meteo. Měření{" "}
        {facility.hasLiveSensors ? "ze skutečných čidel IQRF doplňuje model" : "je zatím modelové (simulace podle budovy a počasí)"}. Data jsou k dispozici i jako{" "}
        <a href="/api/open/docs/" className="underline hover:text-hk-red">
          otevřené API
        </a>
        .
      </p>
    </main>
  );
}

function Figure({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col-reverse">
      <dt className="text-[12px] opacity-75">{label}</dt>
      <dd className="font-mono text-[22px] font-bold tabular-nums">{value}</dd>
    </div>
  );
}

function Fact({ label, value, icon = false }: { label: string; value: string; icon?: boolean }) {
  return (
    <div>
      <p className="text-[12px] text-slate-500">{label}</p>
      <p className="mt-0.5 flex items-center gap-1.5 text-[14px] font-bold">
        {icon && <Landmark className="size-4 text-hk-blue" aria-hidden />}
        {value}
      </p>
    </div>
  );
}

// Z řad po místnostech jedna řada: nejvyšší CO₂ v každé hodině (nejhorší místnost)
function worstPerHour(telemetry: Telemetry): TelemetryPoint[] {
  const byHour = new Map<string, number>();

  for (const series of telemetry.series) {
    for (const point of series.points) {
      byHour.set(point.timestamp, Math.max(byHour.get(point.timestamp) ?? 0, point.value));
    }
  }

  return [...byHour.entries()].sort(([a], [b]) => a.localeCompare(b)).map(([timestamp, value]) => ({ timestamp, value }));
}
