import type { NextConfig } from "next";

// Adresa ASP.NET API z pohledu Next serveru (v Dockeru http://api:8080)
const apiUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5033";

const nextConfig: NextConfig = {
  output: "standalone",

  // Prohlížeč volá jen /api/... na stejné doméně — bez CORS a bez adresy API v bundlu
  async rewrites() {
    return [{ source: "/api/:path*", destination: `${apiUrl}/api/:path*` }];
  },
};

export default nextConfig;
