import { ExternalLink, FileText, Flame, Hourglass } from "lucide-react";
import { getContracts } from "@/lib/api";
import { formatCzk, formatNumber } from "@/lib/facility-meta";
import type { ContractCategory, FacilityContract, FacilityContracts, FacilityDetail } from "@/types/api";
import { PanelCard } from "./panel-card";

// Datum bez času z API ("2025-08-06") — UTC, aby se den neposunul
const date = new Intl.DateTimeFormat("cs-CZ", { dateStyle: "medium", timeZone: "UTC" });
const dateTime = new Intl.DateTimeFormat("cs-CZ", { dateStyle: "short", timeStyle: "short" });

const SECTIONS: { category: ContractCategory; title: string; note: string }[] = [
  { category: "EnergySupply", title: "Dodávky energií", note: "Cena ze smlouvy nahrazuje v auditu paušální odhad." },
  { category: "FacilityManagement", title: "Servis kotelen, vytápění a MaR", note: "Seřízení regulace může být součástí servisu — bez nové zakázky." },
  { category: "Reconstruction", title: "Stavební úpravy a úspory energie", note: "Zateplení, okna, zdroj tepla — co už kraj do budovy investoval." },
];

export async function ContractsTab({ facility }: { facility: FacilityDetail }) {
  const data = await getContracts(facility.id);

  return (
    <>
      <HeatPriceCard data={data} />

      <PanelCard title="Smlouvy z Registru smluv" meta={data.syncedAt ? `aktualizováno ${dateTime.format(new Date(data.syncedAt))}` : undefined}>
        <p className="text-xs leading-relaxed text-slate-600">
          {data.organizationName ?? "Organizace"}
          {data.ico && <> · IČO {data.ico}</>} — smlouvy o energiích a provozu budovy, které organizace zveřejnila podle zákona č. 340/2015 Sb.
        </p>
        <ContractList facilityId={facility.id} data={data} />
      </PanelCard>
    </>
  );
}

function HeatPriceCard({ data }: { data: FacilityContracts }) {
  const { heatPrice } = data;
  const contract = data.contracts.find((c) => c.id === heatPrice.contractId);
  const loss = heatPrice.annualLossCzk ? ` — ztráta z přetápění mimo provoz ${formatCzk(heatPrice.annualLossCzk)} ročně` : "";

  return (
    <section className="mt-3.5 flex gap-3 rounded-2xl border border-slate-200 bg-white p-4.5">
      <Flame className="mt-0.5 size-4.5 shrink-0 text-hk-red" aria-hidden />
      {contract ? (
        <p className="text-[13px] leading-relaxed">
          Budova odebírá teplo za <b>{formatNumber(heatPrice.czkPerKwh * 1000)} Kč/MWh</b> ({contract.partnerName}
          {contract.contractNumber && <>, smlouva č. {contract.contractNumber}</>}){loss}. Audit počítá s touto cenou místo paušálu{" "}
          {formatNumber(heatPrice.defaultCzkPerKwh)} Kč/kWh.
        </p>
      ) : (
        <p className="text-[13px] leading-relaxed text-slate-600">
          Ve smlouvách organizace není cena tepla, audit počítá s paušálem <b>{formatNumber(heatPrice.defaultCzkPerKwh)} Kč/kWh</b>
          {loss}. Elektřinu a plyn kraj pro své organizace nakupuje centrálně na komoditní burze, proto je školy ve svých smlouvách často
          nemají.
        </p>
      )}
    </section>
  );
}

function ContractList({ facilityId, data }: { facilityId: string; data: FacilityContracts }) {
  if (!data.ico) {
    return <p className="mt-3 text-[13px] text-slate-600">Objekt nemá v otevřených datech přiřazenou organizaci s IČO.</p>;
  }

  if (!data.syncedAt) {
    return (
      <p className="mt-3 flex items-center gap-2 text-[13px] text-slate-600">
        <Hourglass className="size-4 text-slate-400" aria-hidden />
        Smlouvy se právě stahují z Registru smluv — zkuste to za pár minut.
      </p>
    );
  }

  if (data.contracts.length === 0) {
    return <p className="mt-3 text-[13px] text-slate-600">V Registru smluv jsme u organizace nenašli smlouvy o energiích ani provozu budovy.</p>;
  }

  return SECTIONS.map(({ category, title, note }) => {
    const contracts = data.contracts.filter((c) => c.category === category);
    if (contracts.length === 0) return null;

    return (
      <div key={category} className="mt-4">
        <h3 className="text-[13px] font-bold">
          {title} <span className="font-mono text-[10.5px] font-normal text-slate-500">{contracts.length}</span>
        </h3>
        <p className="text-[11px] text-slate-500">{note}</p>
        <ul>
          {contracts.map((contract) => (
            <ContractItem key={contract.id} facilityId={facilityId} contract={contract} />
          ))}
        </ul>
      </div>
    );
  });
}

function ContractItem({ facilityId, contract }: { facilityId: string; contract: FacilityContract }) {
  const amount = contract.amountWithoutVat !== null ? `${formatCzk(contract.amountWithoutVat)} bez DPH` : contract.amountWithVat !== null ? `${formatCzk(contract.amountWithVat)} vč. DPH` : "hodnota neuvedena";

  return (
    <li className="border-t border-slate-100 py-3 first:mt-2">
      <p className="text-[13px] leading-snug font-bold">{contract.title}</p>
      <p className="mt-1 text-xs text-slate-600">
        {contract.partnerName}
        {contract.partnerIco && <span className="text-slate-400"> · IČO {contract.partnerIco}</span>}
      </p>
      <p className="mt-0.5 font-mono text-[11px] text-slate-500">
        {amount} · uzavřeno {contract.dateSigned ? date.format(new Date(contract.dateSigned)) : "neuvedeno"}
        {contract.contractNumber && <> · č. {contract.contractNumber}</>}
      </p>
      {contract.unitPriceCzkPerMwh !== null && (
        <p className="mt-1 text-xs text-slate-700">
          Cena energie <b>{formatNumber(contract.unitPriceCzkPerMwh)} Kč/MWh</b>
          {contract.priceSource && <span className="text-slate-500"> — {contract.priceSource}</span>}
        </p>
      )}
      <div className="mt-2 flex flex-wrap gap-x-3 gap-y-1 text-xs">
        {contract.attachments.map((attachment) => (
          <a
            key={attachment.id}
            href={`/smlouvy/${facilityId}/${contract.id}/${attachment.id}`}
            target="_blank"
            rel="noreferrer"
            className="inline-flex max-w-full items-center gap-1 font-bold text-hk-red hover:underline"
          >
            <FileText className="size-3.5 shrink-0" aria-hidden />
            <span className="truncate">{attachment.name}</span>
          </a>
        ))}
        <a href={contract.registryUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-slate-500 hover:text-hk-navy hover:underline">
          Záznam v registru
          <ExternalLink className="size-3" aria-hidden />
        </a>
      </div>
    </li>
  );
}
