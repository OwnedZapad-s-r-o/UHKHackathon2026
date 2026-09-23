# Školní kompas KHK — pokročilá verze nápadu č. 1 s AI

Rozšíření „Kam mizí naši deváťáci" o skutečnou analytiku a hlubokou integraci AI.
Všechna čísla níže jsou **spočítaná z reálných dat**, ne odhady — skripty jsou v `research/analyza/`.

---

## Nejdřív jedna věcná poznámka k AI

Pravidla, čl. 5.3, doslova: *„AI budou moci využít, ale také nemusejí, to už záleží na nich…
avšak **není podmínkou** AI pro návrh aplikace využít."* A ve čtyřech hodnoticích kritériích
(čl. 6.1) **AI není**. Hodnotí se unikátnost, otevřená data, přínos pro kraj, přínos pro veřejnost.

Neznamená to, že AI nemá smysl — Microsoft je partner, v roce 2025 se rozdávaly přístupy
na „vibe coding" a porotě se to zjevně líbí. **Znamená to jen, že AI musí bodovat skrz ty
čtyři kritéria, ne vedle nich.** Celý návrh níže je proto postavený tak, že každá AI vrstva
zvedá některé z těch čtyř kritérií:

| AI vrstva | Které kritérium zvedá a jak |
|---|---|
| Chatbot s voláním funkcí | **d** — data zpřístupní čtrnáctiletému, ne jen analytikovi |
| What-if simulátor | **c** — z appky se stává plánovací nástroj pro Odbor školství |
| Sémantické hledání oborů | **d** — „chci dělat něco s počítači" místo znalosti názvů oborů |
| AI komentář nad čísly | **a** — appka neukazuje graf, ale říká, co z něj plyne |
| Hlasový vstup | **d** — použitelné i pro rodiče, co nechtějí psát |

---

## Co v datech reálně je (ověřeno)

### Gravitační model dojížďky

Nafitovaný vážený model na 5 059 tocích mezi obcemi KHK a 73 školami:

```
log(podíl) = −6,10 + 0,58·log(velikost školy) − 0,23·log(km) + 0,63·stejné_ORP
vážené R² = 0,301        (n = 5 059 toků)
```

Dvě čísla, která z toho vypadla a stojí za celou prezentaci:

- **Stejné ORP = 1,9× vyšší podíl** při stejné vzdálenosti i velikosti školy.
  Správní hranice reálně tvarují, kam děti jezdí — nejen vzdálenost.
- **Krajský vážený průměr dojížďky je 13,2 km.** Heřmánkovice 34,6 km, Hradec Králové 3,4 km.
- Model i aplikace dávají stejná čísla — ověřeno proti sobě (`research/analyza/gravity2.py` vs `research/prototypy/advanced.html`).

Rezidua modelu odhalují **nečekaně silné vazby** (jen toky ≥ 10 žáků):

| Obec → škola | žáků | km | vs. model |
|---|---|---|---|
| Suchý Důl → Gymnázium Broumov | 11 | 6,5 | **12,0×** |
| Martínkovice → Gymnázium Broumov | 13 | 4,5 | 7,0× |
| Chlumec nad Cidlinou → Gymnázium a SOŠ | 61 | 10,3 | 6,6× |
| Špindlerův Mlýn → Krkonošské gymnázium | 25 | 11,2 | 6,5× |
| Police nad Metují → Gymnázium Broumov | 43 | 7,8 | 5,9× |

Broumovsko je uzavřená kapsa se silnou lokální vazbou — a to model najde sám.

⚠️ **Poctivě:** opačná strana reziduí (nečekaně slabé vazby) je nepoužitelná — vychází
z ní samý Hradec Králové, protože má 3 599 žáků a jeho *podíl* do jedné školy je
mechanicky malý. V appce ukazujte jen silné vazby a řekněte proč.

### Oborové pouště — nejsilnější věc pro veřejnost

- **23 z 87 oborů učí jen jediná škola v celém kraji.**
- Průměrná vzdálenost obce k nejbližší škole s oborem:

| Obor | průměr | nejhorší obec | škol v kraji |
|---|---|---|---|
| Operátor skladování | 43,9 km | 87,7 km | 1 |
| Zahradnictví, Rybářství | 42,4 km | 91,1 km | 1 |
| Lesnictví, Ekologie a ŽP | 39,1 km | 73,0 km | 1 |
| Gastronomie | 34,5 km | 62,8 km | 1 |
| … | | | |
| Informační technologie | 11,4 km | 30,7 km | 11 |
| Gymnázium | 7,8 km | 18,7 km | 19 |

- Podle obce: **Bartošovice v Orlických horách 42,8 km** průměrně ke všem oborům,
  **Hradec Králové 8,5 km**. Pětinásobný rozdíl podle toho, kde se dítě narodí.

### Časová řada — 5 školních roků (2021/22 → 2025/26)

- Celkem žáků SŠ: **23 655 → 27 203 (+15 %)**
- Nejvíc mizí: Provozní technika −49 %, Umělecký kovář −27 %, Mechanik seřizovač −20 %,
  Obráběč kovů −17 %
- Nejvíc roste: Zahradnictví +123 %, Zahradnické práce +112 %, Autolakýrník +73 %,
  Ekonomika a podnikání +67 %
- **Úplně zanikly 4 obory:** Bezpečnostní služby, Chemik, Jemný mechanik, Opravářské práce

### Shluky obcí podle profilu dojížďky

K-means na 223 obcích × 25 hlavních škol našel 6 přirozených spádových oblastí.
Klíčové zjištění: **shluky nekopírují ORP.** Největší shluk (85 obcí) míchá
Trutnov (24), Jičín (16) a Vrchlabí (13). Reálná spádovost a správní členění se rozcházejí.

### Co naopak nevyšlo

Napojení na autobusové zastávky jsem zkoušel jako vrstvu „je ta dojížďka vůbec obsloužitelná" —
**nemá to pointu**: bez jediné zastávky VDKHK jsou jen **4 obce ze 448** (Kostelec, Lochenice,
Dohalice, Brada-Rybníček) a bydlí v nich dohromady 62 žáků SŠ. Nestavte na tom feature.

---

## Architektura AI

### 1. Chatbot, který volá funkce (ne halucinuje)

**Tohle je jádro a zároveň odpověď na otázku, kterou porota položí:** „jak víte,
že si to nevymýšlí?"

Model **nedostane data v promptu**. Dostane sadu typovaných funkcí a smí volat jen je:

```js
const TOOLS = [
  {name:'toky_z_obce',        params:{obec:'string'}},
  {name:'spadovost_skoly',    params:{skola:'string'}},
  {name:'kde_se_uci_obor',    params:{obor:'string'}},
  {name:'nejblizsi_obor',     params:{obec:'string', obor:'string'}},
  {name:'trend_oboru',        params:{obor:'string'}},
  {name:'porovnej_obce',      params:{a:'string', b:'string'}},
  {name:'simuluj_zruseni',    params:{skola:'string', obor:'string?'}}
];
```

Tok: dotaz → model vybere funkci a argumenty → **appka ji spočítá nad lokálními daty** →
čísla se vrátí modelu → model je jen naformuluje česky. Model tedy nikdy nevidí nic,
co by si mohl vymyslet.

**Druhá polovina triku:** volání funkce zároveň **ovládá mapu**. Zeptáš se na Broumov,
mapa na Broumov skočí a vykreslí oblouky. Chat není postranní panel, je to ovládání aplikace.

Azure OpenAI, volání zhruba takto:

```
POST https://<resource>.openai.azure.com/openai/deployments/<deployment>/chat/completions
     ?api-version=2024-10-21
Header: api-key: <klíč>
Body:   { messages:[...], tools:[...], tool_choice:"auto" }
```

Přesný název deploymentu a api-version dostanete od organizátorů — vedoucí týmů
dostávají postup po registraci (čl. 5.3).

⚠️ **Klíč v prohlížeči je vidět.** Na demo se to toleruje, ale řekněte to nahlas dřív,
než se zeptá porota, a mějte odpověď: „v produkci by šel dotaz přes tenký proxy endpoint".
Když zbude čas, napište ten proxy — je to 20 řádků.

