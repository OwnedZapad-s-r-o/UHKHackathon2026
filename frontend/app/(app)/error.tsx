"use client";

type ErrorProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

// Nejčastější příčina: API ještě dokončuje první import otevřených dat
export default function Error({ reset }: ErrorProps) {
  return (
    <main className="flex flex-1 flex-col items-center justify-center gap-3 p-4 text-center">
      <p className="font-medium">Data se nepodařilo načíst.</p>
      <p className="max-w-sm text-sm text-muted-foreground">
        Při prvním spuštění se stahují otevřená data kraje a dopočítává telemetrie — trvá to zhruba minutu.
      </p>
      <button type="button" onClick={reset} className="rounded-md border px-3 py-1.5 text-sm hover:bg-muted">
        Zkusit znovu
      </button>
    </main>
  );
}
