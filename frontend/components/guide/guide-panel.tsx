"use client";

import { useCallback, useEffect, useRef, useState, type KeyboardEvent, type ReactNode } from "react";
import { ArrowUp, Compass, Database, MapPin, RotateCcw, ShieldCheck, Sparkles, X, type LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import type { GuideAnswer as GuideAnswerData, GuideContext, GuideFacilityCard, GuideMode } from "@/types/api";
import { GuideAnswer } from "./guide-answer";

export type GuideStarter = { icon: LucideIcon; label: string; question: string };

type GuideStep = { id: number; question: string } & ({ state: "loading" } | { state: "done"; answer: GuideAnswerData } | { state: "error"; message: string });

type GuidePanelProps = {
  context: GuideContext;
  // Název objektu, ke kterému se uživatel právě ptá („tady“, „tahle budova“)
  contextLabel?: string | null;
  starters: GuideStarter[];
  // Ve kterém úložišti držet rozpracovanou prohlídku (aplikace a veřejný web zvlášť)
  storageKey: string;
  // map = panel uvnitř mapy, floating = plovoucí v rohu stránky bez mapy
  variant: "map" | "floating";
  // Pozice panelu v mapě — v aplikaci je vlevo nahoře drobečková navigace okresů
  panelClassName?: string;
  onAnswer: (answer: GuideAnswerData) => void;
  onOpenFacility: (card: GuideFacilityCard) => void;
  onOpenDistrict: (code: string) => void;
  onReset?: () => void;
};

const MAX_QUESTION = 400;
const MAX_STEPS = 8;
const LOADING_STAGES = ["Hledám ve znalostech…", "Ptám se dat kraje…", "Připravuji mapu…"];

// Průvodce SmartAuditu: ne chat, ale stezka kroků — každá otázka je krok s odpovědí, kartami a pohybem mapy.
// Starší kroky se sbalí do jednoho řádku a kliknutím se k nim jde vrátit (mapa ukáže znovu jejich výsledek).
export function GuidePanel({ context, contextLabel, starters, storageKey, variant, panelClassName, onAnswer, onOpenFacility, onOpenDistrict, onReset }: GuidePanelProps) {
  const [open, setOpen] = useState(false);
  const [steps, setSteps] = useState<GuideStep[]>([]);
  const [expanded, setExpanded] = useState<number | null>(null);
  const [question, setQuestion] = useState("");
  const inputRef = useRef<HTMLTextAreaElement>(null);
  const scrollRef = useRef<HTMLDivElement>(null);
  const abortRef = useRef<AbortController | null>(null);
  const nextId = useRef(1);

  const loading = steps.at(-1)?.state === "loading";
  const lastMode = lastDone(steps)?.answer.mode;

  // Rozpracovaná prohlídka přežije obnovení stránky (jen v této kartě prohlížeče)
  useEffect(() => {
    const saved = readSteps(storageKey);
    if (saved.length === 0) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect -- obnova z úložiště prohlížeče po prvním vykreslení (na serveru úložiště není)
    setSteps(saved);
    nextId.current = Math.max(...saved.map((s) => s.id)) + 1;
  }, [storageKey]);

  useEffect(() => {
    writeSteps(storageKey, steps);
  }, [storageKey, steps]);

  // „/“ kdekoli mimo pole otevře průvodce a dá kurzor do dotazu
  useEffect(() => {
    function handleKey(event: globalThis.KeyboardEvent) {
      const target = event.target;
      if (event.key !== "/" || (target instanceof HTMLElement && target.closest("input, textarea, select, [contenteditable=true]"))) return;
      event.preventDefault();
      setOpen(true);
    }

    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, []);

  // Po otevření rovnou psát — pole existuje až po vykreslení panelu
  useEffect(() => {
    if (open) inputRef.current?.focus({ preventScroll: true });
  }, [open]);

  // Nový krok (otázka i hotová odpověď) se ukáže od svého začátku — text odpovědi nesmí utéct nad karty
  useEffect(() => {
    const container = scrollRef.current;
    const latest = container?.querySelector<HTMLElement>("article:last-of-type");
    if (container && latest) container.scrollTo({ top: latest.offsetTop - 12, behavior: "smooth" });
  }, [steps.length, loading]);

  const finish = useCallback((id: number, result: { state: "done"; answer: GuideAnswerData } | { state: "error"; message: string }) => {
    setSteps((current) => current.map((s) => (s.id === id ? { id: s.id, question: s.question, ...result } : s)));
  }, []);

  const ask = useCallback(
    async (text: string) => {
      const trimmed = text.trim().slice(0, MAX_QUESTION);
      if (!trimmed) return;

      abortRef.current?.abort();
      const controller = new AbortController();
      abortRef.current = controller;

      const id = nextId.current++;
      const history = steps.map((s) => s.question).slice(-4);
      setSteps((current) => [...current.filter((s) => s.state !== "loading"), { id, question: trimmed, state: "loading" as const }].slice(-MAX_STEPS));
      setExpanded(null);
      setQuestion("");
      setOpen(true);

      try {
        const response = await fetch("/pruvodce", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ question: trimmed, context, history }),
          signal: controller.signal,
        });
        const data = (await response.json().catch(() => null)) as (GuideAnswerData & { message?: string }) | null;

        if (!response.ok || !data || typeof data.answer !== "string") {
          finish(id, { state: "error", message: data?.message ?? "Průvodce teď neodpovídá, zkuste to prosím za chvíli." });
          return;
        }

        finish(id, { state: "done", answer: data });
        onAnswer(data);
      } catch (error) {
        if ((error as Error).name !== "AbortError") finish(id, { state: "error", message: "Spojení se nepodařilo. Zkontrolujte připojení a zkuste to znovu." });
      }
    },
    [context, finish, onAnswer, steps],
  );

  function reset() {
    abortRef.current?.abort();
    setSteps([]);
    setExpanded(null);
    onReset?.();
    inputRef.current?.focus({ preventScroll: true });
  }

  // Návrat ke staršímu kroku: rozbalit ho a ukázat jeho výsledek v mapě
  function revisit(step: GuideStep) {
    setExpanded((current) => (current === step.id ? null : step.id));
    if (step.state === "done" && expanded !== step.id) onAnswer(step.answer);
  }

  function handleKeyDown(event: KeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      void ask(question);
    } else if (event.key === "Escape") {
      // Esc v poli nezavírá objekt v mapě (globální zkratka mapy)
      event.stopPropagation();
      event.currentTarget.blur();
    }
  }

  if (!open) {
    return (
      <Launcher variant={variant} hasHistory={steps.length > 0} onOpen={() => setOpen(true)} />
    );
  }

  const latestDone = lastDone(steps);

  return (
    <section
      aria-label="Průvodce SmartAuditu"
      className={cn(
        "animate-guide-in z-30 flex flex-col overflow-hidden rounded-2xl border border-slate-200 bg-slate-50/97 text-hk-navy shadow-[0_18px_48px_rgba(11,25,44,0.22)] backdrop-blur-md",
        variant === "map"
          ? cn("absolute inset-x-2 bottom-2 max-h-[78%] sm:inset-x-auto sm:top-3 sm:bottom-3 sm:left-3 sm:max-h-none sm:w-[380px]", panelClassName)
          : "fixed inset-x-2 bottom-2 h-[min(640px,calc(100dvh-5rem))] sm:inset-x-auto sm:right-4 sm:bottom-4 sm:w-[400px]",
      )}
    >
      <header className="flex items-center gap-2.5 border-b border-slate-200 bg-white px-3.5 py-3">
        <span className="flex size-9 shrink-0 items-center justify-center rounded-xl bg-hk-navy text-white">
          <Compass className="size-4.5" strokeWidth={2} aria-hidden />
        </span>
        <span className="min-w-0 flex-1 leading-tight">
          <span className="flex items-center gap-1.5 text-[14px] font-bold">
            Průvodce
            {lastMode && <ModeBadge mode={lastMode} />}
          </span>
          <span className="block truncate text-[11.5px] text-slate-500">Ptejte se na budovy kraje — ukážu je v mapě</span>
        </span>
        {steps.length > 0 && (
          <IconButton label="Začít znovu" onClick={reset}>
            <RotateCcw className="size-4" aria-hidden />
          </IconButton>
        )}
        <IconButton label="Zavřít průvodce" onClick={() => setOpen(false)}>
          <X className="size-4" aria-hidden />
        </IconButton>
      </header>

      <div ref={scrollRef} className="relative min-h-0 flex-1 space-y-3 overflow-y-auto overscroll-contain px-3.5 py-3.5" aria-live="polite">
        {steps.length === 0 && <Welcome starters={starters} onAsk={ask} />}

        {steps.map((step) => {
          const isLatest = step === steps.at(-1);
          const isOpen = isLatest || expanded === step.id;

          return (
            <article key={step.id} className="animate-guide-step">
              <button
                type="button"
                onClick={() => !isLatest && revisit(step)}
                disabled={isLatest}
                className={cn(
                  "flex w-full items-center gap-2 text-left text-[12px] font-bold",
                  isLatest ? "cursor-default text-hk-navy" : "rounded-lg px-1 py-1 text-slate-500 hover:bg-white hover:text-hk-navy",
                )}
              >
                <span className={cn("size-1.5 shrink-0 rounded-full", isLatest ? "bg-hk-red" : "bg-slate-300")} aria-hidden />
                <span className={cn(!isOpen && "truncate")}>{step.question}</span>
                {!isLatest && step.state === "done" && step.answer.facilities.length > 0 && (
                  <span className="ml-auto shrink-0 font-normal text-slate-400">{step.answer.facilities.length} v mapě</span>
                )}
              </button>

              {isOpen && (
                <div className="mt-2 pl-3.5">
                  {step.state === "loading" && <Thinking />}
                  {step.state === "error" && (
                    <p className="rounded-xl border border-red-200 bg-red-50 px-3.5 py-3 text-[13px] text-red-800" role="alert">
                      {step.message}{" "}
                      <button type="button" onClick={() => ask(step.question)} className="font-bold underline">
                        Zkusit znovu
                      </button>
                    </p>
                  )}
                  {step.state === "done" && (
                    <GuideAnswer answer={step.answer} latest={step === latestDone && isLatest} onAsk={ask} onOpenFacility={onOpenFacility} onOpenDistrict={onOpenDistrict} />
                  )}
                </div>
              )}
            </article>
          );
        })}
      </div>

      <form
        className="border-t border-slate-200 bg-white px-3 pt-2.5 pb-2"
        onSubmit={(event) => {
          event.preventDefault();
          void ask(question);
        }}
      >
        {contextLabel && (
          <p className="mb-2 flex items-center gap-1.5 truncate text-[11.5px] text-slate-500">
            <MapPin className="size-3.5 shrink-0 text-hk-red" aria-hidden />
            Ptáte se k: <b className="truncate text-hk-navy">{contextLabel}</b>
          </p>
        )}
        <div className="flex items-end gap-2 rounded-xl border border-slate-300 bg-white px-3 py-2 focus-within:border-hk-blue focus-within:ring-3 focus-within:ring-hk-blue/15">
          <label htmlFor="guide-question" className="sr-only">
            Otázka pro průvodce
          </label>
          <textarea
            id="guide-question"
            ref={inputRef}
            value={question}
            onChange={(event) => setQuestion(event.target.value.slice(0, MAX_QUESTION))}
            onKeyDown={handleKeyDown}
            rows={1}
            maxLength={MAX_QUESTION}
            placeholder={contextLabel ? "Např. proč tu audit hlásí problém?" : "Např. kde se dýchá nejhůř v okrese Náchod?"}
            className="field-sizing-content max-h-28 min-h-6 flex-1 resize-none bg-transparent text-[13.5px] leading-6 outline-none placeholder:text-slate-400"
          />
          <button
            type="submit"
            disabled={!question.trim() || loading}
            aria-label="Zeptat se"
            className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-hk-red text-white transition-colors hover:bg-hk-red-dark disabled:bg-slate-200 disabled:text-slate-400"
          >
            <ArrowUp className="size-4" strokeWidth={2.5} aria-hidden />
          </button>
        </div>
        <p className="mt-1.5 flex items-center justify-between gap-2 text-[10.5px] text-slate-400">
          <span className="flex items-center gap-1">
            <ShieldCheck className="size-3" aria-hidden />
            Jen o budovách kraje · nepíše kód ani nic mimo účel
          </span>
          {question.length > 300 && <span className="tabular-nums">{question.length}/{MAX_QUESTION}</span>}
        </p>
      </form>
    </section>
  );
}

