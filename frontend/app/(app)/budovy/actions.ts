"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { apiSendAuthorized, getSession } from "@/lib/api";
import type { FacilityCategory } from "@/types/api";

export type NewBuildingInput = {
  name: string;
  category: FacilityCategory;
  kind: string | null;
  latitude: number;
  longitude: number;
  street: string | null;
  houseNumber: string | null;
  psc: string | null;
  floors: number | null;
  constructionYear: number | null;
};

const CATEGORIES: FacilityCategory[] = ["School", "Social", "Health", "Culture"];
const ID_PATTERN = /^[0-9a-f-]{36}$/i;

// Budova se po uložení rovnou otevře v mapě (přelet ve 3D + panel); ?nova=1 ukáže, že se čidla teprve připravují
export async function createBuilding(input: NewBuildingInput): Promise<{ error: string }> {
  const session = await getSession();
  if (session?.role !== "Spravce") return { error: "Budovu může přidat jen správce budovy." };

  const name = input.name.trim();
  if (name.length < 3) return { error: "Napište název budovy (aspoň 3 znaky)." };
  if (!CATEGORIES.includes(input.category)) return { error: "Vyberte, k čemu budova slouží." };
  if (!Number.isFinite(input.latitude) || !Number.isFinite(input.longitude)) return { error: "Vyberte místo budovy v mapě." };
  if (input.floors !== null && (!Number.isInteger(input.floors) || input.floors < 1 || input.floors > 30)) return { error: "Počet podlaží musí být 1 až 30." };
  if (input.constructionYear !== null && !Number.isInteger(input.constructionYear)) return { error: "Rok výstavby není platný." };

  const result = await apiSendAuthorized<{ id: string; code: string }>("POST", "/api/facilities", { ...input, name });
  if (!result.ok) return { error: result.message };

  // Nová budova mění mapu, seznam budov i počty v postranním panelu
  revalidatePath("/", "layout");
  redirect(`/mapa?objekt=${result.data.id}&nova=1`);
}

export async function deleteBuilding(facilityId: string): Promise<{ error: string }> {
  const session = await getSession();
  if (session?.role !== "Spravce") return { error: "Budovu může odebrat jen správce budovy." };
  if (!ID_PATTERN.test(facilityId)) return { error: "Neplatná budova." };

  const result = await apiSendAuthorized<boolean>("DELETE", `/api/facilities/${facilityId}`);
  if (!result.ok) return { error: result.message };

  revalidatePath("/", "layout");
  redirect("/mapa");
}
