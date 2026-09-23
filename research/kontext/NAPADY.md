# 10 nápadů pro Hackathon KHK 2026

**Verze 2** — přepsáno poté, co jsem projel schémata a reálné hodnoty **všech 165 datových sad**
a dohledal **kompletní seznam ~85 projektů z pěti minulých ročníků** (2020, 2022, 2023, 2024, 2025).

Podklady: [KONTEXT_datove_sady.md](KONTEXT_datove_sady.md) · [KONTEXT_minule_rocniky.md](KONTEXT_minule_rocniky.md) · [KONTEXT_pravidla.md](KONTEXT_pravidla.md)

---

## Dvě věci, které se ukázaly až po hloubkovém průzkumu

**1. Nasycení je horší, než to vypadalo.** Za pět ročníků vzniklo **13 turistických aplikací
a 9 aplikací na výběr školy**. Hledání lékaře 3×, silnice 5×, bezpečnost a krize 5×,
volnočasové aktivity 5×. Jestli jdete do některé z těchto kategorií, porota vás porovnává
s deseti předchozími pokusy a kritérium unikátnosti je předem prohrané.

**2. Porota odměňuje nástroj pro konkrétního člověka, ne dashboard pro úředníka.**
Vítěz 2024 = *Portál seniora* (senioři). Vítěz 2025 = *KompaZ* (průvodce v krizi).
Druhé místo 2025 = výběr bydlení, třetí = „ověř si své místečko". Kritérium
*„využití pro kraj"* v praxi neznamená „hodí se to úřadu do Excelu", ale
**„kraj by to rád nabídl svým lidem"**. Tomu jsem přizpůsobil pořadí.

## Jak číst čísla

`Šance na TOP 3` počítá s ~20 týmy (základ ~15 %) a platí *při dobré exekuci a prezentaci*.
`Obtížnost` je v kontextu **7,5 h kódování** pro tým 3–4 lidí.
**a** unikátnost · **b** využití otevřených dat · **c** přínos pro kraj · **d** přínos pro veřejnost.

| # | Nápad | Šance | Obtížnost | a·b·c·d | Sad |
|---|---|---|---|---|---|
| 1 | Kam mizí naši deváťáci | 42 % | 3/5 | 10·9·10·8 | 8 |
| 2 | Kraj bez bariér | 35 % | 2/5 | 9·9·9·9 | 12 |
| 3 | Co se u nás bude stavět | 30 % | 3/5 | 9·8·9·8 | 14 |
| 4 | Přípojová síť KHK | 27 % | 4/5 | 9·7·8·7 | 5 |
| 5 | Co se utopí | 26 % | 3/5 | 7·9·9·7 | 15 |
| 6 | Index dostupnosti obce | 24 % | 2/5 | 5·10·9·8 | 19 |
| 7 | Silniční dluh | 20 % | 3/5 | 6·7·7·7 | 6 |
| 8 | Obor vs. ekonomika | 17 % | 4/5 | 8·8·9·6 | 8 |
| 9 | Paměť kraje | 16 % | 2/5 | 9·6·8·6 | 7 |
| 10 | Šance na přijetí | 15 % | 2/5 | 3·8·8·10 | 6 |

---

# TOP 3

## 1. „Kam mizí naši deváťáci" — mapa toků dojížďky

**42 % · Obtížnost 3/5 (~6 h) · a:10 b:9 c:10 d:8**

Sada **Dojížďka do středních škol (12 013 řádků)** je jediná v celém katalogu, která obsahuje
**vztah**: pro každou kombinaci *škola × obor × obec trvalého bydliště* říká počet žáků.
Je to matice toků — a **za pět ročníků a ~85 projektů se jí nikdo ani nedotkl**.
Unikátnost 10/10, ověřeno.

**Co v datech reálně je** (spočítáno přes API)

- **27 042 žáků** celkem ve středních školách kraje
- Podle okresu bydliště: Hradec Králové 6 570 · Trutnov 5 224 · Náchod 4 843 ·
  Rychnov n. Kn. 3 002 · Jičín 2 978
- **A přitéká sem 4 425 žáků odjinud** — ze 70 okresů celé ČR: Pardubice 1 104,
  Semily 879, Ústí nad Orlicí 380, Praha 267; **285 s bydlištěm mimo ČR**.
  To je samo o sobě příběh, který nikdo neukázal.
- 73 středních škol ve 29 obcích, 87 různých oborů; žáci jezdí z 1 342 obcí

**Co appka dělá**