function Launcher({ variant, hasHistory, onOpen }: { variant: "map" | "floating"; hasHistory: boolean; onOpen: () => void }) {
  return (
    <button
      type="button"
      onClick={onOpen}
      data-tour="ai-pruvodce"
      className={cn(
        "group z-30 inline-flex h-11 items-center gap-2.5 rounded-full bg-hk-navy pr-3 pl-4 text-[13.5px] font-bold whitespace-nowrap text-white shadow-[0_12px_28px_rgba(11,25,44,0.32)] transition-colors hover:bg-hk-blue focus-visible:outline-3 focus-visible:outline-offset-2 focus-visible:outline-[#8DAACD]",
        variant === "map" ? "absolute bottom-6 left-1/2 -translate-x-1/2" : "fixed right-4 bottom-4",
      )}
    >
      <span className="relative flex size-5 items-center justify-center">
        <span className="absolute inset-0 animate-ping rounded-full bg-hk-red/50 [animation-duration:2.6s]" aria-hidden />
        <Sparkles className="relative size-4.5 text-white" aria-hidden />
      </span>
      {hasHistory ? "Pokračovat v průvodci" : "Zeptejte se průvodce"}
      <kbd className="hidden rounded-md border border-white/20 px-1.5 font-mono text-[11px] font-normal text-white/60 sm:inline">/</kbd>
    </button>
  );
}

