import type { Metadata, Viewport } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import localFont from "next/font/local";
import { ThermalScrollbar } from "@/components/thermal-scrollbar";
import "./globals.css";

// Firemní písmo Hradce Králové — stejné soubory jako v prototypu
const hradec = localFont({
  variable: "--font-hradec",
  display: "swap",
  src: [
    { path: "./fonts/Hradec-Regular.woff2", weight: "400", style: "normal" },
    { path: "./fonts/Hradec-Bold.woff2", weight: "700", style: "normal" },
  ],
});

// Záložní písmo pro znaky, které Hradec nemá (např. dolní index v CO₂)
const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin", "latin-ext"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin", "latin-ext"],
});

export const metadata: Metadata = {
  title: "SmartAudit · Královéhradecký kraj",
  description:
    "Energetický audit budov zřizovaných Královéhradeckým krajem nad otevřenými daty KHK.",
  applicationName: "SmartAudit",
  // Favicony (icon.svg, favicon.ico, apple-icon.png), manifest a náhled pro sdílení jsou soubory v app/ — Next je vloží sám
};

export const viewport: Viewport = {
  themeColor: "#0b192c",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="cs"
      className={`${hradec.variable} ${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <body className="flex h-dvh flex-col">
        <ThermalScrollbar />
        {children}
      </body>
    </html>
  );
}
