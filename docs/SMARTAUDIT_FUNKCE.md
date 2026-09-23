# SmartAudit KHK — katalog funkcí (v2)

Podklad: prototyp `SmartAudit KHK - Aplikace.html`, zadání „6. NEW FEATURES", pravidla hackathonu
([KONTEXT_pravidla.md](../research/kontext/KONTEXT_pravidla.md)), ověřená struktura všech 165 sad KHK
([../data/khk-datasets-schema.json](../research/katalog/khk-datasets-schema.json)) a **ověřená dostupnost
ČÚZK INSPIRE Buildings WFS** (testováno 23. 9. 2026, viz část 2).

**Změny proti v1:** vyhozena povodňová data (zdůvodnění v části 1), přidán model budovy
(část 2) a 20 dalších funkcí (část 3). Dokument už neodkazuje na NAPADY.md — ten návrh neplatí.

---

## 1. Povodňová data: proč to byl špatný nápad

Ptal jsi se, jaký je pro ně v téhle appce důvod. Odpověď je, že **žádný dost dobrý.**
Byl to způsob, jak nafouknout počet použitých sad pro kritérium 2, ne funkce, kterou by někdo
potřeboval. Appka měří teplo, CO₂ a příkon; záplavová zóna neříká nic o tom, jestli budova plýtvá.

Existují dvě obhajitelná použití, ale ani jedno nestojí za samostatnou záložku:

- **Kam neumisťovat technologii.** Kotelna, rozvaděč, venkovní jednotka tepelného čerpadla nebo
  IQRF gateway ve sklepě v aktivní zóně Q100 je špatná investice. To je ale poznámka na jeden
  řádek u konkrétní budovy, ne vrstva v mapě.
- **Pořadí investic.** Objekt, který bude za 10 let v aktivní zóně, se nezatepluje první.
  To je smysluplné jen tehdy, když appka opravdu dělá investiční pořadník.

**Doporučení:** povodně vypustit. Pokud chceš rizikovou vrstvu, která k energetice *skutečně*
patří, je jí **Hospodářsky a sociálně ohrožené oblasti** (448 obcí s `urov_ohr`) — protože určuje,
kde má kraj investovat dřív, a to je otázka, na kterou appka odpovídá. A **stáří budovy**
(část 2, F20), které o spotřebě vypovídá víc než cokoli jiného z otevřených dat.

Obecné pravidlo pro zbytek dokumentu: každá sada musí odpovídat na otázku, kterou si uživatel
appky reálně klade. Sady „na počet" porota pozná a v kritériu 1 to stojí body.

---

## 2. Model budovy a plán místností — tvůj nápad, po ověření dat

Ověřil jsem, co se dá získat automaticky. Nápad je dobrý, ale musí se postavit jinak, než to
na první pohled vypadá.

### Co reálně existuje (otestováno dnes)

**ČÚZK INSPIRE Buildings WFS** — veřejný, bez registrace, bez klíče:

```
https://services.cuzk.gov.cz/wfs/inspire-bu-wfs.asp?service=WFS&version=2.0.0
  &request=GetFeature&typeNames=bu:Building&count=200
  &srsName=urn:ogc:def:crs:EPSG::4326
  &bbox=<minLat>,<minLon>,<maxLat>,<maxLon>,urn:ogc:def:crs:EPSG::4326
```

Vrací pro každou budovu:

| Atribut | Ověřená hodnota | Použití |
|---|---|---|
| `bu-core2d:geometry2D` | polygon obrysu (GML posList) | **půdorys budovy** |
| `bu-ext:officialArea` | `113 m²` (constructedArea) | zastavěná plocha → odhad podlahové |
| `bu-base:dateOfConstruction` | `1945-12-31` | **stáří → energetická třída** |
| `bu-base:currentUse` | `publicServices`, `collectiveResidence`, `industrial`… | typ provozu |
| `bu-base:conditionOfConstruction` | `functional` | v provozu / ruina |
| `bu-ext:cadastralParcel` | odkaz do katastru | „Číslo parcely", které máte v UI jako mock |
| `bu-base:externalReference` | kód RÚIAN SO (`20099703`) | spárování s adresou školy |

CRS: výchozí EPSG:5514 (Křovák), ale podporuje i 4326 — přepočet nepotřebujete, máte na něj
navíc `research/analyza/hk/krovak.py`.

### Co naopak NEEXISTUJE — a to je ta důležitá část