- Klikneš na obec (448 polygonů) → vyletí oblouky do škol, kam její děti jezdí,
  tloušťka = počet žáků. Nahoře: „udrží si 34 % dětí doma, 51 % jede do Hradce".
- Klikneš na školu → obrátí se to: spádová oblast, odkud se rekrutují žáci.
- Filtr na obor (`obor_nazev`) → „kdo v kraji učí Ošetřovatelství a odkud se tam jezdí".
- Vrstva **přílivu a odlivu přes hranice kraje**.

**Proč to boduje.** Odbor školství tohle potřebuje pro optimalizaci sítě škol a kapacit
a nemá to nijak vizualizované — kritérium c je 10. Zároveň to není dashboard: rodič deváťáka
se dívá, kam jezdí děti z jeho vesnice, a to je osobní. A tok na mapě je vidět v desáté
sekundě prezentace.

**Data (8 sad):** Dojížďka do SŠ (12 013) · Dojížďka do VOŠ (456) · Hranice obcí (448) ·
Hranice ORP (15) · Školy a školská zařízení (1 626) · Počet žáků dle oborů (1 754) ·
Autobusové zastávky (4 538) · HSOO (448)

**Technicky.** Jeden `fetch` na FeatureServer se stránkováním, `groupBy` v paměti,
**deck.gl `ArcLayer`** nebo Leaflet s SVG oblouky. Žádný backend, žádná databáze.
Spojovací klíč je `kod_obce_bydliste_studenta` → `kod_obce` v hranicích obcí.

⚠️ **Riziko:** nevykreslovat 12 013 oblouků naráz. Nejdřív agregovat na dvojice
*obec → škola*; pak jich jsou stovky. Naplánujte to hned v prvním commitu.

---

## 2. „Kraj bez bariér" — bezbariérovost napříč krajem

**35 % · Obtížnost 2/5 (~5 h) · a:9 b:9 c:9 d:9**

Průzkum schémat odhalil něco, co v popisech sad vůbec není vidět: **7 kulturních sad
má pole `bezbarierovost`** a dohromady pokrývají **802 míst**. U několika je i slovní
poznámka („bezbariérové WC, balkon není bezbariérový", „tři schody u vstupu, pět na toaletu").

| Sada | Míst | ano | částečně | ne | nezjištěno |
|---|---|---|---|---|---|
| Knihovny | 434 | 105 | – | 234 | 61 prázdných |
| Muzea a galerie | 160 | 42 | 58 | 51 | 9 |
| Kulturní domy a kongresová centra | 56 | 30 | 16 | 10 | – |
| Kina | 51 | 36 | 6 | 7 | 2 |
| Hudební kluby a festival parky | 36 | 13 | 1 | 19 | 3 prázdné |
| Divadla a filharmonie | 33 | 25 | – | 8 | – |
| Základní umělecké školy | 32 | 5 | 11 | 15 | 1 prázdné |

Divadla mají navíc `typ_bezbarierovosti` rozlišený na **tělesné / sluchové / zrakové postižení**
— dá se filtrovat podle typu hendikepu, ne jen ano/ne.

A školská data mají `obor_vhodny_pro_ozp` — **které obory středních škol jsou vhodné
pro uchazeče se zdravotním postižením**. To je informace, kterou dnes rodina nedohledá.

**Co appka dělá**

- Vyber typ hendikepu → mapa toho, co je pro tebe reálně dostupné.
- Tvrdá čísla, která nikdo neřekl nahlas: „**z 434 knihoven v kraji je bezbariérových 105**",
  „v okrese X není bezbariérové ani jedno kino".
- Žebříček obcí a ORP podle podílu přístupných míst — kde se na lidi na vozíku zapomnělo.
- Záložka vzdělávání: obory SŠ vhodné pro OZP + které školy je nabízejí.

**Proč to boduje.** Za pět ročníků **nula projektů na přístupnost**. Přitom to přesně sedí
na vkus poroty: konkrétní skupina lidí, konkrétní potřeba — stejný recept, jakým vyhrál
Portál seniora. Kraj má v téhle oblasti povinnost a chybějící nástroj; publikoval by to.
A demo je emočně silné za 20 sekund.

**Data (12 sad):** Knihovny · Muzea a galerie · Kulturní domy · Kina · Hudební kluby ·
Divadla · ZUŠ · Přijímací řízení na SŠ (`obor_vhodny_pro_ozp`) · Poskytovatelé sociálních
služeb · Autobusové zastávky · Hranice obcí · Adresář obcí

