"use client";

import { useEffect } from "react";

// Paleta termokamery: studená modrá nahoře stránky, přes fialovou, purpurovou a jantar do červené kraje na konci.
// Purpurová mezi fialovou a jantarem drží přechod sytý (přímý přechod vede přes kalnou hnědou)
const STOPS: [number, [number, number, number]][] = [
  [0, [59, 130, 246]],
  [0.3, [139, 92, 246]],
  [0.55, [236, 72, 153]],
  [0.8, [245, 158, 11]],
  [1, [217, 4, 41]],
];

function heatColor(t: number) {
  const x = Math.min(1, Math.max(0, t));
  const i = STOPS.findIndex(([at]) => at >= x);
  if (i <= 0) return `rgb(${STOPS[0][1].join(" ")})`;
  const [a, from] = STOPS[i - 1];
  const [b, to] = STOPS[i];
  const k = (x - a) / (b - a);
  return `rgb(${from.map((c, j) => Math.round(c + (to[j] - c) * k)).join(" ")})`;
}

// Posuvník jako snímek z termokamery — každý posouvatelný prvek se s posunem dolů „ohřívá“.
// Barvu jezdce přebírá CSS z proměnných --scroll-heat a --scroll-heat-hot (globals.css)
export function ThermalScrollbar() {
  useEffect(() => {
    const pending = new Set<HTMLElement>();
    let frame = 0;

    const paint = () => {
      frame = 0;
      for (const el of pending) {
        const vertical = el.scrollHeight - el.clientHeight;
        const horizontal = el.scrollWidth - el.clientWidth;
        const t = vertical > 0 ? el.scrollTop / vertical : horizontal > 0 ? el.scrollLeft / horizontal : 0;
        el.style.setProperty("--scroll-heat", heatColor(t));
        el.style.setProperty("--scroll-heat-hot", heatColor(t + 0.18));
      }
      pending.clear();
    };

    // Událost scroll neprobublává — zachytávací fáze na dokumentu ji chytí z libovolného prvku
    const onScroll = (event: Event) => {
      const target = event.target === document ? document.documentElement : event.target;
      if (!(target instanceof HTMLElement)) return;
      pending.add(target);
      frame ||= requestAnimationFrame(paint);
    };

    document.addEventListener("scroll", onScroll, { capture: true, passive: true });
    return () => {
      document.removeEventListener("scroll", onScroll, { capture: true });
      cancelAnimationFrame(frame);
    };
  }, []);

  return null;
}
