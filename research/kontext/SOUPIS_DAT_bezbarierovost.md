# Co přesně bude v datech — nápad „Kraj bez bariér"

Vše ověřeno stažením a rozbalením 8.–9. 9. 2026. Skripty: `research/analyza/hk/`.

**Souhrn: 1 823 záznamů o přístupnosti · 790 poznámek volným textem (39 126 znaků)
· 19 datových sad · vše otevřená data (kraj + CC0 město)**

---

## A. Kraj — bezbariérovost kulturních míst (7 sad, 802 míst)

Živé ArcGIS API, CORS otevřené, stáhne se za sekundu.

| Sada | Míst | ano | částečně | ne | nezjištěno | Poznámky |
|---|---|---|---|---|---|---|
| Knihovny | 434 | 112 | – | 253 | 69 prázdných | – |
| Muzea a galerie | 160 | 42 | 58 | 51 | 9 | 139 (6 782 zn.) |
| Kulturní domy a kongresová centra | 56 | 30 | 16 | 10 | – | 44 (2 590 zn.) |
| Kina | 51 | 36 | 6 | 7 | 2 | 51 (1 889 zn.) |
| Hudební kluby a festival parky | 36 | 13 | 1 | 19 | 3 prázdné | 12 (709 zn.) |
| Divadla a filharmonie | 33 | 25 | – | 8 | – | 25 (683 zn.) |
| Základní umělecké školy | 32 | 5 | 11 | 15 | 1 prázdné | 32 (2 271 zn.) |
| **celkem** | **802** | **263** | **92** | **363** | **84** | **303 (14 924 zn.)** |

**Divadla** mají navíc pole `typ_bezbarierovosti` rozlišené na
*tělesné postižení · sluchové postižení · zrakové postižení* — dá se filtrovat podle hendikepu.

⚠️ **Knihovny nemají poznámky vůbec** a 69 z nich má prázdnou hodnotu — to je největší díra.

> **Oprava proti první verzi:** původně jsem u knihoven uváděl 105/234/61. To bylo z dotazu
> omezeného na 400 záznamů, ale knihoven je 434. Správná čísla jsou **112 / 253 / 69**
> a celkový součet **263 / 92 / 363 / 84**. Aplikace stránkuje, takže počítá správně.

---

## B. Město Hradec Králové — „Bezbariérový Hradec" (10 vrstev, 1 021 objektů)

Licence **CC0**, portál `ckan.mmhk.cz`. Formát **zip:shp**.

| Vrstva | Záznamů | Geometrie | Co je uvnitř |
|---|---|---|---|
| **Objekty – vozíčky** | 148 | bod | `VSTUP` (0–3) · `DO_80CM` / `NAD_80CM` (šířka dveří) · `VNITR_BEZB` · `SCHODY` · `WC_BEZBAR` · `BANKOMAT` · `HODNOCENI` (1–3) |
| **Objekty – nevidomí** | 168 | bod | `MAJACEK` (hlasový majáček ano/ne) · `INSTITUCE` · `HODNOCENI` · 2 pole poznámek |
| **Přechody – nevidomí** | 177 | bod | `VIDITELNOST` · `HMATNE_PRV` · `SVETEL_SIG` · `AKUST_SIG` · `HODNOCENI` |
| **Přechody – vozíčky** | 141 | bod | `PROBLEM` (volný text) · `HODNOCENI` |
| **Chodníky – nevidomí** | 278 | linie | `HODNOCENI` · `PRIR_VOD_L` / `UMEL_VOD_L` (vodicí linie) · `DELKA_TRAS` |
| **Zastávky – nevidomí** | 55 | bod | `SIGNAL_PAS` · `VIZUAL_PRV` · `BEZBARIERO` (0–3) · `ULICE` · `SMER` |
| **Zastávky – vozíčky** | 12 | bod | `ZASTAVKA` · `PROBLEM` · `HODNOCENI` |
| **Trasy** | 36 | linie | `TYP`: HLAVNI / VEDLEJSI — doporučené bezbariérové trasy městem |
| **Indukční smyčky** | 5 | bod | Bio Centrál · Klicperovo divadlo · Krajský úřad KHK · UHK · Česká spořitelna |
| Úseky – nevidomí | 1 | linie | (skoro prázdná vrstva) |
| **celkem** | **1 021** | | |

### ⚠️ Dva háčky, které musíte vyřešit hned ráno

1. **Souřadnice jsou v S-JTSK Krovák**, ne WGS84 — `.prj` hlásí `S-JTSK_Krovak_East_North`
   a body vypadají takhle: `(-641630.76, -1042005.84)`. ✅ **Vyřešeno** — viz „Prototyp" níže.
2. **`ckan.mmhk.cz` neposlá CORS hlavičky** — ověřeno, ZIP nejde stáhnout přímo z prohlížeče.
   Řešením je převod skriptem předem (běží 20 s), nebo vlastní proxy.
   *Pozor: převod dat předem je příprava, ne psaní aplikace — ale ověřte si u organizátorů,
   jestli je to v pořádku (čl. 4.2 mluví o práci výlučně v místě konání).*

### Datum aktualizace

`metadata_modified` u většiny sad je **2020-01-30**. Data jsou šest let stará. Řekněte to
nahlas — u přístupnosti je to zásadní a poctivost tady boduje víc než předstírání aktuálnosti.

---

## C. Volný text — materiál pro AI vrstvu

