# 03 — Designový systém

> Jediný dokument sady, ve kterém jsou povolené konkrétní hodnoty: barvy, rozměry, názvy
> písem a proměnné CSS. Živý vzorník k rekonstrukci 1:1 je v `design_schema.html`.

Zdroj pravdy: `frontend/app/globals.css`, `frontend/app/layout.tsx`,
`frontend/components/ui/*`, `frontend/lib/facility-meta.ts`,
`frontend/lib/district-shading.ts`, `frontend/components/thermal-scrollbar.tsx`.

**Barvy jsou v původní codebase zapsané v `oklch`.** Hexadecimální hodnoty v tabulkách
níže jsou **přesné převody** do sRGB (spočítané, nikoli odhadnuté) a slouží jako
alternativní zápis. Reimplementace může použít kteroukoli formu; `oklch` je doporučená,
protože zachová chování při prolínání barev (`color-mix`).

---

## 1. Barevný systém

### 1.1 Značková paleta Královéhradeckého kraje

Tyto barvy jsou **pevné hexadecimální hodnoty** už v původním zdroji. Používají se
pro značku, nikoli pro plochy rozhraní.

| Token | Hodnota | Použití |
|---|---|---|
| `--color-hk-red` | `#D90429` | Hlavní akcent značky, nejteplejší konec termo škály |
| `--color-hk-red-dark` | `#B00321` | Tmavší varianta pro aktivní stav značkových prvků |
| `--color-hk-navy` | `#0B192C` | Tmavá značková plocha; zároveň barva motivu prohlížeče |
| `--color-hk-blue` | `#1E3E62` | Doplňková tmavě modrá, vlasová linka dráhy posuvníku |
| `--color-hk-accent` | `#E63946` | Světlejší akcent |

### 1.2 Plochy a povrchy

| Token | Světlý motiv | Tmavý motiv | Role |
|---|---|---|---|
| `--background` | `oklch(1 0 0)` → `#FFFFFF` | `oklch(0.145 0 0)` → `#0A0A0A` | Hlavní plátno stránky |
| `--card` | `oklch(1 0 0)` → `#FFFFFF` | `oklch(0.205 0 0)` → `#171717` | Karty a vyvýšené kontejnery |
| `--popover` | `oklch(1 0 0)` → `#FFFFFF` | `oklch(0.205 0 0)` → `#171717` | Rozbalovací nabídky, dialogy |
| `--secondary` | `oklch(0.97 0 0)` → `#F5F5F5` | `oklch(0.269 0 0)` → `#262626` | Druhotná plocha |
| `--muted` | `oklch(0.97 0 0)` → `#F5F5F5` | `oklch(0.269 0 0)` → `#262626` | Ztlumená plocha, stav po najetí myší |
| `--accent` | `oklch(0.97 0 0)` → `#F5F5F5` | `oklch(0.269 0 0)` → `#262626` | Zvýrazněná položka v nabídce |
| `--sidebar` | `oklch(0.985 0 0)` → `#FAFAFA` | `oklch(0.205 0 0)` → `#171717` | Plocha postranní navigace |

### 1.3 Text

| Token | Světlý motiv | Tmavý motiv | Role |
|---|---|---|---|
| `--foreground` | `oklch(0.145 0 0)` → `#0A0A0A` | `oklch(0.985 0 0)` → `#FAFAFA` | Primární text |
| `--card-foreground` | `#0A0A0A` | `#FAFAFA` | Text na kartě |
| `--popover-foreground` | `#0A0A0A` | `#FAFAFA` | Text v nabídce |
| `--muted-foreground` | `oklch(0.556 0 0)` → `#737373` | `oklch(0.708 0 0)` → `#A1A1A1` | Sekundární a ztlumený text, **zástupný text polí** |
| `--primary-foreground` | `oklch(0.985 0 0)` → `#FAFAFA` | `oklch(0.205 0 0)` → `#171717` | Inverzní text na primární ploše |
| `--secondary-foreground` | `#171717` | `#FAFAFA` | Text na druhotné ploše |
| `--accent-foreground` | `#171717` | `#FAFAFA` | Text na zvýrazněné položce |

### 1.4 Interaktivní a akcentové barvy

