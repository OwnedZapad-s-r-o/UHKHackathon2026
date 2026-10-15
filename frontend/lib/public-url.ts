import "server-only";

import { requestOrigin } from "@/lib/request-origin";

// Veřejná adresa aplikace pro QR kódy: PUBLIC_URL (za běhu serveru) nebo NEXT_PUBLIC_PUBLIC_URL (vložená při buildu).
// Null = není nastavená a platí adresa, na které uživatel stránku otevřel (window.location.origin).
export function configuredPublicUrl() {
  const configured = process.env.PUBLIC_URL || process.env.NEXT_PUBLIC_PUBLIC_URL;

  return configured ? configured.replace(/\/+$/, "") : null;
}

export async function publicBaseUrl() {
  return configuredPublicUrl() ?? (await requestOrigin());
}