⚠️ **Buďte poctiví k dírám v datech.** 61 knihoven má prázdnou hodnotu a 9 muzeí
„nezjištěno". Nezametejte to — udělejte z toho čtvrtou kategorii „**nevíme**" a řekněte
nahlas, že tohle je věc, kterou by kraj měl doplnit. Porota to ocení víc než falešná stoprocentnost.

---

## 3. „Co se u nás bude stavět" — územní plán čitelně

**30 % · Obtížnost 3/5 (~5 h) · a:9 b:8 c:9 d:8**

**21 sad ze Zásad územního rozvoje kraje (ZÚR) a nula projektů za pět ročníků.**
Přitom v poli `Popis` jsou úplně konkrétní věci, které lidi zajímají:
*poldr Skalice · suchá nádrž Mlýnec · transformovna TR 110/35 kV Broumov ·
retenční nádrž Dědina, Mělčany · územní studie jihozápadního obchvatu*.

Dále: **rozvojové oblasti** (Hradec Králové, Náchodsko, Vrchlabí), **rozvojové osy** (5),
**specifické oblasti** (Broumovsko, Krkonoše, Orlické hory, Jičínsko a — nově —
**Specifická oblast sucho**), **územní rezervy** (9).

**Co appka dělá**

- Napiš obec → „co u vás kraj plánuje": plochy a koridory pro veřejně prospěšné stavby,
  co spadá do rozvojové oblasti nebo osy, jestli jste ve specifické oblasti sucho.
- Mapa s filtrem podle účelu: doprava / technická infrastruktura / protipovodňová opatření.
- Vrstva **územních rezerv** — místa, kde je do budoucna něco blokované.

**Proč to boduje.** „Co se u nás bude stavět" je otázka, kterou si lidi kladou pořád
a odpověď je dnes zamčená v PDF územně plánovací dokumentace, kterou nikdo nepřečte.
Přesně definice kritéria *„je to to, co nám chybí"*. Nula konkurence za pět let.

**Data (14 sad):** Plochy a koridory pro VPS — plošné (608), liniové (6), bodové (13) ·
Vymezení rozvojových oblastí (3) · os (5) · specifických oblastí (5) · Územní rezervy (9) ·
Významné oborové záměry (2) · Plochy podmíněné regulačním plánem (1) · Vymezení krajin (25) ·
ÚSES nadregionální a regionální (526) · Hranice obcí · Hranice ORP · HSOO

⚠️ **Hlavní riziko je srozumitelnost, ne technika.** ZÚR je úřední žargon a kódy
(`Ucel`: D / T / U / R, `Typ`: V, `Jev`: A118) nejsou nikde v datech vysvětlené.
Půl hodiny na začátku věnujte tomu, že si ty kódy rozluštíte a **přeložíte do lidštiny** —
v tom je celá hodnota aplikace. Když to neuděláte, je z toho nečitelná mapa polygonů.

---

# Zbytek desítky

## 4. „Přípojová síť KHK" — co se stane, když ujede spoj
**27 % · Obtížnost 4/5 (~7 h) · a:9 b:7 c:8 d:7**

Sada **Seznam zaručených přípojů (3 515 řádků)** říká pro každý spoj, **na který jiný spoj
čeká, na které zastávce a kolik minut** (`vycka_min` 2–30 min, `typ`: autobus / vlak,
plus navazující linka, spoj, dopravce a čas). Je to graf a nikdo ho nenakreslil.

Appka: vyber linku → kaskáda navazujících spojů. „Když tenhle autobus nabere 8 minut,
padne 5 přípojů a 2 vlaky." Plus mapa nejkřehčích uzlů sítě.

⚠️ Dvě věci. Přípoje **nemají geometrii**, jen názvy zastávek (`"Hořice,aut.st."`) — spojení
se sadou zastávek je fuzzy match a může vás stát 2 hodiny ze 7,5. **Řešení: postavte to jako
čistý graf bez mapy**, obtížnost spadne na 3/5. A v roce 2025 běželo *Doprava KHK*
(vyhledávání zastávek a spojů) — musíte jasně říct, že vy neděláte vyhledávač,
ale **analýzu křehkosti sítě**.

## 5. „Co se utopí" — kritická infrastruktura v záplavovém území
**26 % · Obtížnost 3/5 (~5 h) · a:7 b:9 c:9 d:7**

Průnik bodů služeb se čtyřmi úrovněmi záplavových území (Q5 / Q20 / Q100 / Q500 + aktivní zóna,
dohromady 324 polygonů). „V záplavovém území stoleté vody leží X škol, Y domovů pro seniory,
Z ordinací, W zastávek." Posuvník Q5 → Q500, na kterém se seznam ohrožených objektů nafukuje.

Technicky triviální: `turf.booleanPointInPolygon`.