### 2. Vlastní model: gravitační predikce + what-if simulátor

**Tohle je ten „vlastní model na určitou věc".** Není to LLM, je to statistický model
nafitovaný přímo v prohlížeči na 5 059 tocích — a je to poctivé ML: má trénovací data,
features, koeficienty a měřitelnou chybu.

Fit metodou nejmenších čtverců, běží pod 100 ms:

```js
// features: log(velikost školy), log(vzdálenost), stejné ORP
// cíl:      log(podíl žáků obce, kteří jdou do té školy)
// váha:     počet žáků (jinak rozhodují obce s jedním žákem)
const beta = lstsq(X, y, w);   // → [-6.10, 0.58, -0.23, 0.63]
```

**What-if simulátor** — to, co z appky dělá nástroj pro kraj:

> „Co se stane, když v Broumově zruší gymnázium?"

Model vezme 342 žáků té školy a rozdělí je mezi zbývající školy podle predikované
přitažlivosti `exp(β·x)` pro každou obec zvlášť. Výstup:

- mapa obcí obarvená podle **nárůstu průměrné dojížďky v km**
- „průměrná dojížďka Broumovska stoupne z X na Y km"
- „nejvíc to odnese obec Z: +N km"

Porota tohle nikdy neviděla a Odbor školství přesně tuhle otázku řeší.

### 3. Sémantické hledání oborů

Deváťák nezná názvy oborů. Napíše *„chci dělat něco s počítači"* nebo *„baví mě zvířata"*.

87 oborů se jednou zaembeduje (`text-embedding-3-small`), dotaz taky, cosine similarity,
top 5 oborů → a rovnou se ukáže, kde se učí a jak daleko to je z tvé obce.

Levné, rychlé, a v demu to vypadá skvěle.

### 4. AI komentář nad vybranou obcí

Po kliknutí na obec se spočítají statistiky (retence, průměrná dojížďka, žebříček
mezi 448, největší cíle, rezidua) a **pošlou se modelu jako čísla** s instrukcí
napsat tři věty česky. Model nic nedopočítává, jen vypráví.

Příklad výstupu, který chcete vidět:

> „Broumov si udrží 37 % svých deváťáků, což je pod krajským průměrem. Gymnázium
> v Broumově přitahuje okolní obce 6× silněji, než odpovídá jeho velikosti — je to
> pevný lokální bod. Zbytek dětí ale jezdí přes 20 km do Náchoda a Hronova."

### 5. Hlasový vstup

`webkitSpeechRecognition` s `lang='cs-CZ'`, deset řádků kódu. V prezentaci se
zeptáte nahlas a appka odpoví — porota si to zapamatuje.

---

## Co postavit za 7,5 hodiny — žebříček priorit

Tým 3–4 lidí. **Dělejte to v tomhle pořadí a v 16:00 přestaňte přidávat.**

| # | Blok | Kdo | Čas | Bez tohohle nemáte co ukázat |
|---|---|---|---|---|
| 1 | Načtení dat + agregace na serveru | 1 člověk | 1,0 h | ✅ ano |
| 2 | Mapa: choropleth + oblouky obou směrů | 1 člověk | 1,5 h | ✅ ano |
| 3 | Oborové pouště (nejblíž. škola s oborem) | 1 člověk | 1,0 h | ✅ ano |
| 4 | Gravitační model + rezidua | 1 člověk | 1,0 h | ⬜ ne, ale je to páteř AI |
| 5 | Chatbot s voláním funkcí | 1 člověk | 1,5 h | ⬜ ne, ale kvůli tomu to děláte |
| 6 | What-if simulátor | 1 člověk | 1,0 h | ⬜ ne — nejsilnější, ale až po 1–5 |
| 7 | Sémantické hledání oborů | 1 člověk | 0,5 h | ⬜ ne |
| 8 | AI komentář + hlas | 1 člověk | 0,5 h | ⬜ ne, ale je to levné |
| 9 | Prezentace, patička se sadami, nasazení | všichni | 0,5 h | ✅ ano |