**Počet podlaží ani výška v téhle službě nejsou.** Testoval jsem 200 budov v centru Hradce
Králové: `numberOfFloorsAboveGround` i `heightAboveGround` jsou **ve 200 z 200 případů
`xsi:nil` s důvodem „Unpopulated"**. Takže LOD1 extruze *z tohohle zdroje* nejde — žádný
automatický 3D model kraje si z ČÚZK nevytáhnete.

Počet nadzemních podlaží a dokonce i **způsob vytápění** jsou v RÚIAN jako technickoekonomické
atributy stavebního objektu, ale dostupné jen přes **výměnný formát RÚIAN (VFR)** ve VDP —
stahuje se přes webový formulář za obec, je to GML v ZIPu a parsování je práce na hodiny.
Navíc jde o *nereferenční* data, která plní stavební úřady, takže jsou místy prázdná.
**Na 7,5 h to není.**

Vnitřní dispozice budov (plán místností) v otevřených datech neexistuje vůbec. Žádný registr
v ČR nevede, kde je ve škole která třída.

### Verdikt: ano, ale v tomto pořadí

**F20. Karta budovy z ČÚZK — DĚLEJ.** `45 min · kritérium 1, 2`
Reálný půdorys objektu v mapě (místo pinu), zastavěná plocha, rok výstavby, číslo parcely,
odkaz do katastru. Tři věci, které máte dnes jako mock, se stanou skutečnými.
Z roku výstavby udělejte odhad energetické kategorie podle období (do 1945 / 1946–1975 /
1976–1990 / 1991–2005 / po 2006) a **použijte ho jako vstup do výpočtu ztráty** — tím přestane
být ztráta v Kč vycucaná z prstu a začne mít metodiku, kterou umíte obhájit.

**F21. Schematický řez budovou se senzory — DĚLEJ TOHLE místo 3D.** `90 min · kritérium 1, 4`
Ne půdorys, ale **řez**: SVG mřížka `podlaží × zóny`, každá buňka obarvená podle teploty nebo
CO₂, ikona senzoru tam, kde měří. Generuje se z metadat (`floors: 3, zones: ['severní křídlo',
'jižní křídlo', 'jídelna']`), ne z reálného plánu, takže funguje pro **libovolnou** budovu, ne
jen pro tu jednu vymodelovanou. Do 5sekundového pohledu je vidět „druhé podlaží, jižní strana,
1 800 ppm" — to je ten „aha" moment, který v prototypu dnes chybí, protože heatmapa hodin nic
neříká o *místě*.
Zóny pro demo budovy vyplňte ručně (10 minut), u ostatních zobrazte „dispozice nezadána"
a nabídněte formulář. Přiznaná neúplnost působí lépe než falešný plán.

**F22. Plný 3D model — NEDĚLEJ na hackathonu.** `4 h+ · efekt nízký`
Bez výšek z otevřených dat by to znamenalo modelovat ručně jednu budovu. Vznikne pěkná
rotující krabice, která neodpovídá na žádnou otázku, zabere 4 hodiny a ve 5minutové prezentaci
sežere čas, který má patřit číslům. Pokud to chceš za každou cenu vizuálně, extruduj půdorys
z F20 v deck.gl `PolygonLayer` s `extruded: true` a výškou `podlaží × 3,2 m` — ale podlaží
si musíš odhadnout (např. z `currentUse` a plochy) a **musí to být v UI napsané jako odhad**.
To je 30 minut a 80 % vizuálního efektu.

**F23. Orientace a solární zisky — bonus, když F20 stojí.** `30 min · kritérium 1`
Z půdorysu spočítáte azimut nejdelší stěny. Open-Meteo dává historické i předpovědní
`shortwave_radiation`. Výstup: „Jižní fasáda, 42 m. V květnu mezi 11–15 h dopadá ~600 W/m² —
přehřívání není porucha vytápění, je to chybějící stínění." Appka, která **odmítne** falešnou
anomálii a vysvětlí proč, vypadá na jinou úroveň než appka, která jen svítí červeně.

---

## 3. Další funkce, o které se dá appka rozšířit

Seřazeno podle poměru efekt/čas. Sloupec „kdo" říká, pro kterou personu to je — appka dnes
obsluhuje jen krajského úředníka, a to je její hlavní slabina v kritériu 4.

### 3.1 Zdraví vnitřního prostředí — nejvíc nevyužitý prostor