| Token | Světlý motiv | Tmavý motiv | Role |
|---|---|---|---|
| `--primary` | `oklch(0.205 0 0)` → `#171717` | `oklch(0.922 0 0)` → `#E5E5E5` | Plocha hlavního tlačítka |
| `--ring` | `oklch(0.708 0 0)` → `#A1A1A1` | `oklch(0.556 0 0)` → `#737373` | Prstenec zaměření klávesnicí |
| `--sidebar-primary` | `#171717` | `oklch(0.488 0.243 264.376)` → `#1447E6` | Aktivní položka navigace |

**Stav po najetí myší** se neodvozuje z vlastního tokenu, ale z průhlednosti nebo
míchání barev:

- Hlavní tlačítko: plocha `--primary` na **80 % krytí**.
- Druhotné tlačítko: `color-mix` v prostoru oklch — `--secondary` smíchané s `--foreground` v poměru **95 : 5**.
- Obrysové a průhledné tlačítko: plocha přejde na `--muted`.

**Aktivní stav (stisk):** prvek se posune o **1 pixel dolů** — kromě prvků, které
otevírají nabídku.

### 1.5 Funkční stavové barvy

Stavy auditu používají standardní paletu Tailwindu ve čtveřici odstínů
(500 pro tečku, 800 pro text, 50 pro pozadí, 200 pro ohraničení).
**Pravidlo: stavová barva nikdy nestojí sama — vždy je doplněná ikonou a textovým popiskem.**

| Stav | Tečka (500) | Pozadí (50) | Ohraničení (200) | Text (800) | Ikona | Popisek |
|---|---|---|---|---|---|---|
| Optimální chod | `#10B981` | `#ECFDF5` | `#A7F3D0` | `#065F46` | zaškrtnutí | „Optimální chod" |
| Varování | `#F59E0B` | `#FFFBEB` | `#FDE68A` | `#92400E` | vítr | „Varování" |
| Kritická anomálie | `#EF4444` | `#FEF2F2` | `#FECACA` | `#991B1B` | výstražný trojúhelník | „Kritická anomálie" |

**Destruktivní akce** (mazání) má vlastní token:

| Token | Světlý motiv | Tmavý motiv |
|---|---|---|
| `--destructive` | `oklch(0.577 0.245 27.325)` → `#E7000B` | `oklch(0.704 0.191 22.216)` → `#FF6467` |

Destruktivní tlačítko **není plné** — používá plochu `--destructive` na 10 % krytí
a text v plné barvě. Po najetí myší 20 %.

### 1.6 Ohraničení a oddělovače

| Token | Světlý motiv | Tmavý motiv | Role |
|---|---|---|---|
| `--border` | `oklch(0.922 0 0)` → `#E5E5E5` | `oklch(1 0 0 / 10%)` bílá na 10 % | Ohraničení karet, oddělovače |
| `--input` | `oklch(0.922 0 0)` → `#E5E5E5` | `oklch(1 0 0 / 15%)` bílá na 15 % | Ohraničení formulářových polí |
| `--sidebar-border` | `#E5E5E5` | bílá na 10 % | Oddělovač v navigaci |

### 1.7 Datové palety

#### Neutrální řada pro grafy

| Token | Hodnota | Hex |
|---|---|---|
| `--chart-1` | `oklch(0.87 0 0)` | `#D4D4D4` |
| `--chart-2` | `oklch(0.556 0 0)` | `#737373` |
| `--chart-3` | `oklch(0.439 0 0)` | `#525252` |
| `--chart-4` | `oklch(0.371 0 0)` | `#404040` |
| `--chart-5` | `oklch(0.269 0 0)` | `#262626` |

Řada je **záměrně bez barevného tónu** — barva v rozhraní patří stavu, ne dekoraci grafu.

#### Choroplet okresů — podíl objektů se zjištěním

Sekvenční modrá o pěti třídách. **Stejná škála se používá v aplikaci i v mapě okresů
v PDF reportu** — to je závazné, obě místa musí dávat stejnou barvu.

| Třída | Rozsah | Barva |
|---|---|---|
| 1 | `< 15 %` | `#D5E1EE` |
| 2 | `15–22 %` | `#B3C8E0` |
| 3 | `22–29 %` | `#8DAACD` |
| 4 | `29–36 %` | `#6687B5` |
| 5 | `≥ 36 %` | `#3F6592` |

