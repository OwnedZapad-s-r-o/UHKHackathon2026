import { Check } from "lucide-react";
import { cn } from "@/lib/utils";

// Kroky připojení čidla — stejné na seznamu gatewayí i na detailu, kde se posouvají samy podle stavu gatewaye
export const SENSOR_STEPS = [
  { title: "Zadejte gateway", text: "budova, název a id" },
  { title: "Nastavte ji v budově", text: "adresa brokeru MQTT" },
  { title: "Gateway odpoví", text: "aplikace najde čidla" },
  { title: "Čidla do místností", text: "u každého vyberete místnost" },
  { title: "Hotovo", text: "měření v mapě a auditu" },
] as const;

// Aktuální krok 1–5; 5 = vše hotovo (všechny kroky odškrtnuté)
export function SensorSteps({ current, className }: { current: number; className?: string }) {
  return (
    <ol className={cn("grid gap-2 rounded-2xl border border-slate-200 bg-white p-3 sm:grid-cols-5", className)} aria-label="Postup připojení čidla">
      {SENSOR_STEPS.map((step, index) => {
        const number = index + 1;
        const done = number < current || current === SENSOR_STEPS.length;
        const active = number === current && !done;

        return (
          <li
            key={step.title}
            aria-current={active ? "step" : undefined}
            className={cn("flex items-center gap-2.5 rounded-xl px-2.5 py-2 transition-colors", active && "bg-red-50 ring-1 ring-hk-red/25")}
          >
            <span
              className={cn(
                "flex size-7 shrink-0 items-center justify-center rounded-full font-mono text-[12px] font-bold",
                done ? "bg-emerald-500 text-white" : active ? "bg-hk-red text-white" : "bg-slate-100 text-slate-500",
              )}
            >
              {done ? <Check className="size-3.5" strokeWidth={3} aria-label="hotovo" /> : number}
            </span>
            <span className="min-w-0 leading-tight">
              <span className={cn("block truncate text-[13px] font-bold", !done && !active && "text-slate-500")}>{step.title}</span>
              <span className="block truncate text-[11.5px] text-slate-500">{step.text}</span>
            </span>
          </li>
        );
      })}
    </ol>
  );
}
