import type { Metadata } from "next";
import { BadgeCheck, BookOpen, Braces, ExternalLink, FileSpreadsheet, Gauge, Library, Map as MapIcon } from "lucide-react";
import { getFacilities } from "@/lib/api";
import { publicBaseUrl } from "@/lib/public-url";
import { CATEGORY_META, DISTRICTS, STATUS_META } from "@/lib/facility-meta";
import type { AuditStatus, FacilityCategory } from "@/types/api";
import { TopBar } from "../_components/top-bar";
import { CopyButton } from "../_components/copy-button";

export const metadata: Metadata = {
  title: "Otevřené API · SmartAudit",
  description: "Data SmartAuditu jako otevřená data — JSON, GeoJSON a CSV bez přihlášení a bez klíče.",
};

const BASE_PATH = "/api/open/v1";
const DOCS_PATH = "/api/open/docs/";
const OPENAPI_PATH = "/api/open/v1/openapi.json";

const DOWNLOADS = [
  { href: `${BASE_PATH}/facilities`, label: "Objekty (JSON)", note: "pro aplikace a skripty", icon: Braces },
  { href: `${BASE_PATH}/facilities.geojson`, label: "Objekty (GeoJSON)", note: "body pro QGIS a webové mapy", icon: MapIcon },
  { href: `${BASE_PATH}/facilities.csv`, label: "Objekty (CSV)", note: "tabulka pro Excel", icon: FileSpreadsheet },
  { href: `${BASE_PATH}/preaudit`, label: "Pre-audit budov (JSON)", note: "parametry A–E se zdroji a sada IQRF", icon: Gauge },
  { href: `${BASE_PATH}/preaudit.csv`, label: "Pre-audit budov (CSV)", note: "skóre, fyzika budovy, IQRF", icon: FileSpreadsheet },
];

// Co SmartAudit sám publikuje jako nová otevřená data (Čl. 5.2b pravidel hackathonu)
const PUBLISHED_FORMATS = ["JSON", "GeoJSON", "CSV", "DCAT-AP-CZ JSON-LD"];

const ENDPOINTS = [
  { path: "/facilities", description: "Seznam objektů, které zřizuje kraj — adresa, poloha, stav auditu, poslední měření", cache: "5 min" },
  { path: "/facilities.geojson", description: "Totéž jako body v GeoJSON (WGS 84)", cache: "5 min" },
  { path: "/facilities.csv", description: "Totéž jako CSV (RFC 4180, UTF-8, čárka)", cache: "5 min" },
  { path: "/facilities/{id}", description: "Detail objektu — organizace, budova z ČÚZK, obsazenost, zjištění auditu", cache: "5 min" },
  { path: "/facilities/{id}/footprint", description: "Půdorys budovy z ČÚZK jako GeoJSON Feature s odhadem výšky", cache: "2 dny" },
  { path: "/facilities/{id}/telemetry", description: "Časová řada měření po zónách — metric, from, to, bucket (nejvýš 31 dní)", cache: "5 min" },
  { path: "/preaudit", description: "Pre-audit budov z otevřených dat — skóre 0–100, parametry A–E s vysvětlením a klíči zdrojových sad, sada IQRF", cache: "5 min" },
  { path: "/preaudit.csv", description: "Totéž jako CSV — body parametrů, měrná ztráta, denostupně, potřeba tepla, velikost a cena sady IQRF", cache: "5 min" },
  { path: "/datasets", description: "Použité zdroje dat a stav jejich posledního stažení", cache: "5 min" },
  { path: "/catalog", description: "Lokální katalog otevřených dat DCAT-AP-CZ (JSON-LD) pro Národní katalog otevřených dat", cache: "—" },
  { path: "/catalog/datasets/{id}", description: "Záznam datové sady v katalogu (budovy-a-energeticky-audit, predaudit-budov-a-navratnost-iqrf)", cache: "—" },
];

// Lokální katalog DCAT-AP-CZ — adresu stačí zaregistrovat v NKOD (data.gov.cz), ten si metadata sklízí sám
const CATALOG_PATH = `${BASE_PATH}/catalog`;

const STATUSES: AuditStatus[] = ["Nominal", "Warning", "Critical"];

const METRICS = [
  { value: "Temperature", label: "teplota, °C" },
  { value: "Co2", label: "CO₂, ppm" },
  { value: "Power", label: "příkon, kWh/h (po dnech kWh/den)" },
];

