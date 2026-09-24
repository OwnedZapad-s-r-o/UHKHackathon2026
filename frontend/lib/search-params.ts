// Stav obrazovek (vybraný objekt, záložka, filtry) žije v URL — dá se sdílet odkazem a funguje tlačítko zpět

export type RawSearchParams = Record<string, string | string[] | undefined>;

export function readParam(params: RawSearchParams, key: string): string | undefined {
  const value = params[key];
  return typeof value === "string" && value.length > 0 ? value : undefined;
}

// Nová URL se změněnými parametry; null parametr odstraní
export function withParams(path: string, current: RawSearchParams | URLSearchParams, changes: Record<string, string | null>) {
  const params = current instanceof URLSearchParams ? new URLSearchParams(current) : new URLSearchParams();

  if (!(current instanceof URLSearchParams)) {
    for (const [key, value] of Object.entries(current)) {
      if (typeof value === "string") params.set(key, value);
    }
  }

  for (const [key, value] of Object.entries(changes)) {
    if (value === null) params.delete(key);
    else params.set(key, value);
  }

  const query = params.toString();
  return query ? `${path}?${query}` : path;
}

// Objekt v mapě i s jeho okresem — mapa ukáže špendlíky okresu a po zavření panelu zůstane v něm
export function facilityMapHref(facility: { id: string; districtCode: string }, panel: "telemetrie" | "audit") {
  return `/mapa?okres=${facility.districtCode}&objekt=${facility.id}&panel=${panel}` as const;
}
