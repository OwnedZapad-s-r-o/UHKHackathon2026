"use client";

import { useState, type ReactNode } from "react";
import { AlertTriangle, CheckCircle2, ChevronDown, FlaskConical, Search } from "lucide-react";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import type { GatewayGroup } from "./gateway-row";

// Řádek gatewaye vykreslený na serveru (čas posledního kontaktu) + údaje pro řazení a hledání
export type GatewayListEntry = {
  id: string;
  group: GatewayGroup;
  needsAttention: boolean;
  searchText: string;
  row: ReactNode;
};

// Hledání se ukáže až u delšího seznamu — správce s pár budovami ho nepotřebuje
const SEARCH_FROM = 10;

// Skupinu „V pořádku“ do tohoto počtu necháme rozbalenou — krátký seznam vypadá jako dřív
const OPEN_OK_UP_TO = 5;

// Připojené gatewaye: nahoře ty, které potřebují zásah, pak skutečné v pořádku (sbalené) a nakonec virtuální a demo.
// Sbalené řádky zůstávají v DOM (jen skryté) — průvodce z nich bere odkaz na nepřiřazenou gatewayi.
export function GatewayList({ entries }: { entries: GatewayListEntry[] }) {
  const [query, setQuery] = useState("");

  const attention = entries.filter((e) => e.group === "attention");
  const ok = entries.filter((e) => e.group === "ok");
  // I mezi virtuálními napřed ty, které čekají na přiřazení
  const virtual = entries.filter((e) => e.group === "virtual").toSorted((a, b) => Number(b.needsAttention) - Number(a.needsAttention));
  const virtualPending = virtual.filter((e) => e.needsAttention).length;

  const normalized = normalize(query);
  const matches = normalized ? entries.filter((e) => normalize(e.searchText).includes(normalized)) : null;

  return (
    <>
      {entries.length > SEARCH_FROM && (
        <div className="relative border-b border-slate-100 px-5 py-3">
          <Search className="pointer-events-none absolute top-1/2 left-8 size-4 -translate-y-1/2 text-slate-400" aria-hidden />
          <Input
            type="search"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Hledat gateway, budovu nebo kód objektu…"
            aria-label="Hledat v připojených gatewayích"
            className="h-9 border-slate-200 bg-white pl-9 text-[13px]"
          />
        </div>
      )}

      {matches ? (
        matches.length === 0 ? (
          <p className="px-5 py-5 text-[13px] text-slate-600">Hledání neodpovídá žádná gateway.</p>
        ) : (
          <EntryList entries={matches} />
        )
      ) : (
        <>
          {attention.length > 0 && (
            <div className="border-b border-slate-100">
              <p className="flex items-center gap-2 bg-amber-50 px-5 py-2 text-[12px] font-bold text-amber-900">
                <AlertTriangle className="size-3.5" aria-hidden />
                Vyžaduje pozornost ({attention.length})
              </p>
              <EntryList entries={attention} />
            </div>
          )}

          {ok.length > 0 && (
            <Group
              icon={<CheckCircle2 className="size-4 text-emerald-600" aria-hidden />}
              title={attention.length === 0 ? `Všech ${ok.length} gatewayí měří v místnostech` : `V pořádku (${ok.length})`}
              defaultOpen={ok.length <= OPEN_OK_UP_TO}
              entries={ok}
            />
          )}

          {attention.length === 0 && ok.length === 0 && (
            <p className="border-b border-slate-100 px-5 py-4 text-[13px] text-slate-600">Skutečnou gateway zatím nikdo nepřipojil. Virtuální a demo zařízení jsou níže.</p>
          )}

          {virtual.length > 0 && (
            <Group
              icon={<FlaskConical className="size-4 text-hk-blue" aria-hidden />}
              title={`Virtuální a demo (${virtual.length})`}
              note={virtualPending > 0 ? `${virtualPending} čeká na přiřazení` : undefined}
              defaultOpen={false}
              entries={virtual}
            />
          )}
        </>
      )}
    </>
  );
}

type GroupProps = {
  icon: ReactNode;
  title: string;
  note?: string;
  defaultOpen: boolean;
  entries: GatewayListEntry[];
};

function Group({ icon, title, note, defaultOpen, entries }: GroupProps) {
  const [open, setOpen] = useState(defaultOpen);

  return (
    <div className="border-b border-slate-100 last:border-b-0">
      <button
        type="button"
        onClick={() => setOpen(!open)}
        aria-expanded={open}
        className="flex w-full items-center gap-2 px-5 py-3 text-left text-[13px] font-bold transition-colors hover:bg-slate-50"
      >
        {icon}
        <span className="flex-1">{title}</span>
        {note && <span className="rounded-full bg-amber-50 px-2 py-0.5 text-[11.5px] text-amber-800">{note}</span>}
        <span className="text-xs font-normal text-slate-500">{open ? "sbalit" : "rozbalit"}</span>
        <ChevronDown className={cn("size-4 text-slate-400 transition-transform", open && "rotate-180")} aria-hidden />
      </button>
      <div hidden={!open}>
        <EntryList entries={entries} />
      </div>
    </div>
  );
}

function EntryList({ entries }: { entries: GatewayListEntry[] }) {
  return (
    <ul className="divide-y divide-slate-100 border-t border-slate-100">
      {entries.map((entry) => (
        <li key={entry.id}>{entry.row}</li>
      ))}
    </ul>
  );
}

// Hledání bez ohledu na velikost písmen a diakritiku („tyl“ najde „Tyla“, „jicin“ Jičín)
function normalize(text: string) {
  return text
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .toLowerCase()
    .trim();
}