Hranice tříd: `0.15`, `0.22`, `0.29`, `0.36`.
Nejsvětlejší třída je úmyslně ještě viditelně modrá — okres s málo problémy nesmí
splynout s podkladem mapy.

#### Termo škála posuvníku (`thermal-scrollbar`)

Posuvník se chová jako snímek z termokamery: s posunem uživatele dolů po stránce se dynamicky „ohřívá“. Tvoří vizuální most mezi energetickým auditem a technickým monitoringem.

**Geometrie a provedení dráhy:**
- Šířka dráhy: **6 px**, pevné umístění u pravého okraje plátna.
- Vlasová linka vodicí dráhy: tmavě modrá krajská barva `--color-hk-blue` (`#1E3E62`) s 30% krytím.
- Jezdec (thumb): zaoblení pilulky (`9999px`), výška závislá na poměru zobrazené části k celkové výšce obsahu (minimálně 36 px).

**Pětitřídní přechodová škála s lineárním prolnutím v sRGB:**

| Pozice | Barva v sRGB | Hex | Odstín a sémantický význam |
|---|---|---|---|
| `0.00` (vrchol) | `rgb(59 130 246)` | `#3B82F6` | **Studená modrá:** klidový stav, noční útlum, minimální tepelná ztráta |
| `0.30` | `rgb(139 92 246)` | `#8B5CF6` | **Fialová:** přechodové pásmo probouzení budovy |
| `0.55` | `rgb(236 72 153)` | `#EC4899` | **Purpurová:** aktivní provoz (zabraňuje nečisté kalné hnědé mezi modrou a oranžovou) |
| `0.80` | `rgb(245 158 11)` | `#F59E0B` | **Jantar / oranžová:** varovná zvýšená spotřeba, počínající anomálie |
| `1.00` (spodek) | `rgb(217 4 41)` | `#D90429` | **Červená KHK:** maximální zátěž, kritické anomálie, akutní tepelná ztráta |

**Efekt žhavého termo jádra:**
Jezdec posuvníku nepoužívá plochou jednolitou barvu, ale lineární svislý gradient mezi dvěma dynamickými proměnnými CSS:
- `--scroll-heat`: barva interpolovaná na aktuální pozici posunu (od 0,0 nahoře do 1,0 dole).
- `--scroll-heat-hot`: barva posunutá o offset $+0{,}18$ v termální škále (limitována na $1{,}0$).
Tento svislý přechod vytváří plastickou iluzi tepelného jádra s horkým ohniskem.

---

## 2. Typografie

### 2.1 Rodiny písem

| Proměnná | Rodina | Zdroj | Tloušťky | Styl |
|---|---|---|---|---|
| `--font-hradec` | **Hradec** | lokální soubory WOFF2 | 400, 700 | normal |
| `--font-geist-sans` | **Geist** | Google Fonts, podmnožiny `latin`, `latin-ext` | proměnná | normal |
| `--font-geist-mono` | **Geist Mono** | Google Fonts, tytéž podmnožiny | proměnná | normal |

**Deklarace lokálního písma** (popis, nikoli kód): dvě tváře se stejným názvem rodiny,
`Hradec-Regular.woff2` s tloušťkou 400 a `Hradec-Bold.woff2` s tloušťkou 700,
obě `normal`, strategie zobrazení **`swap`** (text se vykreslí náhradním písmem
a po načtení se vymění — stránka nikdy nečeká na písmo).

### 2.2 Zásobníky

| Role | Zásobník |
|---|---|
| `--font-sans` (výchozí pro celý dokument) | `var(--font-hradec)`, `var(--font-geist-sans)`, `system-ui`, `sans-serif` |
| `--font-heading` | totožný se `--font-sans` — **nadpisy nemají vlastní písmo**, liší se jen velikostí a tloušťkou |
| `--font-mono` | `var(--font-geist-mono)` |

Geist je v zásobníku jako **druhý, ne jako záloha pro výpadek** — doplňuje znaky, které
Hradec nemá (typicky dolní index v zápisu CO₂).

### 2.3 Typografická stupnice

