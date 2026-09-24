import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ChevronLeft } from "lucide-react";
import QRCode from "qrcode";
import { getPublicFacilityByCode } from "@/lib/public-api";
import { requestOrigin } from "@/lib/request-origin";
import { PrintButton } from "./_components/print-button";

export const metadata: Metadata = {
  title: "QR kód na dveře · SmartAudit KHK",
};

// Tisková cedulka A5 na dveře třídy nebo ke vchodu — QR vede na veřejnou kartu objektu
export default async function QrPage({ params }: PageProps<"/objekty/[kod]/qr">) {
  const { kod } = await params;
  const facility = await getPublicFacilityByCode(kod);
  if (!facility) notFound();

  const url = `${await requestOrigin()}/objekty/${facility.code}`;
  const qr = await QRCode.toString(url, { type: "svg", margin: 0, errorCorrectionLevel: "M", color: { dark: "#0b192c", light: "#ffffff" } });

  return (
    <main className="mx-auto max-w-xl px-4 py-8 print:max-w-none print:p-0">
      <div className="mb-5 flex items-center justify-between print:hidden">
        <Link href={`/objekty/${facility.code}`} className="inline-flex items-center gap-1 text-[13px] text-slate-500 hover:text-hk-red">
          <ChevronLeft className="size-4" aria-hidden />
          Zpět na kartu objektu
        </Link>
        <PrintButton />
      </div>

      <article className="rounded-3xl border-2 border-hk-navy p-8 text-center print:rounded-none print:border-0">
        <Image src="/brand/hk-logo-white.png" alt="Hradec Králové" width={330} height={142} className="mx-auto h-10 w-auto invert" />
        <h1 className="mt-6 text-[30px] leading-tight font-bold tracking-tight">Jak se dýchá v této budově?</h1>
        <p className="mt-2 text-[15px] text-slate-600">Naskenujte a uvidíte CO₂, teplotu a kdy větrat — bez přihlášení.</p>

        {/* SVG z knihovny qrcode je vlastní výstup na serveru, ne vstup od uživatele */}
        <div className="mx-auto mt-6 w-64 [&>svg]:h-auto [&>svg]:w-full" dangerouslySetInnerHTML={{ __html: qr }} />

        <p className="mt-6 text-[17px] font-bold">{facility.name}</p>
        <p className="mt-1 font-mono text-[12px] break-all text-slate-500">{url}</p>
        <p className="mt-6 text-[12px] text-slate-500">SmartAudit KHK · Královéhradecký kraj</p>
      </article>
    </main>
  );
}
