# SmartAudit KHK — podklad pro designéra prezentace

Prezentace pro **Hackathon Královéhradeckého kraje 2026** (15. 10. 2026, FIM UHK, aula J1).
Dokument má čtyři části:

1. co jsme zjistili z 35 prezentací z minulých ročníků,
2. zadání (brief),
3. hotový prompt pro designéra nebo AI nástroj (kapitola 7),
4. seznam obrázků.

Obrázky jsou ve složce `obrazky/`, náhledy z vítězných prezentací minulých let v `reference/`.

---

## 1. Pravidla, která prezentaci určují

- **5 minut** na tým v prvním kole, pořadí podle čísla týmu, ukazuje se **živé demo**. Slidy jsou podpora, ne hlavní věc.
- **Odevzdání mezi 17:00 a 17:15** na flash disk pořadatele: kód a **náhled prezentace `.pptx` nebo `.pdf`**.
  Kdo neodevzdá, nesmí prezentovat.
- Do 2. kola jde **5 nejlepších**. Tam o 1.–3. místě rozhoduje porota bez ohledu na body z 1. kola.
- **Cenu publika** volí online všichni v sále: soutěžící, mentoři, porotci i pořadatelé.
- **Kritéria** (každé 0–10 bodů, stejná váha):
  - **a) unikátnost**: je to nové, obohatí to veřejnost, chybí to,
  - **b) otevřená data**: kolik a jaká data KHK a NKOD aplikace využívá,
  - **c) využití pro kraj**: rozvoj kraje, možná publikace na Data KHK,
  - **d) využití pro veřejnost**: užitečnost pro širokou veřejnost.
- **Porota:** lidé z kraje (ekonomika, datový portál, analytici), z národní koordinace otevřených dat, z univerzity
  (databáze, kybernetická bezpečnost), z komunikace veřejné správy a zástupce partnera **MICRORISC / IQRF**.
  Rozumí datům a veřejné správě, technologický výčet je nenadchne.

## 2. Co ukázalo porovnání minulých prezentací

**Zdroj:** repozitáře kraje github.com/krhk za roky 2019, 2020, 2022 a 2023. Stáhli jsme 37 souborů a rozebrali 35 prezentací
(jedna nešla otevřít, jedna je PDF kopie jiné). Z let 2024 a 2025 se prezentace nezveřejnily.

### Jak vypadá typická prezentace

| Ukazatel | Hodnota |
|---|---|
| Počet slidů | medián **6** (2 až 11) |
| Slov na slide | medián **12**, ale třetina prezentací má slidy s 20–30 slovy (odrážky) |
| Slide „Technologie“ (React, Node, logotypy) | **19 z 35** |
| „Děkujeme za pozornost“ | 12 z 35 |
| Aspoň jeden velký snímek obrazovky nebo fotka | 24 z 35 |
| Bez jediného obrázku | 8 z 35 |
| Zmiňuje použitá data | jen **16 z 35** |
| **Konkrétní čísla o dopadu** (Kč, %, počet lidí) | jen **2 z 35** |
| Slide „Co jsme nestihli / Původní plán / Zrada“ | 4 z 35 |

**Typická osnova** (skoro všude stejná): Název a tým → O aplikaci / Myšlenka → Funkce → Technologie → Budoucnost → Děkujeme.
Většinou výchozí šablona PowerPointu, fotky z fotobanky, zeď logotypů technologií a odrážky.

### Co měli vítězové

| Rok a místo | Prezentace | Čím vyčnívala |
|---|---|---|
| 2022 · 1. | **RoadMapper** (portál o stavu silnic) | Nejméně textu (7 slov na slide), 8 velkých obrázků: úvodní fotka jako hook, pak **skutečné snímky aplikace** (mapa silnic, feed hlášení), slide „Spouštím instanci…“ a **živá ukázka MVP**. Na každém slidu navigace (Myšlenka · Výstup · Technologie · Ukázka). |
| 2022 · 2. | Veřejné sbírky, přehledně | Jasné „Co to je“ a hlavně **„Proč něco takového zřizovat“** (přínos pro veřejnost) a vize. Jen text, bez obrázků. |
| 2022 · 3. | Školská zařízení KHK | Krátká (6 slidů), jednotný vizuál, jasný účel. |
| 2023 · 1. | **Královéhradecký Školský Kompas** | Jen **3 slidy**: název, cíl v jedné větě („pomoci studentům najít vysněnou školu, jednoduchý a přehledný vyhledávač“) a **jeden snímek aplikace**. Rozhodlo demo a srozumitelnost. |