Stupnice vychází z výchozí škály Tailwindu; aplikace nepřidává vlastní velikosti
kromě jedné (`0.8rem` u malého tlačítka).

| Stupeň | Velikost | Řádkování | Tloušťka | Prostrkání | Transformace |
|---|---|---|---|---|---|
| H1 | `1.875rem` / 30 px | `2.25rem` / 36 px | 700 | normální | žádná |
| H2 | `1.5rem` / 24 px | `2rem` / 32 px | 700 | normální | žádná |
| H3 | `1.25rem` / 20 px | `1.75rem` / 28 px | 600 | normální | žádná |
| H4 | `1.125rem` / 18 px | `1.75rem` / 28 px | 600 | normální | žádná |
| H5 | `1rem` / 16 px | `1.5rem` / 24 px | 600 | normální | žádná |
| H6 | `0.875rem` / 14 px | `1.25rem` / 20 px | 600 | normální | žádná |
| Body Regular | `1rem` / 16 px | `1.5rem` / 24 px | 400 | normální | žádná |
| Body Small | `0.875rem` / 14 px | `1.25rem` / 20 px | 400 | normální | žádná |
| Caption | `0.75rem` / 12 px | `1rem` / 16 px | 400 | normální | žádná |
| Kódový blok | `0.875rem` / 14 px | `1.25rem` / 20 px | 400 | normální | žádná, rodina `--font-mono` |
| Text tlačítka | `0.875rem` / 14 px | — | **500** | normální | žádná |
| Text malého tlačítka | `0.8rem` | — | 500 | normální | žádná |
| Text nejmenšího tlačítka | `0.75rem` / 12 px | — | 500 | normální | žádná |

**Zvláštnost formulářových polí:** základní velikost textu v poli je `1rem` (16 px)
a teprve od šířky obrazovky `md` klesá na `0.875rem`. Důvod je praktický — pole s textem
menším než 16 px způsobí na mobilních prohlížečích automatické přiblížení stránky.

Pro celý dokument je zapnuté **vyhlazování písma** (`antialiased`).

---

## 3. Prostorový systém

### 3.1 Škála mezer

Aplikace používá výchozí škálu Tailwindu, kde základní krok je `0.25rem` (4 px).

| Stupeň | rem | px | Typické použití |
|---|---|---|---|
| 0.5 | `0.125rem` | 2 | Jemné odsazení ikony |
| 1 | `0.25rem` | 4 | Mezera mezi ikonou a textem v nejmenším tlačítku |
| 1.5 | `0.375rem` | 6 | Mezera mezi ikonou a textem v tlačítku |
| 2 | `0.5rem` | 8 | Vnitřní odsazení nejmenšího tlačítka |
| 2.5 | `0.625rem` | 10 | **Vodorovné odsazení tlačítka a pole** |
| 3 | `0.75rem` | 12 | Mezery v seznamech |
| 4 | `1rem` | 16 | Vnitřní odsazení karty |
| 6 | `1.5rem` | 24 | Odsazení sekce |
| 8 | `2rem` | 32 | Velké odsazení |

### 3.2 Poloměry zaoblení

Základ je jediná proměnná `--radius` = **`0.625rem` (10 px)**; všechno ostatní se
z ní počítá. To je důležité — změna jednoho čísla posune zaoblení celé aplikace.

| Token | Výpočet | Výsledek |
|---|---|---|
| `--radius-sm` | `--radius × 0.6` | `0.375rem` / 6 px |
| `--radius-md` | `--radius × 0.8` | `0.5rem` / 8 px |
| `--radius-lg` | `--radius` | `0.625rem` / 10 px |
| `--radius-xl` | `--radius × 1.4` | `0.875rem` / 14 px |
| `--radius-2xl` | `--radius × 1.8` | `1.125rem` / 18 px |
| `--radius-3xl` | `--radius × 2.2` | `1.375rem` / 22 px |
| `--radius-4xl` | `--radius × 2.6` | `1.625rem` / 26 px |

| Prvek | Použitý poloměr |
|---|---|
| Tlačítko (výchozí, velké) | `--radius-lg` — 10 px |
| Tlačítko (malé) | menší z `--radius-md` a 12 px → **8 px** |
| Tlačítko (nejmenší) | menší z `--radius-md` a 10 px → **8 px** |
| Formulářové pole | `--radius-lg` — 10 px |
| Karta | `--radius-xl` — 14 px |
| Odznak ve tvaru pilulky | plné zaoblení (`9999px`) |
| Jezdec posuvníku | plné zaoblení (`9999px`) |

