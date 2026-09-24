# SmartAudit KHK — brief pro redesign landing page

## Co je v této složce

| Soubor | K čemu |
|---|---|
| `landing.html` | Současná landing page jako jeden samostatný soubor. Otevři v prohlížeči, funguje offline: styly, písmo Hradec a obrázky jsou uvnitř, skripty nejsou potřeba. |
| `nahled-desktop.png`, `nahled-mobil.png` | Celá stránka jako obrázek (1440 px a 390 px) |
| `obrazky/` | Originály snímků obrazovky (WebP 1440 × 900, telefon 390 × 844) a logo HK |

Živá verze běží lokálně po `docker compose up` na http://localhost:3000. Tam je vidět i samotná aplikace (tlačítko „Vyzkoušet demo“) a veřejné stránky `/objekty`.

---

## 1. Produkt v kostce

**SmartAudit KHK** je webová aplikace pro energetický audit budov, které zřizuje Královéhradecký kraj:
školy, domovy pro seniory, nemocnice a kulturní zařízení. Dnes jich je 172.

- **Problém:** kraj topí ve stovkách budov a neví, kde plýtvá. Topí se o víkendech, v noci chybí útlum a třídy jsou vydýchané.
- **Řešení:** aplikace spojí otevřená data kraje (25 sad z portálu Data KHK), katastr ČÚZK, počasí a prázdniny
  s měřením ze senzorů. Pravidla najdou problémy a vyčíslí ztrátu v Kč. Kraj pak ví, kam dát peníze nejdřív.
- **Senzory:** skutečná čidla **IQRF** (český výrobce MICRORISC, Jičín) se připojí přes gateway a MQTT.
  Kde čidla zatím nejsou, počítá aplikace poctivě označená modelová data.
- **Výstupy:** mapa s 3D budovou, inspekční panel, PDF audit, AI návrh opatření (Azure OpenAI) a otevřené API.

Vzniká pro **Hackathon Královéhradeckého kraje 2026**. Porota hodnotí stejnou vahou čtyři kritéria:
unikátnost, využití otevřených dat, přínos pro kraj a přínos pro veřejnost. Landing page je první, co porota
a veřejnost uvidí, a musí za pár vteřin říct, co to je a proč na tom záleží.

## 2. Pro koho je landing page

| Návštěvník | Co hledá | Kam ho posíláme |
|---|---|---|
| **Kraj** (radní, investiční a majetkový odbor) | Kde se plýtvá, kolik to stojí, pořadí investic | „Vyzkoušet demo“ → `/login` → role Kraj |
| **Správce budovy** (ředitel školy, domova) | Co je v mé budově špatně a co s tím | „Vyzkoušet demo“ → role Správce |
| **Veřejnost** (rodiče, žáci, senioři a jejich blízcí) | Jak se dýchá ve škole mého dítěte nebo v domově mé mámy | „Najít svou školu“ → `/objekty` (bez přihlášení) |
| **Vývojáři a open data komunita** | Data k dalšímu použití | Otevřené API `/api/open/docs/` |
| **Porota hackathonu** | Rychle pochopit nápad, data a přínos | Celá stránka, demo |

## 3. Klíčová sdělení

1. **Kraj plýtvá teplem v budovách, které nikdo neměří.** SmartAudit ukáže kde a kolik to stojí.
2. **Postaveno na otevřených datech kraje.** 25 sad Data KHK plus ČÚZK, Open-Meteo a MŠMT, každá s konkrétní úlohou.
3. **Skutečná čidla IQRF, připojení za pět minut.** Aplikace sama najde senzory, správce jen vybere místnost.
4. **Užitek pro veřejnost.** Kdokoli zjistí, jak se dýchá ve škole nebo domově, a QR kód na dveřích třídy ho k tomu dovede.
5. **Data se vrací zpět jako otevřená data.** API bez klíče, CC BY 4.0.

**Tón:** česky, věcně, konkrétně, bez marketingových frází („revoluční“, „chytrý“). Čísla místo přívlastků.
Poctivě: co je simulace, se vždy označí jako modelová data.

## 4. Současná struktura a texty

Texty jsou pracovní, designér je může navrhnout lépe. Musí ale zůstat pravdivé.

1. **Hlavička (sticky):** logo HK + „SmartAudit / Královéhradecký kraj“. Navigace: Najdi svou školu · Kvalita vzduchu ·
   Otevřená data. Tlačítko **Přihlásit**, případně **Do aplikace** pro přihlášeného.
