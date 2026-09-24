import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import localFont from "next/font/local";
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
  icons: {
    icon: [
      { url: "/icon.svg", type: "image/svg+xml" },
      { url: "/favicon.svg", type: "image/svg+xml" },
      { url: "/hkIcon.ico" },
    ],
    shortcut: "/icon.svg",
    apple: "/apple-icon.png",
  },
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="cs"
      className={`${hradec.variable} ${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <head>
        <link rel="icon" type="image/svg+xml" href="/icon.svg" />
        <link rel="alternate icon" href="/hkIcon.ico" />
        <link rel="apple-touch-icon" href="/apple-icon.png" />
      </head>
      <body className="flex h-dvh flex-col">{children}</body>
    </html>
  );
}