**F24. Dopad CO₂ na výuku, ne jen ppm.** `40 min · veřejnost/ředitel · kritérium 1, 4`
Místo „1 650 ppm" ukažte „v téhle třídě se dnes 3,5 hodiny učilo nad hygienickým limitem".
Studie kvality vnitřního prostředí spojují koncentrace nad ~1 000 ppm s měřitelným poklesem
soustředění (Allen et al., Harvard COGfx — *citaci si před prezentací ověřte a uveďte*).
Převeďte to na „ztracené hodiny pozornosti za měsíc". Peníze zajímají ekonomický odbor,
ale tohle zajímá rodiče, ředitele i porotu — a je to jediná metrika v celé appce, kterou
nikdo nemusí překládat.

**F25. Kdy větrat — a kdy naopak ne.** `35 min · ředitel/učitel · kritérium 1, 4`
Z trendu CO₂ dopočítáte, za jak dlouho se limit překročí, a doporučíte okno na větrání do
přestávky. Otočka: Open-Meteo **Air Quality API** (zdarma, bez klíče) dává PM2.5/PM10, takže
appka umí říct „teď nevětrat, venku je 48 µg/m³ PM10 — otevřete až po 16:00".
Konkrétní rada pro konkrétního člověka v konkrétní hodinu. Tohle je ta funkce, po které
se aplikace používá denně, ne jednou při auditu.

**F26. Report souladu s vyhláškou.** `25 min · kraj/ředitel · kritérium 3`
Vyhláška **410/2005 Sb.** stanoví pro učebny teplotní rozmezí a limit **1 500 ppm CO₂**
(*znění ověřte*). Měsíční PDF „počet hodin nad limitem, po místnostech" je dokument, který
ředitel musí mít pro hygienu a dnes ho nemá. Tím se z appky stává nástroj, který někdo
*potřebuje*, ne který si prohlédne.

### 3.2 Peníze, kterým jde věřit

**F27. Rekonciliace s vyúčtováním.** `40 min · kraj · kritérium 3`
Pole pro roční fakturovanou spotřebu (GJ / kWh) a srovnání s tím, co říkají senzory.
Rozdíl je buď chyba měření, nebo ztráta v rozvodech — a v obou případech je to zjištění.
Zároveň to řeší největší slabinu appky: čísla dnes nemají o co se opřít.

**F28. Kumulativní „protopeno navíc" od začátku sezóny.** `25 min · ředitel · kritérium 4`
Jeden velký čítač, který od 1. 9. roste. Rostoucí číslo mění chování; statická tabulka ne.

**F29. Seznam opatření s návratností, ne jen ztráta.** `45 min · kraj · kritérium 3`
Ztráta bez řešení je stížnost. Ke každému zjištění přiřaďte opatření z pevného katalogu
(seřízení ekvitermní regulace ~0 Kč · termostatické hlavice ~15 tis. · víkendový útlum v MaR
~0 Kč · rekuperace ~400 tis. · zateplení ~mil.) s Kč/rok a návratností v měsících.
Odbor si pak z appky odnese rozpočtovou položku.

**F30. Transparentní cenový model.** `20 min · kraj · kritérium 1`
Vstupní pole Kč/GJ a Kč/kWh místo zadrátované konstanty. Na otázku „jak jste se dostali
k těm milionům" musíte umět odpovědět posunutím jednoho čísla v UI.

**F31. Uhlíková stopa a ESG.** `30 min · kraj · kritérium 3`
kWh → t CO₂ (emisní faktor konfigurovatelný, pro elektřinu v ČR řádově 0,4 t/MWh — *ověřte*).
Kraje mají reportingové povinnosti a nemají z čeho čerpat. Jedna karta, velká relevance.

### 3.3 Aby to používal člověk, ne dashboard

**F32. Obrazovka „Kdo jsem".** `30 min · všichni · kritérium 1, 4`
Tři dlaždice místo rovnou mapy: **Jsem z kraje** · **Spravuji budovu** · **Jsem z veřejnosti**.
Stejná data, tři vstupy, tři různé sady dovoleného. Bez tohohle appka nemá adresáta pro
kritérium 4.

**F33. Týdenní digest pro ředitele.** `35 min · ředitel · kritérium 4`
„Tento týden: 2 anomálie, 14 300 Kč, doporučení: …" Náhled e-mailu v UI stačí, posílat
se nic nemusí. Ukazuje to, že appka za člověka pracuje, i když v ní není.

