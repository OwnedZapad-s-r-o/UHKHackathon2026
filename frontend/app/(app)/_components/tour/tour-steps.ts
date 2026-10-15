import type { Role } from "@/types/api";

// Krok průvodce: target = hodnota data-tour prvku, který se zvýrazní. Bez targetu je krok karta uprostřed obrazovky.
export type TourStep = {
  target?: string;
  title: string;
  body: string;
  // Úvodní krok — tři body „co tu najdete“
  highlights?: { title: string; text: string }[];
  // Poslední krok nabídne otevřít konkrétní budovu a pokračovat průvodcem v ní
  finale?: { action: string; skip: string };
};

export type TourPhase = "aplikace" | "objekt";

const WELCOME: Record<Role, TourStep> = {
  Kraj: {
    title: "Vítejte v SmartAuditu",
    body: "Za minutu vám ukážeme, jak najít budovy kraje, kde se zbytečně topí nebo špatně dýchá, a co s tím dělat.",
    highlights: [
      { title: "Kde je problém", text: "mapa kraje po okresech a budovách" },
      { title: "Proč a kolik to stojí", text: "audit z měření s odhadem ztráty v Kč" },
      { title: "Co s tím", text: "PDF podklad a návrh opatření od AI" },
    ],
  },
  Spravce: {
    title: "Vítejte v SmartAuditu",
    body: "Uvidíte své budovy, co v nich odhalilo měření, a jak připojit vlastní čidla IQRF. Provedeme vás za minutu.",
    highlights: [
      { title: "Vaše budovy", text: "stav každé budovy na mapě" },
      { title: "Co řešit", text: "zjištění s vysvětlením a důkazem z měření" },
      { title: "Vlastní čidla", text: "připojení IQRF gateway krok za krokem" },
    ],
  },
  Verejnost: {
    title: "Jak se dýchá v budovách kraje",
    body: "Najdete tu školy, domovy a další budovy kraje a uvidíte, jaký je v nich vzduch. Ukážeme vám, kde co je.",
    highlights: [
      { title: "Najděte budovu", text: "na mapě nebo podle názvu" },
      { title: "Kvalita vzduchu", text: "CO₂ a teplota po místnostech" },
      { title: "Kdy větrat", text: "srozumitelně, bez odborných pojmů" },
    ],
  },
};

const MAP: TourStep = {
  target: "mapa",
  title: "Kraj po okresech",
  body: "Barva okresu ukazuje, jaký podíl budov má zjištění. Bublina říká, kolik budov v okrese je a kolik z nich má problém. Kliknutím na okres mapa přeletí na jeho budovy.",
};

const LEGEND: TourStep = {
  target: "legenda",
  title: "Co znamenají barvy",
  body: "Špendlík má barvu stavu budovy: červená je kritická anomálie, oranžová varování, zelená v pořádku. Ikona uvnitř říká druh budovy.",
};

const FILTERS: TourStep = {
  target: "filtry",
  title: "Okres a hledání",
  body: "Okres přepnete i tady nahoře. Hledání najde budovu podle názvu, obce nebo kódu.",
};

const HELP: TourStep = {
  target: "pruvodce",
  title: "Průvodce kdykoli znovu",
  body: "Když si nebudete jistí, spusťte průvodce odsud.",
};

