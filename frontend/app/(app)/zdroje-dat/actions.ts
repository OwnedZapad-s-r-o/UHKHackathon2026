"use server";

import { revalidatePath } from "next/cache";
import { apiPostAuthorized } from "@/lib/api";
import type { OpenDataSync } from "@/types/api";

export type SyncOpenDataResult = { sync: OpenDataSync } | { error: string };

// Živá synchronizace se zdroji (Data KHK, ČÚZK, Open-Meteo, Registr smluv) — API hlídá timeout 5 s na zdroj
// a souběžné kliknutí vrátí poslední stav, takže tlačítko smí zkusit kdokoli
export async function syncOpenData(): Promise<SyncOpenDataResult> {
  const result = await apiPostAuthorized<OpenDataSync>("/api/opendata/sync");
  if (!result.ok) return { error: result.message };

  revalidatePath("/zdroje-dat");

  return { sync: result.data };
}
