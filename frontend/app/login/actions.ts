"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { apiPost, SESSION_COOKIE } from "@/lib/api";
import type { AuthResponse, Role, Session } from "@/types/api";

const ROLES: Role[] = ["Kraj", "Spravce", "Verejnost"];

export type LoginState = { error: string } | null;

async function startSession(auth: AuthResponse) {
  const session: Session = {
    token: auth.token,
    displayName: auth.displayName,
    role: auth.role,
    organizationName: auth.organizationName ?? null,
  };

  (await cookies()).set(SESSION_COOKIE, JSON.stringify(session), {
    httpOnly: true,
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production" && process.env.COOKIE_SECURE === "true",
    path: "/",
    maxAge: 60 * 60 * 8,
  });
}

// Tlačítko „Demo“ — přihlásí předpřipravený účet zvolené role bez hesla
export async function demoLogin(_prevState: LoginState, formData: FormData): Promise<LoginState> {
  const role = formData.get("role");

  if (typeof role !== "string" || !ROLES.includes(role as Role)) {
    return { error: "Neznámá role." };
  }

  const result = await apiPost<AuthResponse>("/api/auth/demo", { role });
  if (!result.ok) {
    return { error: result.message };
  }

  await startSession(result.data);
  // Demo je ukázka pro nového člověka — průvodce aplikací se spustí po každém přihlášení
  redirect("/mapa?pruvodce=1");
}

export async function login(_prevState: LoginState, formData: FormData): Promise<LoginState> {
  const email = formData.get("email");
  const password = formData.get("password");

  if (typeof email !== "string" || typeof password !== "string" || !email.trim() || !password) {
    return { error: "Vyplňte e-mail i heslo." };
  }

  const result = await apiPost<AuthResponse>("/api/auth/login", { email, password });
  if (!result.ok) {
    return { error: result.message };
  }

  await startSession(result.data);
  redirect("/mapa");
}

export async function logout() {
  (await cookies()).delete(SESSION_COOKIE);
  redirect("/login");
}
