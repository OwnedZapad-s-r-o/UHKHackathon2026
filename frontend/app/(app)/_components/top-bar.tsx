import type { ReactNode } from "react";

type TopBarProps = {
  title: string;
  children?: ReactNode;
};

export function TopBar({ title, children }: TopBarProps) {
  return (
    <header className="flex h-16 shrink-0 items-center gap-4 border-b border-slate-200 bg-white px-6">
      <h1 className="text-base font-bold tracking-tight">{title}</h1>
      {children && <div className="ml-auto flex items-center gap-3">{children}</div>}
    </header>
  );
}