Bloky 1–3 a 9 jsou **povinné minimum**, které samo o sobě obstojí (to je zhruba
současný prototyp plus oborové pouště). Všechno ostatní je nadstavba — dělejte ji
v pořadí 4 → 5 → 6, protože 6 bez 4 nejde a 5 je to, co porotě prodáte jako AI.

**Když se nestíhá:** obětujte 7 a 8, nikdy ne 3 a 9.

---

## Pětiminutová prezentace

Porota vidí dvacet prezentací. Držte se scénáře a nemluvte o tom, co jsou otevřená data.

| čas | co se děje na plátně |
|---|---|
| 0:00–0:30 | Klik na Bartošovice v Orlických horách. „Tady bydlí deváťák. Ke 87 oborům, které kraj nabízí, to má průměrně **42,9 km**. V Hradci 8,5. To je pětinásobek." |
| 0:30–1:30 | Klik na Broumov → vějíř oblouků. „283 dětí, **63 % odjíždí** mimo vlastní ORP." Přepnutí na školu → spádovost. |
| 1:30–2:30 | Chat: *„Kde se v kraji učí Rybářství?"* → AI zavolá funkci, mapa ukáže **jednu školu**, odpověď: 42 km průměrně, nejhorší obec 91 km. „23 z 87 oborů má v kraji jedinou školu." |
| 2:30–3:30 | **What-if**: *„Co když v Broumově zruší gymnázium?"* → mapa se přebarví, „průměrná dojížďka Broumovska stoupne o X km". Tady se porota probudí. |
| 3:30–4:15 | Model: „Stejné ORP znamená **1,9× vyšší** šanci, že tam dítě půjde. Správní hranice tvarují vzdělávání." + shluky, které nekopírují ORP. |
| 4:15–5:00 | Trend: 4 obory za 5 let zanikly, Provozní technika −49 %. Patička: **vyjmenovat použité sady**. Odkaz na datakhk.cz. |

---

## Poctivé věci, které řekněte dřív, než se zeptají

Porota ocení, když díry v datech pojmenujete sami:

- **900 z 1 342 obcí bydliště leží mimo kraj** → nemáme pro ně hranice, oblouk nenakreslíme.
  Ale v číslech jsou: 4 425 žáků přijíždí ze 70 okresů ČR, 285 ze zahraničí.
- **R² gravitačního modelu je 0,30.** Není to předpověď, je to nástroj na hledání anomálií.
  Neprodávejte to jako věštbu.
- **Slabá strana reziduí je nepoužitelná** (viz výše, artefakt velikosti Hradce).
- V datech je **Broumov na Tachovsku** i Broumov na Náchodsku — stejnojmenné obce mimo kraj.
- Jedna „škola v kraji" sedí v **Praze 9 – Hloubětíně** a má prázdný okres.
- Sada dojížďky **nemá uvedený školní rok ani periodicitu aktualizace**.

---

## Použité datové sady (kritérium b)

Dejte je do patičky aplikace s odkazy — pravidla to stejně vyžadují (čl. 7.3).

1. Dojížďka do středních škol (12 013)
2. Dojížďka do vyšších odborných škol (456)
3. Hranice obcí v KHK (448)
4. Hranice ORP (15)
5. Administrativní členění obcí (448)
6. Školy a školská zařízení (1 626)
7. Počet žáků dle oborů SŠ — 5 školních roků (1 754)
8. Přijímací řízení na střední školy (352)
9. Počet ubytovaných v domovech mládeže (46)
10. Hospodářsky a sociálně ohrožené oblasti (448)
11. Autobusové zastávky VDKHK (4 538) — jen jako kontext, viz „co nevyšlo"
12. Adresář obcí (448)


---

## Prototyp

Funkční implementace všeho výše je v [`research/prototypy/advanced.html`](../prototypy/advanced.html)
— jeden soubor, Leaflet, data živě z API. Viz [Prototyp/README.md](../prototypy/README.md).

Ověřeno: gravitační model se v prohlížeči nafituje na stejná čísla jako Python skript
v `research/analyza/` (R² 0,301, efekt ORP 1,87×), chat volá všech sedm funkcí, what-if simulátor
u Gymnázia Broumov hlásí +4,8 km průměrné dojížďky pro 26 zasažených obcí.