const STEPS: Record<Role, TourStep[]> = {
  Kraj: [
    WELCOME.Kraj,
    MAP,
    LEGEND,
    FILTERS,
    { target: "zjisteni", title: "Jen budovy se zjištěním", body: "Přepínač schová budovy bez problému — v mapě, portfoliu i anomáliích." },
    { target: "nav-portfolio", title: "Portfolio", body: "Všechny budovy v tabulce, seřazené podle odhadované roční ztráty. Kliknutím na název otevřete budovu v mapě." },
    { target: "nav-anomalie", title: "Anomálie", body: "Co je kde špatně, s důkazem z měření a dopadem v Kč. Začněte od kritických." },
    { target: "nav-reporty", title: "PDF pro jednání", body: "Audit budovy nebo přehled okresu s mapou — podklad pro radu kraje nebo zřizovatele." },
    { target: "nav-senzory", title: "Skutečná čidla IQRF", body: "Připojte gateway v budově. Průvodce vás provede nastavením i přiřazením čidel do místností." },
    HELP,
    {
      title: "Zkuste to na konkrétní budově",
      body: "Otevřeme budovu s nejvážnějším zjištěním a ukážeme, kde najdete měření, audit a PDF.",
      finale: { action: "Otevřít budovu s problémem", skip: "Prozkoumám sám" },
    },
  ],
  Spravce: [
    WELCOME.Spravce,
    MAP,
    LEGEND,
    { target: "nav-anomalie", title: "Co řešit ve vašich budovách", body: "Zjištění seřazená podle závažnosti, s vysvětlením, proč vznikla, a co s nimi." },
    { target: "nav-reporty", title: "PDF audit", body: "Audit budovy s grafy a doporučením — stáhnete ho pro vedení nebo zřizovatele." },
    { target: "nav-senzory", title: "Máte v budově IQRF gateway?", body: "Tady ji připojíte. Aplikace sama najde čidla, vy jen vyberete místnosti. Skutečná měření pak nahradí simulaci." },
    HELP,
    {
      title: "Zkuste to na své budově",
      body: "Otevřeme budovu, která má zjištění, a ukážeme, kde najdete měření, audit a PDF.",
      finale: { action: "Otevřít budovu", skip: "Prozkoumám sám" },
    },
  ],
  Verejnost: [
    WELCOME.Verejnost,
    { ...MAP, title: "Najděte svou budovu", body: "Klikněte na okres, mapa na něj přeletí. Pak klikněte na špendlík školy nebo domova." },
    LEGEND,
    { ...FILTERS, title: "Hledání podle názvu", body: "Znáte název školy nebo obec? Napište ji sem." },
    HELP,
    {
      title: "Podívejte se, jak se dýchá",
      body: "Otevřeme jednu budovu a ukážeme, kde najdete CO₂ a teplotu po místnostech.",
      finale: { action: "Otevřít budovu", skip: "Prozkoumám sám" },
    },
  ],
};

// Druhá část průvodce — po otevření budovy z posledního kroku
const FACILITY_STEPS: Record<Role, TourStep[]> = {
  Kraj: [
    { target: "panel", title: "Inspekční panel", body: "Vše o vybrané budově: údaje kraje a katastru, stav a hlavní zjištění." },
    {
      target: "panel-zalozky",
      title: "Záložky budovy",
      body: "Telemetrie ukazuje teplotu, CO₂ a spotřebu. Audit vysvětlí zjištění, spočítá ztrátu a navrhne opatření s pomocí AI.",
    },
    { target: "mapa", title: "Budova ve 3D", body: "Vybraná budova svítí v barvě stavu. Mapu natočíte pravým tlačítkem myši, Esc výběr zruší." },
    { target: "panel-pdf", title: "PDF audit", body: "Jedním klikem stáhnete audit budovy s grafy — hotový podklad pro jednání. Tím je prohlídka u konce." },
  ],
  Spravce: [
    { target: "panel", title: "Vaše budova", body: "Údaje o budově, její stav a hlavní zjištění." },
    { target: "panel-zalozky", title: "Záložky budovy", body: "Telemetrie ukazuje měření po místnostech, Audit vysvětlí, co je špatně, a navrhne opatření." },
    { target: "panel-pdf", title: "PDF audit", body: "Audit budovy stáhnete pro vedení nebo zřizovatele. Tím je prohlídka u konce." },
  ],
  Verejnost: [
    { target: "panel", title: "Jak se tu dýchá", body: "Aktuální teplota a CO₂. Níže jsou jednotlivé místnosti a u každé, jestli je čas vyvětrat." },
    { target: "mapa", title: "Budova ve 3D", body: "Vybraná budova svítí v barvě stavu. Esc výběr zruší a můžete hledat dál. Tím je prohlídka u konce." },
  ],
};

export function tourSteps(role: Role, phase: TourPhase) {
  return phase === "aplikace" ? STEPS[role] : FACILITY_STEPS[role];
}