**F34. Hlášení od učitele jako vstup do auditu.** `40 min · učitel · kritérium 1, 4`
Dvě tlačítka: „je tu zima" / „je tu dusno". Hlášení se objeví na časové ose vedle telemetrie.
Tam, kde senzor není, je to *jediný* zdroj dat — a tam, kde je, je to kontrola, jestli
model odpovídá realitě. Levné, lidské, a nikdo jiný to mít nebude.

**F35. Z anomálie úkol se zodpovědným a termínem.** `40 min · kraj · kritérium 3`
Anomálie → úkol → stav (nové / předáno správci / vyřešeno) → historie. Appka přestane být
detektor a stane se evidencí, což je přesně to, co úřad potřebuje, aby si ji vzal.

**F36. Energetická výzva mezi školami.** `50 min · žáci/veřejnost · kritérium 1, 4`
Veřejný žebříček úspor mezi školami, obarvený na žáka a normalizovaný na denostupně.
Zapojí do appky lidi, kteří v budově skutečně jsou — a to je ta „široká veřejnost"
z kritéria 4. Riziko: bez normalizace (F38) je žebříček nespravedlivý a porota to trefí.

### 3.4 Metodika a důvěryhodnost

**F37. Panel „Zdroje dat" z manifestu.** `15 min · POVINNÉ`
Pravidla: *„V aplikaci musí být hypertextový odkaz na použitá otevřená data."* V prototypu
jsem ho nenašel — prošel jsem všechny textové uzly. Generujte ho z
`research/katalog/khk-datasets-manifest.json` včetně počtu reálně načtených záznamů.

**F38. Normalizace na denostupně.** `40 min · kritérium 1`
`HDD = Σ max(0, 21 − T_den)` z Open-Meteo Historical API (zdarma, bez klíče), všude
ukazovat **kWh/m²/HDD**. Bez toho jsou všechna srovnání v appce metodicky neplatná
a porotce z energetiky to pozná do minuty.

**F39. Kalendář prázdnin jako detektor.** `30 min · kritérium 1`
Svátky a prázdniny MŠMT jsou předem známé. „Budova má být prázdná a topí na 22 °C" je
deterministické pravidlo — vysvětlitelné, bez ML, a dá smysl heatmapě v UI.

**F40. Kvalita dat místo dokonalé fikce.** `20 min · kritérium 1`
`% dostupnosti měření za 30 dní` a `poslední kontakt` u každého senzoru; anomálie nad
senzorem, který tři dny mlčel, se označí jako nejistá. Přiznaná nejistota působí
kompetentněji než bezchybný mock.

**F41. Srovnání s podobnými objekty.** `40 min · kritérium 1, 4`
„3. nejhorší z 28 základních škol podobné velikosti." Peer skupina z `zarizeni_druh`
a velikostního pásma (počet žáků — 299 záznamů v „Počet žáků v oborech SŠ", lůžková
kapacita u domovů mládeže — 46 záznamů má i skutečně ubytované).

**F42. kWh na žáka / lůžko.** `30 min · kritérium 1, 3`
U domovů mládeže je v datech kapacita **i** skutečně ubytovaní, tedy reálná obsazenost.
Budova vytápěná na 40 % obsazenosti je jiný problém než netěsná okna a má jiné řešení.

### 3.5 Data KHK v roli kontextu (co zbylo po vyhození povodní)