⚠️ Krizové téma bylo v roce 2024 (*IZS*) i 2025 (*KompaZ* — vítěz) a *Kraj Pod Kontrolou*
i *SAFETY MAP* taky. Opakování tématu není diskvalifikace — KompaZ vyhrál rok po IZS —
ale musíte mít jasně jiný úhel: **expozice infrastruktury**, ne průvodce v krizi.
A do appky napište, že polygony jsou dle popisu sady **pouze orientační**.

## 6. „Index dostupnosti obce" — kartička pro 448 obcí
**24 % · Obtížnost 2/5 (~5 h) · a:5 b:10 c:9 d:8**

**Použije 19 sad** — kritérium b je maximálně vytěžené a nedá se zpochybnit. Vzdálenost
k nejbližšímu praktikovi, zubaři, zastávce, škole, sociální službě; pořadí obce mezi 448;
mapa „služebních pouští" prolnutá s HSOO (které má `urov_ohr`: nízká / střední / vysoká míra ohrožení).

Nulové technické riziko — všechno jsou body se souřadnicemi `x`, `y`, stačí haversine.

⚠️ **Tohle je nápad, který v druhé verzi nejvíc klesl.** V roce 2025 skončily na 2. a 3. místě
*Hradec na jednom místě* (výběr bydlení) a *Lifely* („ověř si své místečko"), v roce 2024
běželo *Kde to žije*. Porota tenhle typ „ohodnoť místo" viděla třikrát, z toho dvakrát
na stupních vítězů. Unikátnost jsem srazil z 7 na 5. Berte jen tehdy, když chcete jistotu
bezproblémové exekuce a spolehnete se na kritérium b.

## 7. „Silniční dluh" — které špatné silnice se neopravily
**20 % · Obtížnost 3/5 (~6 h) · a:6 b:7 c:7 d:7**

Průzkum ukázal, že data jsou lepší, než popis sady tvrdí: **Stav povrchu silnic** má
`rok` v rozsahu **2017–2024** (ne jen 2022) na 1 993 různých úsecích, tedy částečnou
časovou řadu, a stupnice `stav_sil` má šest hodnot včetně **„havarijní" a „SUPERhavarijní"**.
Havarijních a superhavarijních záznamů je **1 991**. Křížení s Opravami 2015–2022 přes
`ozn_sil_usek` je čistý atributový join.

⚠️ **Pět předchozích silničních aplikací**, a *RouteStat* (2024) dělal přímo stav povrchu
plus zimní údržbu. Unikátnost jsem srazil na 6. Jediný zbývající neotřelý úhel je
**„co se za deset let neopravilo"** a **zhoršování v čase**, ne „jaký je stav silnic".

## 8. „Obor vs. ekonomika" — mismatch vzdělávání a trhu práce
**17 % · Obtížnost 4/5 (~7 h) · a:8 b:8 c:9 d:6**

Trend počtu žáků v oborech (**5 školních roků 2021/22 → 2025/26**) proti Inovativním firmám
(211, s `kategorie_poctu_zamestancu` a `kategorie_obratu`), Investičním pobídkám
(84, se `sektor` a `rok_rozhodnuti`), Průmyslovým zónám a Klastrům.

⚠️ **V datech není vazba obor ↔ obor podnikání** — museli byste ji namapovat ručně přes
NACE/KZAM a porota se na to zeptá. Navíc *iNVESTRACK KHK* (2024) už investiční pobídky dělal.

## 9. „Paměť kraje" — 25 482 pohřbených
**16 % · Obtížnost 2/5 (~4 h) · a:9 b:6 c:8 d:6**

Sada **Válečné hroby (1 423 míst)** má pole `pocet_hrobu` a `počet_pohrbenych` — v součtu
**2 876 hrobů a 25 482 pohřbených** v jednom kraji. Sada *Válečné hroby ve vlastnictví kraje* (93)
k tomu přidává `historicka_udalost` (1. světová válka, 2. světová válka a **prusko-rakouská
válka**), `autor` sochaře, `vznik` (1866, 1867, 1877…) a `verejne_misto`.

Bitva u Hradce Králové 1866 je historická identita celého regionu a **nula projektů za pět ročníků**.
Appka: mapa v čase, filtr podle konfliktu, „ve vaší obci je pohřbeno X lidí".

⚠️ Silná unikátnost a emoce, ale slabší praktická užitečnost (kritérium d = 6) a jen
7 využitelných sad. Je to nápad na cenu diváků spíš než na první místo.

## 10. „Šance na přijetí" — kompas oborů SŠ
**15 % · Obtížnost 2/5 (~5 h) · a:3 b:8 c:8 d:10**

Data jsou výborná: `pocet_prihl_1_kolo_prij_riz`, `pocet_prijatych_po_1_kole_prij`,
`pocet_nabizenych_mist`, `rocni_skolne` → reálný převis a šance na přijetí u každého oboru;
k tomu trend přes 5 školních roků. **Nejvyšší přínos pro veřejnost z celé desítky.**

⚠️ **A nejnižší unikátnost — devět předchozích školních aplikací.** Doškoly, Školská zařízení
KHK, VyberSkolu, StudyCore, UniGuide, kampak, ŠKOLIFY, Vidimse, Výběr střední školy.
Porota tuhle prezentaci uvidí i letos znovu. Za unikátnost dostanete 2–4 body a čtvrtinu
známky tím ztratíte. Uvádím to jen proto, že veřejná užitečnost je tak vysoká — ale
**samostatně to nedoporučuji**; má to smysl jako druhá záložka nápadu č. 1.

---

## Co jsem po druhém průzkumu vyřadil

- **Turistický itinerář / plánovač výletů** — **13 předchozích aplikací** (cykloroutes, PoCestě,
  e-Výlety, Turisko, V KHK?, Památky na hádky, APA, Královský průvodce, objevkraj.cz,
  ZabrouzdAI, NaRande!, KHK Lokátor, covidtour). Za unikátnost dostanete 1–2 body. Nedělejte to.
- **Atlas péče pro seniory** — *Portál seniora* s tímhle **vyhrál ročník 2024**.
- **Hledání lékaře** — 3 předchozí aplikace.
- **Dotace kraje** — nula konkurence, ale data jsou agregovaná jen na 15 ORP,
  zatímco HSOO je na 448 obcích. Join je hrubý a závěr napadnutelný.

---

# Kdybych vybíral já

**Č. 1 (Kam mizí naši deváťáci).** Po druhém průzkumu ještě silnější než předtím: potvrzeno,
že za pět ročníků a ~85 projektů se téhle sady nikdo nedotkl, a čísla, která z ní vypadla
(27 042 žáků, 4 425 přitékajících z jiných krajů, 285 ze zahraničí), jsou sama o sobě prezentace.

**Prototyp už existuje** — funkční mapa toků je v [Prototyp/](../prototypy/index.html),
včetně triku na agregaci přes API (830 kB místo 4,8 MB). Viz [Prototyp/README.md](../prototypy/README.md).

**Č. 2 (Kraj bez bariér), pokud chcete jít po vkusu poroty.** Konkrétní skupina lidí,
konkrétní potřeba, nula konkurence za pět let a technicky to zvládnete do oběda.
Je to nejbližší analogie k tomu, čím vyhrál Portál seniora — jen v oblasti, kterou
ještě nikdo nesáhl.

**Kombinace, která maximalizuje kritérium b:** základ č. 1 + druhá záložka „kontext obce"
z č. 6. Tím se dostanete na ~20 využitých sad, aniž byste ztratili unikátnost.

## Tři věci, které rozhodnou víc než výběr nápadu

1. **Do 60 sekund musí být na plátně „aha" moment.** Máte 5 minut a porota vidí dvacet
   prezentací. Žádný úvod o tom, co jsou otevřená data — klikněte na obec a ukažte, co se stane.
2. **Vyjmenujte použité sady nahlas a dejte na ně odkaz v aplikaci.** Kritérium b je čtvrtina
   známky a jako jediné se dá koupit prací navíc. Pravidla ten odkaz stejně vyžadují.
3. **V 17:00 musí být na flash disku .pptx/.pdf + kód.** Bez odevzdání tým vůbec nesmí
   prezentovat. Alarm na 16:00, a od té chvíle už jen dokončujte.

---

## Technická poznámka

```
<api_url>/query?where=1%3D1&outFields=*&f=geojson&resultRecordCount=2000&resultOffset=0
```

`&returnDistinctValues=true&outFields=pole` vrátí číselník ·
`&groupByFieldsForStatistics=...&outStatistics=[...]` agreguje na serveru (užitečné,
když nechcete tahat 12 000 řádků do prohlížeče).

Bodové sady mají `x` (lon) a `y` (lat) přímo v atributech.
**Spojovací klíč napříč sadami je `kod_obce`** — sedne na 448 polygonů z Hranic obcí.

Úplný katalog s poli a číselníky: [KONTEXT_datove_sady.md](KONTEXT_datove_sady.md).
Doptat se na konkrétní sadu: `python research/kontext/probe.py "název sady"`.