function Welcome({ starters, onAsk }: { starters: GuideStarter[]; onAsk: (question: string) => void }) {
  return (
    <div>
      <p className="text-[15px] font-bold">Kam se podíváme?</p>
      <p className="mt-1 text-[12.5px] leading-relaxed text-slate-600">
        Napište otázku vlastními slovy. Najdu budovy, ukážu je v mapě a řeknu, z jakých dat odpověď pochází.
      </p>
      <ul className="mt-3 grid grid-cols-2 gap-2">
        {starters.map((starter) => {
          const Icon = starter.icon;

          return (
            <li key={starter.question}>
              <button
                type="button"
                onClick={() => onAsk(starter.question)}
                className="group flex h-full w-full flex-col gap-2 rounded-xl border border-slate-200 bg-white p-3 text-left transition-colors hover:border-hk-red/40"
              >
                <span className="flex size-7 items-center justify-center rounded-lg bg-slate-100 text-hk-navy group-hover:bg-hk-red group-hover:text-white">
                  <Icon className="size-4" strokeWidth={1.9} aria-hidden />
                </span>
                <span className="text-[12.5px] leading-snug font-bold">{starter.label}</span>
              </button>
            </li>
          );
        })}
      </ul>
    </div>
  );
}

// Čekání s popisem toho, co průvodce dělá — odpověď s nástroji nad daty trvá pár sekund
function Thinking() {
  const [stage, setStage] = useState(0);

  useEffect(() => {
    const timer = setInterval(() => setStage((current) => Math.min(current + 1, LOADING_STAGES.length - 1)), 1400);
    return () => clearInterval(timer);
  }, []);

  return (
    <div className="rounded-xl border border-slate-200 bg-white px-3.5 py-3" role="status">
      <p className="flex items-center gap-2 text-[12.5px] text-slate-500">
        <span className="flex gap-1" aria-hidden>
          <span className="size-1.5 animate-bounce rounded-full bg-hk-red [animation-delay:-0.3s]" />
          <span className="size-1.5 animate-bounce rounded-full bg-hk-red [animation-delay:-0.15s]" />
          <span className="size-1.5 animate-bounce rounded-full bg-hk-red" />
        </span>
        {LOADING_STAGES[stage]}
      </p>
      <div className="mt-3 space-y-2" aria-hidden>
        <div className="h-2.5 w-11/12 animate-pulse rounded bg-slate-100" />
        <div className="h-2.5 w-3/4 animate-pulse rounded bg-slate-100" />
      </div>
    </div>
  );
}

