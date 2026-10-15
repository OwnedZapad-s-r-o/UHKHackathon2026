import type { MetadataRoute } from "next";

// Aplikace přidaná na plochu telefonu — ikony z logo kitu (design/logo/web)
export default function manifest(): MetadataRoute.Manifest {
  return {
    name: "SmartAudit",
    short_name: "SmartAudit",
    description: "Energetický audit a kvalita vnitřního vzduchu v budovách Královéhradeckého kraje.",
    lang: "cs",
    start_url: "/",
    display: "standalone",
    theme_color: "#0b192c",
    background_color: "#0b192c",
    icons: [
      { src: "/brand/icon-192.png", sizes: "192x192", type: "image/png" },
      { src: "/brand/icon-512.png", sizes: "512x512", type: "image/png" },
      { src: "/brand/maskable-512.png", sizes: "512x512", type: "image/png", purpose: "maskable" },
    ],
  };
}