Náhledy obrázků z těchto prezentací jsou ve složce `reference/`, jen pro inspiraci, ne k převzetí.

### Závěry pro náš návrh

1. **Vyhrává jasná myšlenka a živé demo, ne počet slidů.** Cíl je 6–8 slidů a demo uprostřed.
2. **Skutečné snímky aplikace, žádné fotobanky.** Vítězové ukazovali vlastní obrazovky.
3. **Čísla, která nikdo jiný neměl.** Jen 2 z 35 prezentací vyčíslily dopad. My máme Kč, počty budov a počty problémů.
4. **Data ukázat, ne vyjmenovat.** Jen polovina týmů vůbec řekla, jaká data použila, a přitom je to celé kritérium b.
5. **Přínos pro veřejnost jako samostatný moment.** U vítězů zazněl vždy jasně („proč to zřizovat“, „pro studenty“).
6. **Vynechat:** zeď technologických logotypů, „Děkujeme za pozornost“, „co jsme nestihli“, localhost odkazy a zapomenuté placeholdery
   (například „popisek“).
7. **Hardware nikdo neměl.** Žádná z 35 prezentací ani žádný projekt ze seznamů ročníků 2024–2025 nepracoval se skutečným čidlem.
   Živé IQRF čidlo v sále tedy porota nejspíš ještě neviděla, a partner IQRF v ní sedí.

## 3. Produkt v kostce (pro designéra)

**SmartAudit KHK** najde plýtvání teplem a vydýchané místnosti v budovách, které zřizuje Královéhradecký kraj.

**Aktuální čísla z aplikace** (na slidy dát až těsně před soutěží, mění se):

| Údaj | Hodnota | Poznámka |
|---|---|---|
| Objekty | **172** | 100 škol, 44 sociálních zařízení, 17 zdravotnických, 11 kulturních |
| Se zjištěním | **46** | 17 kritických, 29 varování |
| Přetápění mimo provoz (víkendy, svátky) | 17 budov | |
| Zvýšené CO₂ (vydýchané třídy) | 15 budov | |
| Nefungující noční útlum | 14 budov | |
| Odhad ztráty | **≈ 5,1 mil. Kč / rok** | **Model** — vždy s poznámkou „odhad z modelových dat, zpřesní se s čidly“ |
| Otevřená data | **25 sad Data KHK + 4 další zdroje** (ČÚZK, Open-Meteo ×2, MŠMT), **6 450 záznamů** | |
| Senzory | 1 521 měřicích bodů (zatím modelových) | Skutečná čidla IQRF: připojení ověřené emulátorem, na místě zapojíme reálné |

**Pro koho:**
- **Kraj:** kde plýtvá, kolik to stojí a pořadí investic.
- **Správce budovy:** co je v budově špatně, PDF audit, AI opatření.
- **Veřejnost:** jak se dýchá ve škole nebo domově, QR kód na dveřích třídy, bez přihlášení.

**Co je unikátní:**
- Energetický audit budov kraje za pět ročníků nikdo nedělal. Nejčastější témata byla turistika (13×) a školy (9×).
- Skutečná čidla IQRF.
- Data se vrací jako otevřené API.

## 4. Doporučená osnova (6–8 slidů, 5 minut)

Časování je orientační. Demo je hlavní část a slidy ho rámují.

| # | Slide | Obsah | Obrázek | Kritérium |
|---|---|---|---|---|
| 1 | **Titul** (0:00–0:15) | „SmartAudit KHK“ a jedna věta: *Kolik kraj protopí v budovách, které nikdo neměří?* Logo HK, jméno týmu malým písmem. | `landing.webp` nebo `mapa-3d.webp` jako tlumené pozadí | — |
| 2 | **Problém s čísly** (0:15–0:45) | 172 budov, 46 s problémem. Tři velká čísla: 17 přetápí o víkendu · 15 vydýchaných · 14 bez nočního útlumu. Pod tím ≈ 5,1 mil. Kč / rok (odhad z modelu). | `mapa-okresy.webp` (kraj s bublinami) | c |
| 3 | **Řešení ve třech krocích** (0:45–1:05) | Otevřená data → čidla IQRF → audit a opatření. Jednoduchý diagram, ne odrážky. | ikony + malé výřezy `zdroje-dat`, `senzory`, `inspekce` | a, b |
| 4 | **Živé demo** (1:05–3:30) | Slide jen jako rámec a záloha: „Kraj → okres → budova ve 3D → živé čidlo → PDF“. Pod tím náhledy pro případ, že demo selže. | `okres-vybrany`, `mapa-3d`, `senzory`, `pdf` | a, c |
| 5 | **Pro veřejnost** (3:30–4:00) | „Jak se dýchá ve škole mého dítěte?“ Karta objektu v telefonu a QR cedulka na dveře. | `karta-mobil.webp`, `qr-cedulka.webp`, `objekty.webp` | d |
| 6 | **Otevřená data tam i zpět** (4:00–4:30) | Vlevo 25 sad KHK a 4 zdroje (skupiny: portfolio, obsazenost, investice, památky, mapy), vpravo „vracíme jako API, CC BY 4.0, bez klíče“. | `zdroje-dat.webp`, `otevrene-api.webp` | b, c |
| 7 | **Co dál + výzva** (4:30–5:00) | Nasadit čidla IQRF do budov kraje, publikovat agregace na Data KHK. QR na aplikaci a GitHub. Místo „Děkujeme“ výzva: *Najděte svou školu*. | QR, `landing.webp` | c, d |

