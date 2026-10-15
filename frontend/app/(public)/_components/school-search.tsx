"use client";

import { useEffect, useId, useMemo, useState, type KeyboardEvent } from "react";
import { useRouter } from "next/navigation";
import { CornerDownLeft, Search } from "lucide-react";
import { AIR_META, airLevel } from "@/lib/air-quality";
import { CATEGORY_META } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { SearchItem } from "./search-items";

type SchoolSearchProps = {
  items: SearchItem[];
  defaultValue?: string;
  placeholder: string;
  // Vzhled pole se liší podle stránky (landing: žlutá sekce, /objekty: běžné pole)
  fieldClassName: string;
  iconClassName?: string;
  inputClassName?: string;
};

// Do seznamu se vejde ~5 návrhů, další jsou dostupné posunem
const MAX_SUGGESTIONS = 30;

// Pole hledání s našeptávačem. Je uvnitř obyčejného formuláře (name="q"):
// Enter bez vybraného návrhu formulář odešle na /objekty, vybraný návrh otevře rovnou kartu objektu.
export function SchoolSearch({ items, defaultValue = "", placeholder, fieldClassName, iconClassName, inputClassName }: SchoolSearchProps) {
  const router = useRouter();
  const listId = useId();
  const [query, setQuery] = useState(defaultValue);
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(-1);

  const suggestions = useMemo(() => suggest(items, query), [items, query]);
  const expanded = open && query.trim().length > 0;

  // Šipkami vybraný návrh musí být vidět i v posunutém seznamu
  useEffect(() => {
    if (active >= 0) document.getElementById(`${listId}-${active}`)?.scrollIntoView({ block: "nearest" });
  }, [active, listId]);

  function choose(item: SearchItem) {
    setOpen(false);
    router.push(`/objekty/${item.code}`);
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
      event.preventDefault();
      setOpen(true);
      if (suggestions.length === 0) return;
      // Šipky obíhají dokola přes návrhy i výchozí stav „nic nevybráno“ (-1 = zůstane napsaný text)
      const step = event.key === "ArrowDown" ? 1 : -1;
      const slots = suggestions.length + 1;
      setActive((current) => ((current + 1 + step + slots) % slots) - 1);
      return;
    }

    if (event.key === "Escape") {
      setOpen(false);
      setActive(-1);
      return;
    }

    if (event.key === "Enter" && expanded && active >= 0 && active < suggestions.length) {
      event.preventDefault();
      choose(suggestions[active]);
    }
  }

  return (
    <div className="relative min-w-0 flex-1">
      <label className={fieldClassName}>
        <Search className={cn("size-4.5 shrink-0", iconClassName)} aria-hidden />
        <input
          type="search"
          name="q"
          value={query}
          onChange={(event) => {
            setQuery(event.target.value);
            setOpen(true);
            setActive(-1);
          }}
          onFocus={() => setOpen(true)}
          onBlur={() => setOpen(false)}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          autoComplete="off"
          role="combobox"
          aria-label={placeholder}
          aria-expanded={expanded}
          aria-controls={listId}
          aria-autocomplete="list"
          aria-activedescendant={expanded && active >= 0 && active < suggestions.length ? `${listId}-${active}` : undefined}
          className={cn("min-w-0 flex-1 bg-transparent text-hk-navy outline-none placeholder:text-slate-400", inputClassName)}
        />
      </label>

      {expanded && (
        <div className="absolute top-full left-0 z-40 mt-2 w-[min(36rem,calc(100vw-2rem))] min-w-full overflow-hidden rounded-xl border border-slate-200 bg-white text-left shadow-[0_20px_44px_-12px_rgba(11,25,44,0.3)]">
          {suggestions.length === 0 ? (
            <p className="px-4 py-3.5 text-[14px] text-slate-500">Žádná škola ani domov neodpovídá. Zkuste obec nebo část názvu.</p>
          ) : (
            <ul id={listId} role="listbox" aria-label="Návrhy objektů" className="max-h-80 overflow-y-auto overscroll-contain py-1.5">
              {suggestions.map((item, index) => (
                <Suggestion
                  key={item.code}
                  id={`${listId}-${index}`}
                  item={item}
                  query={query}
                  active={index === active}
                  onHover={() => setActive(index)}
                  onChoose={() => choose(item)}
                />
              ))}
            </ul>
          )}
          {/* Mousedown místo click: klik jinak nejdřív zavře seznam přes blur pole */}
          <button
            type="submit"
            onMouseDown={(event) => event.preventDefault()}
            className="flex w-full items-center justify-between gap-3 border-t border-slate-100 bg-slate-50 px-4 py-2.5 text-[13px] text-slate-600 transition-colors hover:bg-slate-100 hover:text-hk-navy"
          >
            <span>
              Všechny výsledky pro „<span className="font-bold text-hk-navy">{query.trim()}</span>“
            </span>
            <CornerDownLeft className="size-3.5 shrink-0 text-slate-400" aria-hidden />
          </button>
        </div>
      )}
    </div>
  );
}