function ModeBadge({ mode }: { mode: GuideMode }) {
  if (mode === "Blocked") return null;

  return mode === "Ai" ? (
    <span className="inline-flex items-center gap-1 rounded-full bg-hk-navy/8 px-1.5 py-px text-[10px] font-bold text-hk-blue" title="Odpověď složil jazykový model nad daty kraje a znalostmi SmartAuditu">
      <Sparkles className="size-2.5" aria-hidden />
      AI nad daty
    </span>
  ) : (
    <span className="inline-flex items-center gap-1 rounded-full bg-slate-200/70 px-1.5 py-px text-[10px] font-bold text-slate-600" title="AI není dostupná — odpověď je složená přímo z dat a znalostí">
      <Database className="size-2.5" aria-hidden />
      Bez AI
    </span>
  );
}

function IconButton({ label, onClick, children }: { label: string; onClick: () => void; children: ReactNode }) {
  return (
    <button type="button" onClick={onClick} aria-label={label} title={label} className="rounded-md p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-hk-navy">
      {children}
    </button>
  );
}

function lastDone(steps: GuideStep[]) {
  return steps.findLast((s): s is GuideStep & { state: "done"; answer: GuideAnswerData } => s.state === "done");
}

// Úložiště může chybět (soukromé okno) — průvodce pak jen nepamatuje historii
function readSteps(key: string): GuideStep[] {
  try {
    const raw = sessionStorage.getItem(key);
    const parsed = raw ? (JSON.parse(raw) as GuideStep[]) : [];
    return Array.isArray(parsed) ? parsed.filter((s) => s.state === "done") : [];
  } catch {
    return [];
  }
}

function writeSteps(key: string, steps: GuideStep[]) {
  try {
    sessionStorage.setItem(key, JSON.stringify(steps.filter((s) => s.state === "done")));
  } catch {
    // úložiště nedostupné — nevadí
  }
}