Volitelný 8. slide do zálohy (pro 2. kolo nebo dotazy): čtyři kritéria a čtyři odpovědi. Každé kritérium jednou větou
a důkazem (číslo nebo obrazovka).

## 5. Vizuální zadání

- **Formát:** 16:9, PowerPoint `.pptx` a export do `.pdf`. Musí fungovat na projektoru v aule: tmavé písmo na světlém pozadí,
  nebo naopak, s vysokým kontrastem. Text nejméně 24 pt, velká čísla 80–120 pt.
- **Identita:** stejná jako aplikace a landing page (viz `../landing/KONTEXT_LANDING.md`).
  - hk-navy `#0B192C`, hk-blue `#1E3E62`, červená Hradce Králové `#D90429`, akcent `#E63946`,
  - stavové barvy: zelená `#10B981`, oranžová `#F59E0B`, červená `#EF4444`,
  - písmo **Hradec** (Regular a Bold, soubory `frontend/app/fonts/`), čísla v Geist Mono,
  - logo HK `obrazky/hk-logo-white.png`.
- **Styl:**
  - Hodně vzduchu, jedna myšlenka na slide, nejvýš ~15 slov mimo čísla.
  - Skutečné snímky v jednoduchém rámu prohlížeče nebo telefonu.
  - Tmavé titulní a přechodové slidy, světlé obsahové.
- **Nepoužívat:**
  - fotky z fotobank, zeď logotypů technologií, výchozí šablony PowerPointu,
  - odrážky přes celý slide,
  - „Děkujeme za pozornost“,
  - částky v Kč bez poznámky, že jde o model.
- **Poctivost:** všude, kde je simulace, napsat „modelová data“. Porota ocení upřímnost, přikrášlení odhalí.
- **Písma:** vložit do `.pptx`, případně připravit PDF verzi, protože na počítači v aule písmo Hradec nebude.
- **Animace:** jen jednoduché přechody (fade). Prezentace musí fungovat i bez nich, protože PDF náhled je povinný.

## 6. Obrázky (`obrazky/`)

Všechno jsou skutečné snímky aplikace, 1440 × 900 WebP, telefon 390 × 844. Před soutěží je nafotíme znovu s čerstvými čísly
(`tools/screenshots/capture.py`). Stačí říct, co chybí.

| Soubor | Co ukazuje | Hodí se na slide |
|---|---|---|
| `landing.webp` | Úvod landing page (titulek, CTA, snímek aplikace) | 1, 7 |
| `mapa-okresy.webp` | Celý kraj: okresy podle podílu problémů, bubliny se složením stavů | 2 |
| `okres-vybrany.webp` | Vybraný okres HK s červeným obrysem, špendlíky budov | 4 |
| `mapa-3d.webp` | Budova ve 3D v barvě stavu a inspekční panel s živým čidlem | 4 |
| `inspekce.webp` | Muzeum Náchodska: zjištění „Přetápění mimo provoz“, AI analýza | 3, 4 |
| `senzory.webp` | IQRF gateway: kanály s živými hodnotami, přiřazení do místností | 3, 4 |
| `pdf.webp` | Dvě strany PDF auditu (grafy, mapka okresu) | 4 |
| `portfolio.webp` | Tabulka budov podle odhadované ztráty | záloha |
| `anomalie.webp` | Seznam zjištění seřazený podle dopadu v Kč | záloha |
| `verejna-karta.webp` | Veřejná karta školy „Jak se tu dýchá“ | 5 |
| `karta-mobil.webp` | Totéž v telefonu | 5 |
| `qr-cedulka.webp` | Tisková cedulka s QR kódem na dveře třídy | 5 |
| `objekty.webp` | Veřejné hledání „Najdi svou školu“ a přehled kvality vzduchu | 5 |
| `zdroje-dat.webp` | Obrazovka Zdroje dat: 25 sad Data KHK s počty záznamů | 6 |
| `otevrene-api.webp` | Otevřené API: JSON, GeoJSON, CSV | 6 |
| `hk-logo-white.png` | Logo Hradce Králové (bílé, na světlé pozadí invertovat) | všude |