type SuggestionProps = {
  id: string;
  item: SearchItem;
  query: string;
  active: boolean;
  onHover: () => void;
  onChoose: () => void;
};

function Suggestion({ id, item, query, active, onHover, onChoose }: SuggestionProps) {
  const Icon = CATEGORY_META[item.category].icon;
  const level = airLevel(item.co2);

  return (
    <li
      id={id}
      role="option"
      aria-selected={active}
      onMouseDown={(event) => event.preventDefault()}
      onMouseEnter={onHover}
      onClick={onChoose}
      className={cn("mx-1.5 flex cursor-pointer items-center gap-3 rounded-lg px-2.5 py-2", active && "bg-slate-100")}
    >
      <span className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-hk-navy text-white">
        <Icon className="size-4" aria-hidden />
      </span>
      <span className="min-w-0 flex-1">
        <span className="block truncate text-[14px] font-bold text-hk-navy">
          <Highlight text={item.name} query={query} />
        </span>
        <span className="block truncate text-[12px] text-slate-500">
          <Highlight text={item.municipalityName} query={query} /> · okres {item.districtName}
        </span>
      </span>
      {level && (
        <span className={cn("hidden shrink-0 items-center gap-1.5 text-[11.5px] font-bold sm:flex", AIR_META[level].text)}>
          <span className={cn("size-2 rounded-full", AIR_META[level].dot)} aria-hidden />
          {AIR_META[level].label}
        </span>
      )}
    </li>
  );
}

// Zvýrazní nalezenou část textu — hledá se bez diakritiky, zvýrazní se původní znaky na stejných pozicích
function Highlight({ text, query }: { text: string; query: string }) {
  const needle = normalize(query.trim());
  const start = needle ? normalize(text).indexOf(needle) : -1;
  if (start < 0) return text;

  return (
    <>
      {text.slice(0, start)}
      <mark className="rounded-sm bg-amber-100 px-px text-inherit">{text.slice(start, start + needle.length)}</mark>
      {text.slice(start + needle.length)}
    </>
  );
}

// Odstranění diakritiky zachová délku řetězce (jeden znak → jeden znak), pozice pro zvýraznění proto sedí
function normalize(value: string) {
  return value.normalize("NFD").replace(/\p{Diacritic}/gu, "").toLocaleLowerCase("cs");
}

// Pořadí: název začíná hledaným textem → některé slovo názvu nebo obce začíná → text je kdekoli v názvu, obci nebo kódu
function suggest(items: SearchItem[], query: string) {
  const needle = normalize(query.trim());
  if (!needle) return [];

  return items
    .map((item) => {
      const name = normalize(item.name);
      const place = normalize(item.municipalityName);
      const words = `${name} ${place}`.split(/[\s,.\-–]+/);

      let rank = -1;
      if (name.startsWith(needle) || place.startsWith(needle)) rank = 0;
      else if (words.some((word) => word.startsWith(needle))) rank = 1;
      else if (`${name} ${place} ${normalize(item.code)}`.includes(needle)) rank = 2;

      return { item, rank };
    })
    .filter((entry) => entry.rank >= 0)
    .sort((a, b) => a.rank - b.rank || a.item.name.localeCompare(b.item.name, "cs"))
    .slice(0, MAX_SUGGESTIONS)
    .map((entry) => entry.item);
}
