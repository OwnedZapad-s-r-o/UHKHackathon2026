import Image from "next/image";
import type { LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

type FeatureRowProps = {
  icon: LucideIcon;
  eyebrow: string;
  title: string;
  children: React.ReactNode;
  image: { src: string; alt: string; width: number; height: number };
  // Obrázek vlevo — řádky se střídají, aby stránka neběžela jen jedním sloupcem
  reversed?: boolean;
};

// Jedna funkce aplikace: text s odrážkami a snímek obrazovky
export function FeatureRow({ icon: Icon, eyebrow, title, children, image, reversed = false }: FeatureRowProps) {
  return (
    <div className="grid items-center gap-8 lg:grid-cols-2 lg:gap-14">
      <div className={cn(reversed && "lg:order-2")}>
        <p className="inline-flex items-center gap-2 text-[12px] font-bold tracking-wide text-hk-red uppercase">
          <Icon className="size-4" aria-hidden />
          {eyebrow}
        </p>
        <h3 className="mt-2 text-[26px] leading-tight font-bold tracking-tight sm:text-[30px]">{title}</h3>
        <div className="mt-3 space-y-3 text-[15px] leading-relaxed text-slate-600">{children}</div>
      </div>
      <div className={cn("overflow-hidden rounded-2xl border border-slate-200 bg-slate-50 shadow-[0_24px_60px_rgba(11,25,44,0.14)]", reversed && "lg:order-1")}>
        <Image src={image.src} alt={image.alt} width={image.width} height={image.height} sizes="(min-width: 1024px) 560px, 100vw" className="h-auto w-full" />
      </div>
    </div>
  );
}

export function Bullets({ items }: { items: string[] }) {
  return (
    <ul className="space-y-1.5">
      {items.map((item) => (
        <li key={item} className="flex gap-2.5">
          <span className="mt-2.5 size-1.5 shrink-0 rounded-full bg-hk-red" aria-hidden />
          <span>{item}</span>
        </li>
      ))}
    </ul>
  );
}