### 3.3 Výšky interaktivních prvků

Výšky jsou **sjednocené napříč aplikací** — tlačítko a pole stejné velikosti mají
stejnou výšku, takže se dají postavit vedle sebe.

| Velikost | Výška | Čtvercová varianta (jen ikona) |
|---|---|---|
| `xs` | `1.5rem` / 24 px | 24 × 24 px |
| `sm` | `1.75rem` / 28 px | 28 × 28 px |
| `default` | `2rem` / 32 px | 32 × 32 px |
| `lg` | `2.25rem` / 36 px | 36 × 36 px |

Formulářové pole má jedinou výšku: **32 px** (shodnou s výchozím tlačítkem).

### 3.4 Stíny

Aplikace používá stíny velmi střídmě — hierarchii nese ohraničení a plocha, ne hloubka.

| Stupeň | Hodnota | Použití |
|---|---|---|
| `sm` | `0 1px 2px 0 rgb(0 0 0 / 0.05)` | Ovládací prvky mapy, připsání zdroje |
| `md` | `0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)` | Vyvýšená karta |
| `lg` | `0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)` | Rozbalovací nabídka |
| plovoucí dialog | `0 25px 50px -12px rgb(0 0 0 / 0.25)` | Modální okno |

### 3.5 Hierarchie vrstvení

| Vrstva | Pořadí | Poznámka |
|---|---|---|
| Obsah stránky | `0` | — |
| Mapové překryvy (špendlíky, bubliny) | `10` | Uvnitř plátna mapy |
| Lepivá hlavička, panel nad mapou | `20` | — |
| Rozbalovací nabídky | `50` | — |
| Panel průvodce | `100` | Vysouvá se zespodu |
| Modální okno a jeho pozadí | `200` | — |
| Oznámení (toast) | `300` | Nad vším ostatním |
| Nápověda (tooltip) | `400` | — |

---

## 4. Komponentový designový manuál

### 4.1 Tlačítko

Sdílený základ všech variant: řádkový blok, prvky zarovnané na střed, zaoblení `lg`,
průhledné ohraničení, text `0.875rem` tloušťky 500, **bez zalamování**, plynulý přechod
všech vlastností, výchozí obrys potlačen. Ikona uvnitř nereaguje na ukazatel a má
pevnou velikost **16 × 16 px** (u malých variant 12 nebo 14 px).

| Varianta | Výchozí | Po najetí | Stisk | Zaměření klávesnicí | Zakázané |
|---|---|---|---|---|---|
| **Primární** | plocha `--primary`, text `--primary-foreground` | plocha na 80 % krytí | posun o 1 px dolů | ohraničení `--ring` + prstenec 3 px v `--ring` na 50 % | 50 % krytí, ukazatel vypnutý |
| **Obrysová** | ohraničení `--border`, plocha `--background` | plocha `--muted`, text `--foreground` | posun o 1 px dolů | totéž | totéž |
| **Druhotná** | plocha `--secondary` | `color-mix(--secondary + --foreground 5 %)` | posun o 1 px dolů | totéž | totéž |
| **Průhledná** | bez plochy a ohraničení | plocha `--muted` | posun o 1 px dolů | totéž | totéž |
| **Destruktivní** | plocha `--destructive` na 10 %, text `--destructive` | plocha na 20 % | posun o 1 px dolů | prstenec v `--destructive` na 20 % | totéž |
| **Odkaz** | text `--primary`, bez plochy | **podtržení** s odsazením 4 px | — | totéž | totéž |

**Stav otevřené nabídky** (prvek rozbaluje seznam): obrysová, druhotná a průhledná
varianta zůstanou v barvě jako po najetí myší, aby bylo vidět, co je otevřené.
Takový prvek se **neposouvá** při stisku.

**Chybný stav:** při označení prvku jako neplatného se ohraničení přebarví na
`--destructive` a objeví se prstenec 3 px v `--destructive` na 20 % krytí.

