import type { NextConfig } from "next";

// Adresa ASP.NET API z pohledu Next serveru (v Dockeru http://api:8080)
const apiUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5033";

// Veřejná doména (Raspberry Pi / tunel) — Server Actions přijmou požadavek i tehdy, když proxy nepředá x-forwarded-host
const publicHosts = [process.env.PUBLIC_URL, process.env.NEXT_PUBLIC_PUBLIC_URL, ...(process.env.SERVER_ACTIONS_ALLOWED_ORIGINS ?? "").split(",")]
  .map((value) => value?.trim())
  .filter((value): value is string => Boolean(value))
  .map((value) => {
    try {
      return new URL(/^https?:\/\//.test(value) ? value : `https://${value}`).host;
    } catch {
      return value;
    }
  });

const nextConfig: NextConfig = {
  output: "standalone",

  // Dokumentace API (Scalar) žije na /api/open/docs/ s lomítkem na konci — Next ho nesmí odstraňovat
  skipTrailingSlashRedirect: true,

  experimental: {
    serverActions: {
      allowedOrigins: [...new Set(publicHosts)],
    },
  },

  // Prohlížeč volá jen /api/... na stejné doméně — bez CORS a bez adresy API v bundlu
  async rewrites() {
    return [
      // Obecné pravidlo níž koncové lomítko zahodí a Scalar by přesměrovával dokola
      { source: "/api/open/docs/", destination: `${apiUrl}/api/open/docs/` },
      { source: "/api/:path*", destination: `${apiUrl}/api/:path*` },
    ];
  },
};

export default nextConfig;