**F43. Reálné portfolio místo mock budov.** `60 min · kritérium 2, 3`
Školy a školská zařízení (**1 626**) · Právní subjekty škol (566) · Poskytovatelé sociálních
služeb (**461**) · Organizace Královéhradeckého kraje (**126**, obsahuje pole `odbor` —
ten „Zřizující odbor", který je v UI dnes vymyšlený) · nemocnice a lůžková péče (30+).
Filtr `zrizovatel === 'kraj'` dá přesně objekty, do kterých kraj investuje.

**F44. HSOO jako váha investiční priority.** `30 min · kritérium 2, 3`
448 obcí s `urov_ohr` (mírná / střední / vysoká). Objekt v ohrožené oblasti jde v pořadníku
dřív. Na rozdíl od povodní tohle odpovídá na otázku, kterou appka klade.

**F45. Dotační kontext ORP.** `25 min · kritérium 2`
15 polygonů ORP s `poc_dotaci` a `finance` (ORP Hradec Králové: 1 596 dotací, 197,7 mil. Kč).
Dnes je to v UI mock; napojení je jeden lookup podle `kod_orp`.

**F46. Pořadník zateplení s exportem.** `45 min · kritérium 3`
Řazení podle `ztráta × HSOO × stáří (F20) × využitost (F42)`, export XLSX/PDF jako
„podklad pro investiční odbor". Dashboard říká stav; tohle říká, co udělat v pondělí.

**F47. Kam dát dalších 20 senzorů.** `30 min · kritérium 3`
Přepis zadané ROI kalkulačky z jedné budovy na celé portfolio: „za 700 tis. Kč osaďte těchto
20 objektů, odhalí odhadem X Kč/rok." Pro MICRORISC obchodní případ, pro kraj plán nákupu.

**F48. Publikace zpět jako otevřená data.** `40 min · kritérium 3`
Z výstupu appky udělejte DCAT-AP-CZ sadu „Energetický profil objektů KHK" — „takhle by to
vypadalo na portálu Data KHK". Doslovné znění kritéria 3.

### 3.6 AI (Microsoft) — ať boduje skrz kritéria, ne vedle nich

**F49. Chat s function calling, ne s volným textem.** `60 min · kritérium 1, 4`
Model musí volat `getBuilding`, `getTelemetry`, `compareToPeers`, `getMeasures` a čísla brát
odtud. Halucinovaná úspora v korunách je v pětiminutové prezentaci nevratná.

**F50. Veřejná persona chatu.** `20 min navíc · kritérium 4`
Stejný endpoint, jiný system prompt a jiná oprávnění: rodič se zeptá „jak se dýchá ve třídě
mého dítěte", nevidí Kč ani ovládání ventilů.

**F51. Návrh dopisu správci nebo zřizovateli.** `25 min · kritérium 3`
Z anomálie vygenerovat text „žádáme o seřízení ekvitermní regulace, podklad v příloze".
Úřad komunikuje dokumenty; appka, která je umí napsat, se používá.

### 3.7 Prezentace

**F52. Režim prezentace.** `35 min`
Skriptovaný 60s scénář: pátek 16:00 → naskočí anomálie → audit → tlačítko → úspora.
Máte 5 minut a živá data můžou zradit.

**F53. Přehrání posledního týdne.** `30 min`
Posuvník časem nad mapou i řezem budovy (F21). Vidět, jak se budova v pátek odpoledne
„rozsvítí", je přesvědčivější než jakýkoli graf.

---

## 4. Co bych nedělal

- Plný 3D model budovy (F22) — 4 hodiny, žádná odpovědět na žádnou otázku.
- Povodňové vrstvy — viz část 1.
- Autentizace, role, databáze — 7,5 hodiny.
- Skutečné MQTT — mock stačí, nikdo neověří.
- ML predikce spotřeby — nevysvětlitelné a není na čem trénovat.
- Další grafy do telemetrie — grafů je dost, chybí rozhodnutí.
- Parsování RÚIAN VFR kvůli počtu podlaží — hodiny práce za jeden atribut.

---

## 5. Kdybych měl vybrat, co postavit v těch 7,5 h

```
09:00–10:00  F43 reálné portfolio + F37 zdroje dat          (bez toho nemá appka data ani legitimitu)
10:00–10:45  F20 karta budovy z ČÚZK                        (půdorys, plocha, rok, parcela — konec mocků)
10:45–12:00  F21 schematický řez se senzory                 (vizuální jádro prezentace)
12:30–13:15  F24 dopad CO₂ na výuku + F26 report vyhlášky   (lidská metrika + dokument pro ředitele)
13:15–14:00  F38 denostupně + F30 cenový model              (aby čísla obstála)
14:00–14:45  F25 kdy větrat (vč. PM z Open-Meteo)           (funkce na denní použití)
14:45–15:30  F29 opatření s návratností + F46 pořadník      (co si odnese kraj)
15:30–16:30  F49 chat s function calling                    (až když zbytek stojí)
16:30–17:00  F52 režim prezentace, export, commit
```

Pokud se něco nestihne, obětuj AI (F49) — v hodnoticích kritériích není.

Věta pro prezentaci: **„Neptáme se, kolik stupňů má budova. Ptáme se, ve které třídě se dneska
tři hodiny učilo v dusnu, kolik to stálo a co s tím udělat do pondělí — a u 1 600 objektů kraje,
kde senzor není, umíme říct, kam ho dát první."**
