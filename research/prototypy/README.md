# Prototypy nad otevřenými daty KHK

Čtyři soubory, dva nápady. Bez buildu, bez backendu, bez databáze. Data se stahují **živě
z ArcGIS API** portálu Data KHK při každém načtení stránky.

| Soubor | Co to je |
|---|---|
| [`index.html`](index.html) | **Základ.** Mapa toků: klik na obec → kam jezdí děti, klik na školu → odkud se rekrutují. |
| [`advanced.html`](advanced.html) | **Pokročilá verze.** Oborové pouště, gravitační model, what-if simulátor, AI chat. |
| [`kammuzu.html`](kammuzu.html) | **⭐ Kam se dostanu.** Nástroj pro člověka: kdo jsi → odkud jdeš → dostaneš se tam? |
| [`bariery.html`](bariery.html) | Starší verze téhož nápadu jako **analýza** — ponecháno pro srovnání, viz poznámka níže. |

## Spuštění

Nelze otevřít přes `file://` — prohlížeč by zablokoval volání API.

```bash
python -m http.server 8080
```

Pak <http://localhost:8080/advanced.html>.

---

## Co umí pokročilá verze

### Záložka „Toky"
Choropleth 448 obcí podle podílu dětí odjíždějících mimo vlastní ORP. Klik na obec →
oblouky do škol; klik na školu → spádová oblast. Filtr na obor.

### Záložka „Obory" — oborové pouště
Vyber obor → mapa obarvená podle vzdálenosti obce k nejbližší škole, která ho učí.

- **23 z 87 oborů má v kraji jedinou školu.**
- *Rybářství* a *Zahradnictví*: jediná škola (Kopidlno), průměr **42,4 km**, nejhorší obec **91,1 km**
- *Gymnázium*: 19 škol, průměr 7,8 km
- Podle obce: **Bartošovice v Orlických horách 42,8 km** vs. **Hradec Králové 8,5 km** — pětinásobek

### Záložka „Model" — vlastní gravitační model
Vážená regrese nafitovaná **v prohlížeči** na 5 059 tocích (metoda nejmenších čtverců
přes normální rovnice, běží pod 100 ms):

```
log(podíl) = −6,10 + 0,58·log(velikost školy) − 0,23·log(km) + 0,63·stejné_ORP
vážené R² = 0,301
```

**Škola ve vlastním ORP přitáhne 1,9× víc dětí** než stejně velká a stejně daleká škola
v cizím ORP. Rezidua najdou nečekaně silné vazby — Suchý Důl → Gymnázium Broumov je
**12× silnější**, než model čeká.

Ověřeno: appka i skript `research/analyza/gravity2.py` dávají stejné koeficienty.

### Záložka „What-if" — simulátor
Vyber školu (nebo klikni na její kroužek) → model přerozdělí její žáky mezi zbylé školy
podle predikované přitažlivosti a spočítá dopad.

> Gymnázium Broumov (342 žáků) → **26 zasažených obcí**, průměrná dojížďka
> z 16,6 na **21,4 km (+4,8)**. Nejvíc Martínkovice +25,1 km.

### Chat — AI vrstva
Model **nedostává data v promptu**. Dostane sedm typovaných funkcí a smí volat jen je;
výpočet proběhne lokálně nad otevřenými daty a modelu se vrátí jen čísla. Volání funkce
zároveň **ovládá mapu** — chat není postranní panel, je to ovládání aplikace.

```
toky_z_obce · spadovost_skoly · kde_se_uci_obor · nejblizsi_obor
trend_oboru · simuluj_zruseni · model_kraje
```

**Bez klíče** běží stejné funkce přes lokální rozpoznání záměru (regulární výrazy +
porovnání kmenů kvůli českému skloňování — „Broumova" trefí „Broumov"). Zapnutí Azure
OpenAI je jeden objekt na začátku skriptu:

```js
const AZURE={endpoint:'https://<resource>.openai.azure.com',
             deployment:'<deployment>', apiKey:'<klíč>', apiVersion:'2024-10-21'};
```

Pak dotaz řeší model přes `tools` / `tool_choice:"auto"` — funkce zůstávají stejné.

⚠️ **Klíč v prohlížeči je vidět.** Na demo se to toleruje; v produkci by dotaz šel
přes tenký proxy endpoint.

Tlačítko 🎤 používá `webkitSpeechRecognition` s `lang='cs-CZ'`.

---

## Trik, který drží přenos malý

Sada dojížďky má 12 013 řádků (~4,8 MB). Místo stahování všeho se agreguje **na serveru**:

