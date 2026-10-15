import { LogoSymbol } from "@/components/brand/logo";

export default function Loading() {
  return (
    <>
      <div className="h-16 shrink-0 border-b border-slate-200 bg-white" />
      <main className="flex flex-1 flex-col items-center justify-center gap-4 text-sm text-slate-500">
        <LogoSymbol animated className="size-12" />
        Načítám objekty kraje…
      </main>
    </>
  );
}