**Stav načítání:** uvnitř tlačítka se zobrazí rotující kroužek na místě ikony,
tlačítko je zakázané.

### 4.2 Formulářové pole

Výchozí: výška 32 px, plná šířka, ohraničení `--input`, **průhledná plocha**,
zaoblení `lg`, vodorovné odsazení 10 px, svislé 4 px, velikost textu 16 px
(od `md` 14 px), plynulý přechod barev.

| Stav | Vzhled |
|---|---|
| Zástupný text | barva `--muted-foreground` |
| Zaměření klávesnicí | ohraničení `--ring`, prstenec 3 px v `--ring` na 50 % |
| Chyba | ohraničení `--destructive`, prstenec 3 px v `--destructive` na 20 % |
| Zakázané | ukazatel vypnutý, kurzor „zakázáno", plocha `--input` na 50 %, krytí 50 % |
| Tmavý motiv | plocha `--input` na 30 %, při zakázání na 80 % |
| Výběr souboru | vnitřní tlačítko 24 px, bez ohraničení a plochy, tloušťka 500 |

### 4.3 Karta

Plocha `--card`, text `--card-foreground`, ohraničení `--border`, zaoblení `xl` (14 px),
vnitřní odsazení 16 px. Volitelná hlavička s nadpisem stupně H5 a patička oddělená
vlasovou linkou v `--border`.

### 4.4 Odznak a pilulka

Plné zaoblení, malé vnitřní odsazení, text `0.75rem`. Stavová varianta nese trojici
barev z tabulky 1.5: pozadí 50, ohraničení 200, text 800 — **a vždy ikonu**.

### 4.5 Postranní navigace

Plocha `--sidebar`, oddělovač `--sidebar-border`. Položka ve výchozím stavu má text
`--sidebar-foreground`; po najetí `--sidebar-accent`; aktivní položka plochu
`--sidebar-primary` s textem `--sidebar-primary-foreground`.

### 4.6 Kostra načítání

Plocha `--muted` s pulzující animací krytí. Rozměr odpovídá obsahu, který nahrazuje.

### 4.7 Mapové prvky

Vzhled knihovny MapLibre je přepsán, aby splynul s aplikací:

| Prvek | Úprava |
|---|---|
| Obsah bubliny | průhledné pozadí, bez stínu, bez vnitřního odsazení, bez zaoblení — vzhled určuje obsah |
| Špička bubliny | **skrytá** |
| Připsání zdroje | plocha `--background`, text `--muted-foreground`, ohraničení `--border`, zaoblení `md`, stín `sm` |
| Odkaz v připsání | `--muted-foreground`, po najetí `--foreground` |
| Tlačítko připsání | průhledné; v tmavém motivu **invertované** |

### 4.8 Horní lišta s KPI kartami (`kpi-row`)

Horní pás metrik je umístěn bezprostředně pod globální navigační lištou nad plátnem mapy. Poskytuje okamžitý manažerský přehled o stavu krajského portfolia.

- **Uspořádání mřížky:** 4 karty vedle sebe (1 sloupec na mobilu `< 640px`, 2 sloupce na tabletu, 4 sloupce na desktopu `lg`).
- **Rozměry a styl karty:** plocha `--card`, ohraničení `--border`, poloměr `--radius-xl` (14 px), vnitřní odsazení 14 px.
- **Sada 4 klíčových KPI:**
  1. **Celkem budov v portfoliu:** celkový počet sledovaných objektů, ikona budovy (`Building2`), sekundární text s počtem zapojených obcí.
  2. **Objekty se zjištěním / anomálií:** celkový počet budov ve stavu Varování nebo Kritická, ikona výstrahy (`AlertTriangle`), stavová tečka jantarová/červená.
  3. **Odhadovaná roční ztráta:** agregovaná finanční ztráta v milionech Kč/rok, ikona plamene (`Flame`). **Striktně podmíněno rolí:** pro roli Veřejnost je celá finanční hodnota skryta a nahrazena pomlčkou.
  4. **Průměrná úspora při nápravě:** procentuální potenciál úspory (např. 18,4 %) a vyčíslení v Kč/rok při implementaci navržených MaR opatření.

### 4.9 Prostorové uspořádání mapy se třemi úrovněmi zoomu