export default async function OpenDataPage() {
  const [origin, facilities] = await Promise.all([publicBaseUrl(), getFacilities()]);
  const example = facilities.find((f) => f.status !== "Nominal") ?? facilities[0];
  const base = `${origin}${BASE_PATH}`;

  const curlExample = [
    `curl "${base}/facilities?district=CZ0521&status=Critical"`,
    example && `curl "${base}/facilities/${example.id}/footprint"`,
    example && `curl "${base}/facilities/${example.id}/telemetry?metric=Co2&bucket=Day"`,
  ]
    .filter(Boolean)
    .join("\n");

  const jsExample = `const response = await fetch("${base}/facilities.geojson");
const objekty = await response.json();

console.log(objekty.features.length, "objektů");`;

  return (
    <>
      <TopBar title="Otevřené API">
        <a
          href={DOCS_PATH}
          target="_blank"
          rel="noreferrer"
          className="inline-flex h-9 items-center gap-2 rounded-lg bg-hk-red px-3.5 text-[13px] font-bold text-white transition-colors hover:bg-hk-red-dark"
        >
          <BookOpen className="size-4" aria-hidden />
          Interaktivní dokumentace
        </a>
      </TopBar>

      <main className="min-h-0 flex-1 overflow-y-auto p-6">
        <h1 className="text-[32px] leading-tight font-bold tracking-tight">Otevřená data SmartAuditu</h1>
        <p className="mt-1.5 max-w-2xl text-sm leading-normal text-slate-600">
          Objekty, které zřizuje Královéhradecký kraj, jejich budovy a stav energetického auditu. API je jen pro čtení, funguje bez
          přihlášení a bez klíče a dá se volat i z jiného webu (CORS). Ztráty ani úspory v Kč neobsahuje (jen ceníkovou cenu sady čidel IQRF). Data můžete volně použít pod licencí{" "}
          <a href="https://creativecommons.org/licenses/by/4.0/deed.cs" target="_blank" rel="noreferrer" className="font-bold text-hk-navy underline hover:text-hk-red">
            CC BY 4.0
          </a>{" "}
          s uvedením zdroje „SmartAudit“ a původních zdrojů ze stránky Zdroje dat.
        </p>

        <section aria-labelledby="publisher" className="mt-5 rounded-2xl border border-hk-blue/20 bg-[#EFF4FA] p-4.5">
          <div className="flex items-start gap-3">
            <BadgeCheck className="mt-0.5 size-5 shrink-0 text-hk-blue" aria-hidden />
            <div className="min-w-0">
              <h2 id="publisher" className="text-[15px] font-bold text-hk-navy">
                SmartAudit KHK sám publikuje nová otevřená data (Čl. 5.2b pravidel)
              </h2>
              <p className="mt-1 text-[13px] leading-normal text-slate-700">
                Spojená a vypočtená data — budovy kraje s kartou ČÚZK, stav auditu, pre-audit z 30+ sad otevřených dat a kalkulace sady čidel MICRORISC IQRF —
                vydáváme pod licencí <b>CC BY 4.0</b> a popisujeme je lokálním katalogem podle OFN <b>DCAT-AP-CZ</b>, který si Národní katalog otevřených dat
                (data.gov.cz) umí sám sklízet.
              </p>
              <div className="mt-2.5 flex flex-wrap items-center gap-1.5">
                {PUBLISHED_FORMATS.map((format) => (
                  <span key={format} className="rounded-md border border-hk-blue/20 bg-white px-1.5 py-0.5 font-mono text-[11px] font-bold text-hk-blue">
                    {format}
                  </span>
                ))}
                <a href={CATALOG_PATH} target="_blank" rel="noreferrer" className="ml-1 inline-flex items-center gap-1 font-mono text-[11.5px] font-bold text-hk-navy hover:text-hk-red hover:underline">
                  {CATALOG_PATH}
                  <ExternalLink className="size-3" aria-hidden />
                </a>
              </div>
            </div>
          </div>
        </section>

        <section aria-labelledby="downloads" className="mt-6">
          <h2 id="downloads" className="mb-2 text-[11px] font-bold tracking-wide text-slate-500 uppercase">
            Stáhnout celý seznam
          </h2>
          <div className="grid gap-3 sm:grid-cols-3">
            {DOWNLOADS.map(({ href, label, note, icon: Icon }) => (
              <a
                key={href}
                href={href}
                target="_blank"
                rel="noreferrer"
                className="group flex items-center gap-3 rounded-2xl border border-slate-200 bg-white px-4 py-3.5 transition-colors hover:border-hk-red/40"
              >
                <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-hk-navy group-hover:bg-hk-red/10 group-hover:text-hk-red">
                  <Icon className="size-4.5" strokeWidth={1.75} aria-hidden />
                </span>
                <span className="min-w-0">
                  <span className="block text-[13.5px] font-bold">{label}</span>
                  <span className="block text-xs text-slate-500">
                    {facilities.length} objektů · {note}
                  </span>
                  <span className="mt-0.5 block font-mono text-[10.5px] text-slate-400">CC BY 4.0 · stáhnout jedním klikem</span>
                </span>
              </a>
            ))}
          </div>

          <a
            href={CATALOG_PATH}
            target="_blank"
            rel="noreferrer"
            className="group mt-3 flex items-center gap-3 rounded-2xl border border-slate-200 bg-white px-4 py-3.5 transition-colors hover:border-hk-red/40"
          >
            <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-hk-navy group-hover:bg-hk-red/10 group-hover:text-hk-red">
              <Library className="size-4.5" strokeWidth={1.75} aria-hidden />
            </span>
            <span className="min-w-0 flex-1">
              <span className="block text-[13.5px] font-bold">Katalog pro NKOD (DCAT-AP-CZ)</span>
              <span className="block text-xs text-slate-500">
                Metadata sady a distribucí v JSON-LD podle otevřené formální normy — adresu stačí zaregistrovat v Národním katalogu otevřených dat
              </span>
            </span>
            <code className="hidden shrink-0 font-mono text-[11.5px] text-slate-500 sm:block">
              {origin}
              {CATALOG_PATH}
            </code>
          </a>
        </section>

        <section aria-labelledby="endpoints" className="mt-8">
          <div className="mb-2 flex flex-wrap items-baseline justify-between gap-2">
            <h2 id="endpoints" className="text-[11px] font-bold tracking-wide text-slate-500 uppercase">
              Endpointy
            </h2>
            <p className="font-mono text-[11px] text-slate-500">
              základ {base} ·{" "}
              <a href={OPENAPI_PATH} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 hover:text-hk-red hover:underline">
                OpenAPI 3.1
                <ExternalLink className="size-3" aria-hidden />
              </a>
            </p>
          </div>
          <ul className="divide-y divide-slate-100 overflow-hidden rounded-2xl border border-slate-200 bg-white">
            {ENDPOINTS.map((endpoint) => (
              <li key={endpoint.path} className="flex flex-col gap-1 px-4.5 py-3 sm:flex-row sm:items-center sm:gap-4">
                <span className="w-fit shrink-0 rounded bg-emerald-50 px-1.5 py-0.5 font-mono text-[11px] font-bold text-emerald-800">GET</span>
                <code className="shrink-0 font-mono text-[13px] font-bold sm:w-64">{endpoint.path}</code>
                <span className="flex-1 text-[13px] text-slate-600">{endpoint.description}</span>
                <span className="shrink-0 font-mono text-[11px] text-slate-500">cache {endpoint.cache}</span>
              </li>
            ))}
          </ul>
          <p className="mt-2 max-w-2xl text-xs leading-normal text-slate-500">
            Půdorys budovy se mění jen s katastrem, proto ho API i mapa aplikace drží v mezipaměti 2 dny (hlavička{" "}
            <code className="font-mono">Cache-Control: max-age=172800</code>) a opakovaný dotaz na server neodchází. Ostatní data se obnovují po 5
            minutách.
          </p>
        </section>

        <section aria-labelledby="parameters" className="mt-8">
          <h2 id="parameters" className="mb-2 text-[11px] font-bold tracking-wide text-slate-500 uppercase">
            Filtry a hodnoty
          </h2>
          <dl className="grid gap-3 rounded-2xl border border-slate-200 bg-white p-4.5 text-[13px] sm:grid-cols-2">
            <ParameterValues name="category" values={(Object.keys(CATEGORY_META) as FacilityCategory[]).map((key) => ({ value: key, label: CATEGORY_META[key].label }))} />
            <ParameterValues name="district" values={DISTRICTS.map((d) => ({ value: d.code, label: d.name }))} />
            <ParameterValues name="status" values={STATUSES.map((key) => ({ value: key, label: STATUS_META[key].label }))} />
            <ParameterValues name="metric" values={METRICS} />
            <div>
              <dt className="font-mono text-xs font-bold">q</dt>
              <dd className="mt-1 text-slate-600">hledání v názvu, kódu a obci</dd>
            </div>
            <div>
              <dt className="font-mono text-xs font-bold">from, to, bucket</dt>
              <dd className="mt-1 text-slate-600">období v ISO 8601 (výchozí posledních 48 h) a agregace Raw, Hour nebo Day</dd>
            </div>
          </dl>
        </section>

        <section aria-labelledby="examples" className="mt-8 grid gap-4 lg:grid-cols-2">
          <h2 id="examples" className="sr-only">
            Ukázky volání
          </h2>
          <CodeSample title="Příkazová řádka" code={curlExample} />
          <CodeSample title="JavaScript" code={jsExample} />
        </section>
      </main>
    </>
  );
}

function ParameterValues({ name, values }: { name: string; values: { value: string; label: string }[] }) {
  return (
    <div>
      <dt className="font-mono text-xs font-bold">{name}</dt>
      <dd className="mt-1 flex flex-wrap gap-1.5">
        {values.map((item) => (
          <span key={item.value} className="rounded-md border border-slate-200 bg-slate-50 px-1.5 py-0.5 text-xs">
            <code className="font-mono">{item.value}</code>
            <span className="text-slate-500"> · {item.label}</span>
          </span>
        ))}
      </dd>
    </div>
  );
}

function CodeSample({ title, code }: { title: string; code: string }) {
  return (
    <figure className="overflow-hidden rounded-2xl bg-hk-navy">
      <figcaption className="flex items-center justify-between border-b border-white/10 px-4 py-2 text-xs font-bold text-white/80">
        {title}
        <CopyButton text={code} />
      </figcaption>
      <pre className="overflow-x-auto p-4 font-mono text-[12px] leading-relaxed text-white/90">
        <code>{code}</code>
      </pre>
    </figure>
  );
}