2. **Úvodní sekce (tmavě modrá):**
   - Nadtitulek: „Hackathon Královéhradeckého kraje 2026 · otevřená data“.
   - Titulek: **„Kolik kraj protopí v budovách, které nikdo neměří?“**
   - Text: „SmartAudit najde ve školách, domovech a nemocnicích kraje přetápění o víkendu, vydýchané třídy i chybějící
     noční útlum. Stojí na otevřených datech kraje a skutečných čidlech IQRF.“
   - Tlačítka: **Vyzkoušet demo** (primární, červené) a **Najít svou školu** (sekundární).
   - Vpravo snímek mapy s 3D budovou, přes něj telefon s veřejnou kartou.
3. **Živá čísla** (načítají se z otevřeného API, mění se):
   - 172 budov kraje v evidenci
   - 46 se zjištěním z telemetrie
   - 29 otevřených datových sad
   - 1 budova se živým čidlem IQRF
4. **Jak to funguje:** tři kroky.
   - 01 **Otevřená data:** kraj zveřejňuje, co zřizuje, a aplikace k tomu připojí katastr, počty žáků, počasí, prázdniny, památky a dotace.
   - 02 **Čidla IQRF:** gateway posílá teplotu, CO₂ a vlhkost. Aplikace sama najde uzly, kde čidla nejsou, počítá model.
   - 03 **Audit a opatření:** pravidla najdou problémy. Kraj dostane pořadí budov podle ztráty, správce PDF audit a AI návrh opatření.
5. **Funkce:** šest řádků, text a obrázek se střídají zleva doprava.

   | Funkce | Titulek | Obrázek |
   |---|---|---|
   | Mapa kraje | „Celý kraj na jedné mapě“ (okresy, bubliny se složením stavů, 3D budova) | `mapa-okresy.webp` |
   | Inspekční panel | „Co se v budově děje, na jeden pohled“ | `inspekce.webp` |
   | Skutečná čidla IQRF | „Připojení čidla za pět minut“ | `senzory.webp` |
   | Reporty a AI | „Podklad pro rozhodnutí, ne další dashboard“ | `pdf.webp` |
   | Pro veřejnost | „Jak se dýchá ve škole mého dítěte?“ | `verejna-karta.webp` |
   | Otevřená data zpět | „Co SmartAudit zjistí, vrací jako otevřená data“ | `otevrene-api.webp` |

6. **Pro koho:** tři karty (Kraj, Správce budovy, Veřejnost), každá se třemi přínosy.
7. **Závěrečná výzva (tmavě modrá):** „Vyzkoušejte si to“, demo bez registrace a obě tlačítka.
8. **Patička:** popis projektu, data (Data KHK, otevřené API, ČÚZK · Open-Meteo · MŠMT) a projekt (GitHub, hackathon).

## 5. Vizuální identita

**Barvy aplikace:**

| Token | Hodnota | Použití |
|---|---|---|
| `hk-navy` | `#0B192C` | Hlavní tmavá: text, tmavé sekce |
| `hk-blue` | `#1E3E62` | Sekundární modrá, hover |
| `hk-red` | `#D90429` | Červená Hradce Králové: akcenty, obrys vybraného okresu |
| `hk-accent` | `#E63946` | Primární tlačítko |
| slate 50–600 | Tailwind | Pozadí sekcí, sekundární text, rámečky |

**Stavové barvy** (budovy, špendlíky, grafy) se vždy doplňují ikonou nebo textem, nikdy nejsou jen barvou:
- v pořádku: `#10B981`
- varování: `#F59E0B`
- kritické: `#EF4444`

**Škála okresů** (podíl budov se zjištěním, od světlé po tmavou): `#D5E1EE`, `#B3C8E0`, `#8DAACD`, `#6687B5`, `#3F6592`.

**Kvalita vzduchu** (veřejná část): zelená „Čerstvý vzduch“ do 1 000 ppm, oranžová „Čas vyvětrat“ do 1 500 ppm,
červená „Vydýchaný vzduch“ nad 1 500 ppm.

**Písmo:**
- **Hradec** (firemní písmo města, Regular a Bold, soubory `frontend/app/fonts/Hradec-*.woff2`) na všechno.
- Náhradní písmo Geist pro znaky, které Hradec nemá (dolní index v CO₂).
- **Geist Mono** na čísla a kódy (hodnoty měření, kódy objektů).