Mapa v MapLibre GL pracuje ve třech odstupňovaných úrovních podrobnosti (LOD):

| Úroveň | Rozsah zoomu | Zobrazený obsah | Kartografické vrstvy |
|---|---|---|---|
| **Úroveň 1 — Kraj** | `zoom < 9` | Celý Královéhradecký kraj, hranice 5 okresů a ORP | Hranice kraje (`hk-border`), pětitřídní choroplet okresů (`#D5E1EE` až `#3F6592`) podle podílu problémových budov, interaktivní souhrnné bubliny okresů (`bubble-pop`) |
| **Úroveň 2 — Okres** | `9 ≤ zoom ≤ 14` | Rozpad na jednotlivé budovy v okrese | Zesvětlené hranice okresu, špendlíky budov (`facility-pin`) s animací dopadu `pin-drop`, barevné kódování dle stavu auditu (zelená `#10B981`, jantarová `#F59E0B`, červená `#EF4444`) |
| **Úroveň 3 — Objekt** | `zoom > 14` | Detailní situace budovy na parcele | 2D půdorys budovy z ČÚZK INSPIRE (`footprint`), **3D extruze budovy** (`selected-building-3d`, vrstva `fill-extrusion`), orientace ke světovým stranám, přilehlé parcely |

### 4.10 Technické řešení 3D extruze v MapLibre GL (`selected-building-3d`)

- **Vykreslení:** vrstva `fill-extrusion` aplikovaná na 2D polygon budovy stažený z `/api/facilities/{id}/footprint`.
- **Parametry zobrazení:**
  * `fill-extrusion-color`: barva odpovídající stavu auditu (zelená `#10B981`, jantarová `#F59E0B`, červená `#EF4444`, neutrální `#1E3E62`),
  * `fill-extrusion-height`: `výška = počet podlaží × 3,2 m`,
  * `fill-extrusion-base`: `0 m`,
  * `fill-extrusion-opacity`: $0{,}88$,
  * `fill-extrusion-vertical-gradient`: `true` (zajišťuje realistický stínový přechod fasád).
- **Poctivost k datům:** Katastr počet podlaží neeviduje. Výchozí výška je vypočtena z heuristiky (užitná plocha / zastavěná plocha) a je v UI VŽDY povinně označena odznakem **„Odhadováno“**.
- **Přepis správcem:** formulář `building-data-form` umožňuje správci zadat přesný počet podlaží a plochu. Po uložení se odznak změní na **„Ověřeno správcem“** a model se v reálném čase přerendruje s novou výškou.

### 4.11 Inspekční panel s 5 záložkami

Plovoucí panel o pevné šířce **440 px** (na mobilu celoobrazovkový výsuvný drawer). Záhlaví obsahuje název budovy, adresu, zřizovatele, stavovou pilulku a 5 přepínacích záložek:
1. **Přehled (`overview-tab`):** celkový stav auditu, zjištěné anomálie, okamžité hodnoty (T, CO₂, RH), roční ztráta v Kč a potenciál úspory v % (skryto pro veřejnost), památkový status.
2. **Budova (`building-tab`):** katastrální atributy, kód RÚIAN, 3D vizualizace, schematický vertikální řez (`facility-section`), editační formulář `building-data-form` pro korekci podlaží a plochy, správa vedlejších částí budovy.
3. **Telemetrie (`telemetry-tab`):** interaktivní časový graf teploty a CO₂ s přepínačem 24h/7d/30d, 24×7 heatmapa denních hodin, živé hodnoty v místnostech, sloupcový graf týdenní spotřeby.
4. **Audit (`audit-tab`):** výsledky AI auditu, identifikace příčiny plýtvání, katalog 8 úsporných opatření s kalkulací investice, roční úspory a návratnosti v letech, generátor úředního dopisu pro zřizovatele.
5. **Smlouvy (`contracts-tab`):** smlouvy organizace z Registru smluv DIA, vyextrahované jednotkové ceny energií v Kč/GJ a Kč/MWh z PDF příloh (PdfPig), odkazy na originální smlouvy.

### 4.12 Schematický vertikální řez budovy (`facility-section`)