**790 poznámek, 39 126 znaků češtiny.** Tohle je ta část, kde AI dělá něco, co bez ní nejde.

| Zdroj | Poznámek | Znaků |
|---|---|---|
| Přechody – nevidomí (HK) | 175 | 10 761 |
| Objekty – nevidomí (HK) | 165 | 6 284 |
| Muzea a galerie (kraj) | 139 | 6 782 |
| Zastávky – nevidomí (HK) | 57 | 4 974 |
| Objekty – vozíčky (HK) | 73 | 1 596 |
| Kulturní domy (kraj) | 44 | 2 590 |
| ZUŠ (kraj) | 32 | 2 271 |
| Kina (kraj) | 51 | 1 889 |
| ostatní | 51 | 2 300 |
| **celkem** | **790** | **39 126** |

Ukázky toho, jak ten text vypadá:

> „zvonek, schod. Plošina, dveře výtahu do 80 cm"
> „automat.dveře, nájezd.rampa, WC v lůžk.části"
> „Signální pás je po 1 m ukončen (nesprávné ukončení, není vedený k přirozené vodící linii)."
> „Do areálu vstupujeme vstupní branou, v areálu absence bezbariérových úprav pro zrakově
> postižené, samostatný pohyb nedoporučujeme."
> „bezbariérové WC, balkon není bezbariérový"

Je to **zkratkovité, nekonzistentní a nestrukturované** — přesně to, na co je jazykový model.
Úloha pro AI: převést každou poznámku na strukturu

```json
{"vstup":"zvonek|rampa|automatické dveře|schod",
 "vytah":true, "sirka_dveri_cm":80, "wc":false,
 "varovani":["samostatný pohyb nedoporučen"],
 "jistota":"vysoká|nízká"}
```

Výsledek je **nová datová vrstva, která dnes neexistuje** — a to je zároveň odpověď
na kritérium a (unikátnost) i c (kraj by ji chtěl zpátky).

---

## D. Doplňkové sady (další body ke kritériu b)

| Sada | Zdroj | K čemu |
|---|---|---|
| Poskytovatelé sociálních služeb (461) | kraj | kde bydlí cílová skupina |
| Autobusové zastávky VDKHK (4 538) | kraj | dostupnost dopravou |
| Hranice obcí (448) · Adresář obcí (448) | kraj | mapový podklad, žebříček obcí |
| Přijímací řízení na SŠ (352) — `obor_vhodny_pro_ozp` | kraj | obory vhodné pro uchazeče se ZP |
| Sociální služby a zdravotnictví | HK, CC0 | body zařízení ve městě |
| Pasport chodníků · Pasport komunikací | HK, CC0 | podklad pro trasování |
| Školství – školy | HK, CC0 | školy ve městě |

**Celkem tedy 19 sad** ve verzi bez živých dat.

---

## E. Živá data — jen pokud to posvětí mentoři

| Zdroj | Co vrací | Stav |
|---|---|---|
| `mapa.fcen.cz/api/vehicle_data` | 49 vozů: `latitude`, `longitude`, `delay`, **`number_of_passengers`**, `line`, `direction_name` | ⚠️ bez CORS, není otevřená data |
| `mapa.fcen.cz/api/platform_list` | 428 zastávek | ⚠️ totéž |
| kamery `doprava.hradeckralove.cz` | 3 lokality, snímek 1×/min | ⚠️ bez CORS, mimo katalog |

`number_of_passengers` je pro vozíčkáře reálně užitečný údaj — plný trolejbus znamená,
že se dovnitř nedostanete. Ale bez potvrzení podmínek užití na tom nestavte jádro.

---

## Co z toho půjde postavit

1. **Mapa přístupnosti** — 802 kulturních míst kraje + 1 021 objektů Hradce, filtr podle
   typu hendikepu (vozík / zrak / sluch)
2. **Tvrdá čísla, která nikdo neřekl** — „z 434 knihoven je bezbariérových 112",
   „v kraji je 5 indukčních smyček"
3. **Žebříček obcí a ORP** podle podílu přístupných míst
4. **AI vrstva** — 790 poznámek převedených na strukturu + chat nad tím
5. **Kategorie „nevíme"** — 84 míst kraje bez údaje, to je zjištění pro kraj samotný
6. *(volitelně)* bezbariérové trasy Hradcem z vrstvy `Trasy` (36 linií)


---

## Prototyp — postaveno a ověřeno

[`research/prototypy/bariery.html`](../prototypy/bariery.html) — čtyři záložky (Kraj · Hradec ·
AI rozbor · Díry v datech) + chat volající funkce nad reálnými daty.

**Reprojekce vyřešena.** `research/analyza/hk/krovak.py` implementuje inverzní Křovák
(EPSG:5514 → Bessel 1841 → Helmert 7 parametrů → WGS84) bez externích knihoven.
`research/analyza/hk/prevod.py` stáhne všech 10 vrstev, převede je a uloží jako GeoJSON
do `research/prototypy/data/`. **Kontrola: všech 1 021 prvků padlo do bounding boxu Hradce.**

Dva omyly, na které jsem cestou narazil a které stojí za zapamatování:

1. `λ₀` Křováku je **24°50′ východně od Greenwiche**, ne od Ferra (to mi posunulo
   zeměpisnou délku o 7°).
2. EPSG:5514 má osy **X = záporný easting, Y = záporný northing**, ale Křovák počítá
   s kladným *jihem* a *západem* — je potřeba je prohodit, jinak vyjde správná šířka
   a nesmyslná délka.