**Logo:** bílé logo Hradce Králové, na světlém pozadí invertované (`obrazky/hk-logo-white.png`).

**Ikony:** [Lucide](https://lucide.dev), tah 1,75–2,25. Druhy budov mají ikony School, HandHeart, Hospital a Landmark.

**Tvary:** zaoblení 12–24 px (karty `rounded-2xl`), jemné rámečky slate-200, měkké stíny s navy tónem.

## 6. Obrázky

Všechno jsou skutečné snímky aplikace. Snímky 1440 × 900 jsou WebP, telefon má 390 × 844.

| Soubor | Co ukazuje |
|---|---|
| `mapa-3d.webp` | Mapa: vybraná budova ve 3D v barvě stavu, inspekční panel s telemetrií |
| `mapa-okresy.webp` | Celý kraj: okresy podle podílu problémů, bubliny se složením stavů, tečky budov |
| `inspekce.webp` | Muzeum Náchodska: zjištění auditu „Přetápění mimo provoz“ a karta AI analýzy |
| `senzory.webp` | Obrazovka Senzory: IQRF gateway, kanály s živými hodnotami a průběhem, přiřazení do místností |
| `pdf.webp` | Dvě strany PDF auditu (grafy, mapka okresu) |
| `verejna-karta.webp` | Veřejná karta školy „Jak se tu dýchá“ (desktop) |
| `karta-mobil.webp` | Totéž v telefonu |
| `otevrene-api.webp` | Stránka otevřeného API |

Snímky jde přegenerovat nebo nafotit jiné. Stačí říct, co potřebuješ: jinou budovu, detail, tmavý režim, jiný poměr stran.
Na snímky je skript `tools/screenshots/capture.py`.

## 7. Mantinely pro návrh

- **Technika:** Next.js 16 (App Router) + Tailwind CSS v4 a komponenty shadcn/ui (Base UI). Návrh by měl jít
  postavit z běžných bloků, bez těžkých knihoven animací. Obrázky jdou přes `next/image`.
- **Responzivita:** mobil 390 px je stejně důležitý jako desktop 1440 px. Veřejnost přijde hlavně z telefonu přes QR kód.
- **Přístupnost:** kontrast WCAG AA, stav nikdy jen barvou, respektovat `prefers-reduced-motion`.
- **Jazyk:** čeština.
- **Obsahová pravidla:**
  - Na veřejných stránkách se neukazují částky v Kč, ty vidí jen kraj a správci.
  - Simulovaná data se vždy označí jako modelová.
  - Živá čísla se mění, návrh nesmí počítat s pevnou délkou.
- **Konzistence:** stejný styl mají veřejné stránky `/objekty` (hledání a přehled kvality vzduchu) a karta objektu
  `/objekty/{kód}` a její tisková QR cedulka. Návrh landing page by na ně měl jít přenést.
- **Branding kraje:** logo a písmo Hradce Králové používat s respektem, bez úprav tvaru loga.

## 8. Co na současné verzi vylepšit (náměty, ne zadání)

- **Úvodní sekce:** titulek je dlouhý a obrázek je hustý snímek celé aplikace. Hledáme silnější první dojem, jeden jasný vizuál.
- **Živá čísla** působí jako tabulka. Chtělo by to víc příběhu (například „46 budov má problém, kde?“).
- **Šest řádků funkcí** je monotónních (stále text + snímek) a stránka je dlouhá. Zvážit zhuštění nebo jiný rytmus.
- **Chybí** sekce o partnerech a technologiích (IQRF, otevřená data kraje, Azure AI) a o důvěře (zdroje dat, licence, open source).
- **Veřejnost** by si zasloužila výraznější vlastní vstup: vyhledávací pole „Najdi svou školu“ přímo na landing page.
- Na mobilu není navigace, zůstává jen tlačítko Přihlásit.
- Patička je řídká.

## 9. Co od designéra potřebujeme

- Návrh landing page pro desktop (1440 px) a mobil (390 px), ideálně ve Figmě nebo jako statické HTML.
- Stavy tlačítek a odkazů (hover a focus).
- Seznam nových assetů (ilustrace, ikony, případně jiné snímky, které ti nafotíme).
- Volitelně návrh, jak styl přenést na `/objekty` a kartu objektu.