Vizuální zobrazení vertikální struktury budovy na záložce Budova:
- **Patra:** horizontální bloky reprezentující jednotlivá podlaží (1. PP, 1. NP, 2. NP, 3. NP, podkroví).
- **Zóny a místnosti:** buňky v rámci patra s označením funkčního typu (učebna, tělocvična, kotelna, kabinet).
- **Senzory IQRF s živými odečty:** každá monitorovaná místnost obsahuje ikonu senzoru s aktuální teplotou (°C) a CO₂ (ppm).
- **Barevná indikace:**
  * Zelená: komfort 20–22 °C, CO₂ < 1 000 ppm,
  * Jantarová: varování (CO₂ 1 000–1 500 ppm, mírný teplotní posun),
  * Červená: anomálie (CO₂ > 1 500 ppm, víkendové přetápění > 22 °C nebo nedotápění).

### 4.13 Tisknutelný QR štítek (`facility-qr-label`)

Šablona pro tisk informačního plakátku na formát A5/A4 na výšku:
- Hlavička s oficiálním logem KHK a textem „SmartAudit KHK — Chytrá škola“,
- Název a přesná adresa školy,
- Velký kontrastní monochromatický QR kód (140 × 140 mm) generovaný knihovnou `qrcode`,
- Srozumitelný návod pro žáky a učitele k naskenování smartphonem pro kontrolu vydýchaného vzduchu,
- Tlačítko okamžitého tisku (`print-button`) volající `window.print()`,
- CSS tiskový styl (`@media print`) skrývající veškeré ovládací a navigační prvky webu.

---

## 5. Pohyb

Šest pojmenovaných animací. Všechny mají dobu trvání v řádu stovek milisekund
a používají časovací křivky s doběhem.

| Název | Trvání | Křivka | Co dělá |
|---|---|---|---|
| `pin-drop` | 420 ms | `cubic-bezier(0.22, 1, 0.36, 1)` | Špendlík objektu „dopadne" na místo — z průhlednosti a posunu −18 px, s přeběhem na 102 % |
| `bubble-pop` | 380 ms | `cubic-bezier(0.34, 1.56, 0.64, 1)` | Bublina okresu se objeví s nafouknutím (křivka má přeběh) |
| `brand-window` | 1,4 s, opakovaně | `ease-in-out` | Okna ve značkovém symbolu se postupně rozsvěcí — stav načítání |
| `guide-in` | 260 ms | `cubic-bezier(0.22, 1, 0.36, 1)` | Panel průvodce se vysune zespodu |
| `guide-step` | 320 ms | `ease-out` | Nový krok v odpovědi průvodce se objeví |
| `bar-grow` | 520 ms | `cubic-bezier(0.22, 1, 0.36, 1)` | Sloupec grafu vyroste od osy (počátek transformace vlevo) |
| `line-draw` | 900 ms | `ease-out` | Čára grafu se „nakreslí" zleva doprava |

**Přístupnost:** při nastavení „omezit pohyb" se **všech sedm animací úplně vypne**.
To je závazné, ne volitelné.

---

## 6. Přepínání motivu

Tmavý motiv se aktivuje třídou `dark` na předkovi (vlastní varianta `dark`).
Tokeny se **předefinují jako celek** — žádná barva nemá jedinou definici uvnitř
tmavého bloku, takže nemůže nastat stav, kdy se text jednoho motivu vykreslí
na ploše druhého.

Barva motivu pro prohlížeč (lišta na mobilu): **`#0B192C`**.

---

## 7. Pravidla, která musí reimplementace dodržet

1. **Zaoblení se počítá z jediné proměnné.** Nikdy nezapisovat poloměr napevno.
2. **Stavová barva nikdy nestojí sama** — vždy ikona + popisek. Aplikace musí být
   použitelná při barvosleposti.
3. **Choroplet okresů musí mít stejnou škálu v rozhraní i v PDF.**
4. **Výšky tlačítek a polí se musí shodovat** v rámci jedné velikosti.
5. **Pole má na mobilu text 16 px**, jinak prohlížeč přiblíží stránku.
6. **Nadpisy nemají vlastní písmo** — stejná rodina jako běžný text.
7. **Vypnutí animací při omezeném pohybu je povinné.**
8. Barvy grafů jsou neutrální; barva nese stav, ne dekoraci.