```
?where=1=1
&groupByFieldsForStatistics=izo,kod_obce_bydliste_studenta
&outStatistics=[{"statisticType":"sum","onStatisticField":"pocet_zaku","outStatisticFieldName":"z"}]
&returnGeometry=false&resultRecordCount=2000&resultOffset=0
```

Sedm takových dotazů → **~830 kB místo 4,8 MB**. Hranice obcí se generalizují přes
`maxAllowableOffset=0.0008` — **11,9 MB → 580 kB**.

---

## Čísla z dat

- **27 042** žáků na SŠ v kraji · **73** škol ve **29** obcích · **87** oborů
- **4 425 žáků přijíždí z jiných krajů** (70 okresů ČR), **285** z ciziny
- Průměrná dojížďka **13,2 km**
- Za 5 školních roků žáků **+15 %** (23 655 → 27 203); **4 obory úplně zanikly**

## Známé vlastnosti dat (v aplikaci se nezametají)

- **900 z 1 342 obcí bydliště leží mimo kraj** → nemáme pro ně hranice, oblouk nenakreslíme.
  Aplikace to u takové obce napíše.
- V datech je **Broumov na Tachovsku** i na Náchodsku — hledání upřednostňuje obce KHK.
- Jedna „škola v kraji" sedí v **Praze 9 – Hloubětíně** a má prázdný okres.
- **R² modelu je 0,30** — není to věštba, je to nástroj na hledání anomálií.
  Slabá strana reziduí je nepoužitelná (artefakt velikosti Hradce), proto se nezobrazuje.
- What-if neřeší kapacity ani to, jestli jinde ten obor vůbec učí. Je to odhad dopadu, ne plán.
- Sada dojížďky **nemá uvedený školní rok ani periodicitu aktualizace**.

## Licence dat

Otevřená data Královéhradeckého kraje, <https://datakhk.cz> — volně k použití
bez licenčních omezení. Použité sady jsou vypsané v `NAPAD_1_ADVANCED.md`.

---

# Kraj bez bariér — [`bariery.html`](bariery.html)

Třetí prototyp, jiný nápad. Přístupnost míst v kraji + detailní pasport Hradce.

## Spuštění

```bash
python Analyza/hk/prevod.py     # jednou — stáhne a převede vrstvy Hradce (~20 s)
python -m http.server 8080      # ve složce Prototyp
```

Pak <http://localhost:8080/bariery.html>.

## Data

| Zdroj | Co | Jak se načítá |
|---|---|---|
| Data KHK, 7 sad | **802 kulturních míst** s polem bezbariérovosti | živě z ArcGIS API |
| Bezbariérový Hradec (CC0), 10 vrstev | **1 021 objektů** | z `data/*.geojson` |
| Hranice obcí KHK | 448 polygonů | živě |

**Proč Hradec neživě:** `ckan.mmhk.cz` neposlá CORS hlavičky, takže prohlížeč ZIP
se shapefilem nenačte. Skript `prevod.py` je stáhne, převede a uloží jako GeoJSON.

## Reprojekce S-JTSK → WGS84

Hradecká data jsou v **S-JTSK Křovák (EPSG:5514)** — souřadnice typu `(-641630, -1042005)`.
`research/analyza/hk/krovak.py` implementuje převod bez externích knihoven:

```
inverzní Křovák → Bessel 1841 φ/λ → geocentrické → Helmert 7 param. → WGS84
```

**Kontrola: všech 1 021 prvků padlo do bounding boxu Hradce** (50,19–50,23 N / 15,78–15,85 E).

Dva omyly, které stojí za zapamatování — oba dávají správnou šířku a nesmyslné délky:

1. `λ₀` Křováku je **24°50′ východně od Greenwiche**, ne od Ferra.
2. EPSG:5514 má **X = záporný easting, Y = záporný northing**, ale Křovák počítá
   s kladným *jihem* a *západem* — osy je potřeba prohodit.

## Čtyři záložky

- **Kraj** — choropleth 448 obcí podle podílu přístupných míst, rozpad po sadách,
  žebříček nejhorších obcí
- **Hradec** — 10 vrstev pasportu; u objektů pro vozíčkáře detail
  (bezbariérové WC, schod u vstupu, šířka dveří, vnitřek)
- **AI rozbor** — 790 poznámek volným textem převedených na strukturu
- **Díry v datech** — co chybí, jako zjištění pro kraj

## Čísla, která z dat vypadla