## 7. Prompt pro designéra (zkopírovat celý)

> **Zadání:** Navrhni prezentaci (16:9, PowerPoint `.pptx` a export `.pdf`, 7 slidů a 1 záložní) pro aplikaci **SmartAudit KHK**
> na Hackathon Královéhradeckého kraje 2026. Tým má **5 minut**, z toho asi 2,5 minuty živé demo. Slidy demo rámují, nenahrazují ho.
>
> **Co aplikace dělá:** najde plýtvání teplem a vydýchané místnosti ve 172 budovách, které zřizuje kraj (školy, domovy pro seniory,
> nemocnice, kultura). Stojí na 25 otevřených datových sadách kraje (Data KHK) a 4 dalších zdrojích (ČÚZK, Open-Meteo, MŠMT)
> a na skutečných čidlech IQRF. Kraj vidí, kde plýtvá a kolik to stojí, správce budovy dostane PDF audit a AI návrh opatření
> a veřejnost bez přihlášení zjistí, jak se dýchá ve škole svého dítěte (QR kód na dveřích třídy).
> Data se vrací jako otevřené API.
>
> **Klíčová čísla:** 172 budov · 46 s problémem (17 přetápí o víkendu, 15 vydýchaných, 14 bez nočního útlumu) ·
> odhad ≈ 5,1 mil. Kč / rok (vždy s poznámkou „odhad z modelových dat“) · 25 + 4 datové sady, 6 450 záznamů.
>
> **Osnova:**
> 1. Titul s otázkou „Kolik kraj protopí v budovách, které nikdo neměří?“
> 2. Problém ve třech velkých číslech
> 3. Řešení ve třech krocích (data → čidla IQRF → audit)
> 4. Rámec živého dema se záložními snímky (kraj → okres → budova ve 3D → živé čidlo → PDF)
> 5. Pro veřejnost: telefon s kartou a QR cedulka
> 6. Otevřená data tam i zpět
> 7. Co dál a výzva s QR kódem
> 8. Záloha: 4 kritéria poroty → 4 odpovědi
>
> **Porota** hodnotí unikátnost, využití otevřených dat, přínos pro kraj a pro veřejnost. Rozumí datům a veřejné správě,
> je v ní i zástupce IQRF. Minulé vítězné prezentace byly krátké (3–9 slidů), s minimem textu, skutečnými snímky aplikace
> a živým demem. Typická prezentace konkurence: odrážky, zeď logotypů technologií, fotobanky, „Děkujeme za pozornost“.
> Vyhni se tomu.
>
> **Vizuál:**
> - identita Hradce Králové: navy `#0B192C`, modrá `#1E3E62`, červená `#D90429`, akcent `#E63946`, stavové barvy zelená/oranžová/červená,
> - písmo Hradec, čísla v Geist Mono, logo HK,
> - tmavé titulní a přechodové slidy, světlé obsahové,
> - jedna myšlenka na slide, nejvýš ~15 slov mimo čísla, velká čísla,
> - skutečné snímky aplikace v jednoduchém rámu prohlížeče nebo telefonu (přiložené ve složce `obrazky/`),
> - vysoký kontrast pro projektor, text nejméně 24 pt,
> - písma vložená do `.pptx`,
> - simulovaná data vždy označit jako modelová.
>
> **Výstup:** `.pptx` s vloženými písmy, `.pdf` export a zdrojové podklady (případně Figma).
> Dodat i variantu bez animací, protože PDF náhled se odevzdává na flash disk.

## 8. Přílohy

- `reference/`: náhledy obrázků z prezentací vítězů 2022 a 2023 a dvou dalších (RoadMapper, Školský kompas, Školská zařízení,
  HK Donates, CheckMyRoute). Jen pro představu úrovně.
- `../landing/`: brief a HTML landing page (stejný vizuální jazyk).
