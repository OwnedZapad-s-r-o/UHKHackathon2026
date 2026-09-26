// Povel pro mapu z prvků mimo ni (drobečková navigace nad mapou): „vrať se na celý kraj“.
// Změna URL to neumí vyjádřit — zavření objektu křížkem má výřez nechat, „Celý kraj“ má přeletět domů.
export const MAP_HOME_EVENT = "smartaudit:map-home";

export function requestMapHome() {
  window.dispatchEvent(new Event(MAP_HOME_EVENT));
}