- **802 míst**: 263 bezbariérových · 92 částečně · 363 nepřístupných · **84 bez údaje**
- Z **434 knihoven** je bezbariérových **112**, u **69 údaj chybí**
- V celém kraji je **5 indukčních smyček** pro nedoslýchavé
- **116 obcí ze 448** nemá ani jedno sledované místo
- Hradec: ze 148 objektů pro vozíčkáře má **70 bezbariérové WC**, **33 schod u vstupu**
- **790 poznámek, 39 126 znaků** českého volného textu

## AI vrstva

Chat volá **6 typovaných funkcí** nad lokálními daty — model nedostává data v promptu
a volání zároveň ovládá mapu. Bez klíče běží lokální rozpoznání záměru
(včetně porovnávání kmenů kvůli českému skloňování — „kolik **knihoven**" trefí sadu „Knihovny").

**Rozbor poznámek** je ta část, kde AI dělá něco, co bez ní nejde: převádí věty jako
„zvonek, schod. Plošina, dveře výtahu do 80 cm" na strukturu. Bez klíče to demonstrují
zjednodušená pravidla, s klíčem to dělá model.

## Co v datech chybí (v aplikaci se to nečístí)

- **Sluchové postižení rozlišuje jediná sada ze sedmi** (Divadla, `typ_bezbarierovosti`).
  Pro neslyšící kraj data prakticky nemá.
- **Hradecká data jsou z 30. 1. 2020** — šest let stará.
- Krajské sady nemají uvedenou periodicitu aktualizace vůbec.
- 499 míst má jen ano/ne bez jakéhokoli detailu.


---

# Kam se dostanu — [`kammuzu.html`](kammuzu.html)

**Přepracováno z `bariery.html`.** Ta verze byla dashboard pro úředníka — choropleth
„podílu přístupných míst“ a záložka „díry v datech“. Člověk na vozíku se ale neptá,
kolik procent knihoven v kraji je bezbariérových. Ptá se **„dostanu se tam?“**

## Jak to funguje

Tři kroky, žádné grafy:

1. **Kdo jsem** — Vozík · Kočárek · Hůl a chodítko · Nevidím · Neslyším
2. **Odkud jdu** — obec nebo tlačítko polohy
3. **Kam chci** — Zdraví · Úřady · Banky · Nákupy · Doprava · Kultura · Školy

Odpovědí je seřazený seznam míst s verdiktem **DOSTANU SE / S POMOCÍ / NEDOSTANU SE /
NEVÍME** a jednou větou proč — ne procenta.

## Stejné místo, jiná odpověď

Tohle je jádro. Verdikt se počítá pro každý profil zvlášť z též sady příznaků
(`SCHODY`, `DO_80CM`, `WC_BEZBAR`, `VNITR_BEZB`, `MAJACEK`, `HODNOCENI`) a z volného textu:

| Profil | Míst celkem | Dostanu se | Nikdo nezjišťoval |
|---|---|---|---|
| Vozík | 1 189 | **306** | 312 |
| Kočárek | 1 189 | **304** | 312 |
| Nevidím | 1 024 | **33** | 800 |
| Neslyším | **38** | 8 | 30 |

Řádek „Neslyším“ je celá prezentace: v celém kraji je **38 míst**, u kterých vůbec
někdo řešil sluchové postižení — z toho 5 indukčních smyček. Není to chyba aplikace,
je to stav dat. A říká se to skrz zkušenost uživatele, ne grafem.

## Co řeší AI

Převod útržkovitého zápisu na větu, kterou člověk použije:

> Data: `„vstup průjezdem k restauraci Evergreen“` + `SCHODY=0`, `WC_BEZBAR=1`
> → **„Bez schodů a s bezbariérovým WC. K dispozici je bezbariérové WC.“**

Bez klíče to skládají pravidla, s Azure OpenAI model — rozumí i formulacím,
na které pravidla nestačí. Originální zápis je v detailu vždy vidět, ať se dá ověřit.

## Proč se člověk vrátí

- **Moje místa** — hvězdička, uloženo v `localStorage`, příště nahoře
- **Profil a obec si pamatuje** — podruhé už nic nenastavujete
- **„Chybí tu údaj — nahlásit kraji“** u každého místa bez informace

## Čím se to liší od `bariery.html`

| | `bariery.html` | `kammuzu.html` |
|---|---|---|
| První obrazovka | choropleth 448 obcí | „Kdo jsem?“ |
| Hlavní číslo | 263 z 802 bezbariérových | „Dostanu se — 0,1 km“ |
| Díry v datech | vlastní záložka pro úřad | „Nikdo to nezjišťoval“ u konkrétního místa |
| Komu slouží | analýze pro kraj | člověku, který někam jde |

`bariery.html` nechávám v repu — ta analýza je použitelná jako **druhá záložka pro kraj**,
ne jako hlavní obrazovka.
