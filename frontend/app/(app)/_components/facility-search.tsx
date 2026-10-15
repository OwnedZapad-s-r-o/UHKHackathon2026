"use client";

import { useId, useState } from "react";
import { Search } from "lucide-react";
import { InputGroup, InputGroupAddon, InputGroupInput } from "@/components/ui/input-group";
import { STATUS_META } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { AuditStatus } from "@/types/api";

export type FacilitySuggestion = {
  id: string;
  code: string;
  name: string;
  municipalityName: string;
  districtName: string;
  status: AuditStatus;
};

const MAX_SUGGESTIONS = 8;

type FacilitySearchProps = {
  facilities: FacilitySuggestion[];
  value: string;
  onValueChange: (value: string) => void;
  onSelect: (facility: FacilitySuggestion) => void;
  placeholder: string;
};

// Hledání objektu s našeptávačem: šipkami a Enterem (nebo kliknutím) se objekt rovnou otevře v mapě.
// Enter bez vybraného návrhu jen filtruje mapu podle textu jako dřív.
export function FacilitySearch({ facilities, value, onValueChange, onSelect, placeholder }: FacilitySearchProps) {
  const listId = useId();
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(-1);

  const suggestions = suggest(facilities, value);
  const expanded = open && value.trim().length > 0;

  function choose(facility: FacilitySuggestion) {
    setOpen(false);
    setActive(-1);
    onSelect(facility);
  }

  function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
      event.preventDefault();
      setOpen(true);
      if (suggestions.length === 0) return;
      const step = event.key === "ArrowDown" ? 1 : -1;
      setActive((current) => (current + step + suggestions.length) % suggestions.length);
    } else if (event.key === "Enter" && expanded && suggestions[active]) {
      event.preventDefault();
      choose(suggestions[active]);
    } else if (event.key === "Escape" && expanded) {
      // Zavře jen návrhy — globální Escape (zavřít objekt v mapě) přijde na řadu až dalším stiskem
      event.stopPropagation();
      setOpen(false);
      setActive(-1);
    }
  }

  return (
    <div className="relative ml-auto w-52 min-w-0 shrink">
      <InputGroup className="h-9 border-slate-300 bg-white">
        <InputGroupAddon>
          <Search className="text-slate-400" aria-hidden />
        </InputGroupAddon>
        <InputGroupInput
          value={value}
          onChange={(event) => {
            onValueChange(event.target.value);
            setOpen(true);
            setActive(-1);
          }}
          onFocus={() => setOpen(true)}
          onBlur={() => setOpen(false)}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          aria-label="Hledat objekt"
          role="combobox"
          aria-expanded={expanded}
          aria-controls={listId}
          aria-autocomplete="list"
          aria-activedescendant={expanded && active >= 0 ? `${listId}-${active}` : undefined}
          autoComplete="off"
          className="text-[13px]"
        />
      </InputGroup>

      {expanded && (
        <ul
          id={listId}
          role="listbox"
          aria-label="Návrhy objektů"
          className="absolute top-full right-0 z-30 mt-1 w-96 max-w-[calc(100vw-2rem)] overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-[0_12px_32px_rgba(11,25,44,0.14)]"
        >
          {suggestions.length === 0 ? (
            <li className="px-3.5 py-2.5 text-[13px] text-slate-500">Žádný objekt neodpovídá. Zkuste kód objektu nebo obec.</li>
          ) : (
            suggestions.map((facility, index) => (
              <li
                key={facility.id}
                id={`${listId}-${index}`}
                role="option"
                aria-selected={index === active}
                // Klik nesmí vzít fokus z pole dřív, než se výběr provede
                onMouseDown={(event) => event.preventDefault()}
                onClick={() => choose(facility)}
                onMouseEnter={() => setActive(index)}
                className={cn("flex cursor-pointer items-start gap-2.5 px-3.5 py-2", index === active && "bg-slate-100")}
              >
                <span className={cn("mt-1.5 size-2 shrink-0 rounded-full", STATUS_META[facility.status].dot)} aria-hidden />
                <span className="min-w-0 flex-1">
                  <span className="block truncate text-[13px] font-bold text-hk-navy">{facility.name}</span>
                  <span className="block truncate text-[11.5px] text-slate-500">
                    {facility.code} · {facility.municipalityName} · okres {facility.districtName}
                  </span>
                </span>
              </li>
            ))
          )}
        </ul>
      )}
    </div>
  );
}

// Objekt musí obsahovat všechna zadaná slova („tyl hradec“). Pořadí: shoda kódu, začátek slova v názvu,
// kdekoli v názvu, zbytek (obec, víc slov)
function suggest(facilities: FacilitySuggestion[], query: string) {
  const needle = normalize(query);
  if (!needle) return [];
  const words = needle.split(/\s+/);

  return facilities
    .map((facility) => {
      const name = normalize(facility.name);
      const haystack = `${normalize(facility.code)} ${name} ${normalize(facility.municipalityName)}`;
      if (!words.every((word) => haystack.includes(word))) return { facility, rank: null };

      const rank = normalize(facility.code).startsWith(needle)
        ? 0
        : name.split(/[\s,.—-]+/).some((word) => word.startsWith(needle))
          ? 1
          : name.includes(needle)
            ? 2
            : 3;
      return { facility, rank };
    })
    .filter((item): item is { facility: FacilitySuggestion; rank: number } => item.rank !== null)
    .toSorted((a, b) => a.rank - b.rank || a.facility.name.localeCompare(b.facility.name, "cs"))
    .slice(0, MAX_SUGGESTIONS)
    .map((item) => item.facility);
}

// Bez ohledu na velikost písmen a diakritiku („jicin“ najde Jičín)
function normalize(text: string) {
  return text
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .toLowerCase()
    .trim();
}
