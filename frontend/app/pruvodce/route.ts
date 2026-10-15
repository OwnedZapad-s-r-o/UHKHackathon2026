import { askGuide } from "@/lib/api";
import type { GuideContext, GuidePage } from "@/types/api";

const PAGES: GuidePage[] = ["mapa", "objekty", "karta", "uvod"];
const MAX_QUESTION = 400;
const MAX_HISTORY = 4;

// Průvodce pro aplikaci i veřejné stránky — prohlížeč token nemá (httpOnly cookie), proto přes server.
// Tvar vstupu se kontroluje tady, obsah (ochranné filtry) v API.
export async function POST(request: Request) {
  const body = (await request.json().catch(() => null)) as Record<string, unknown> | null;
  const question = typeof body?.question === "string" ? body.question.slice(0, MAX_QUESTION + 1) : "";

  const rawContext = (body?.context ?? {}) as Record<string, unknown>;
  const context: GuideContext = {
    page: PAGES.includes(rawContext.page as GuidePage) ? (rawContext.page as GuidePage) : "objekty",
    facilityId: typeof rawContext.facilityId === "string" && /^[0-9a-f-]{36}$/i.test(rawContext.facilityId) ? rawContext.facilityId : null,
    districtCode: typeof rawContext.districtCode === "string" && /^CZ\d{4}$/.test(rawContext.districtCode) ? rawContext.districtCode : null,
  };

  const history = Array.isArray(body?.history)
    ? body.history.filter((item): item is string => typeof item === "string").map((item) => item.slice(0, MAX_QUESTION)).slice(-MAX_HISTORY)
    : [];

  const clientIp = request.headers.get("x-forwarded-for")?.split(",")[0]?.trim() ?? request.headers.get("x-real-ip");
  const result = await askGuide({ question, context, history }, clientIp ?? null);

  return result.ok ? Response.json(result.data) : Response.json({ message: result.message }, { status: result.status });
}
