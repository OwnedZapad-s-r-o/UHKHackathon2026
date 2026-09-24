# Analýza datových sad a další nápady pro SmartAudit KHK

Podklad pro rozhodování, co do aplikace přidat. Vychází z rešerše všech 165 sad portálu Data KHK
(`research/kontext/`), pravidel hackathonu a toho, co SmartAudit dnes umí.

## 1. Jak se hodnotí

Každý porotce dává 0–10 bodů za každé ze čtyř kritérií. Všechna kritéria mají stejnou váhu.

| Kritérium | Co porota chce | Čím na něj SmartAudit dnes odpovídá |
|---|---|---|
| **a) Unikátnost** | Něco nového, co chybí | Energetický audit budov kraje nikdo v minulých ročnících nedělal. Pro srovnání: turistika byla 13×, školy 9×. K tomu skutečná čidla IQRF a 3D budova. |
| **b) Otevřená data** | Kolik sad KHK a NKOD a jak smysluplně | 25 sad Data KHK, ČÚZK, Open-Meteo, MŠMT. Každá sada má konkrétní úlohu (obrazovka Zdroje dat). |
| **c) Využití pro kraj** | Rozvoj kraje, možná publikace na Data KHK | Pořadí investic podle ztráty v Kč, PDF pro investiční odbor, otevřené API. Rozšíření čidel IQRF do budov. |
| **d) Využití pro veřejnost** | Užitečnost pro širokou veřejnost | Role Veřejnost: kvalita vzduchu ve třídách bez částek v Kč, otevřená data (JSON, GeoJSON, CSV). |

Do 2. kola postupuje pět nejlepších týmů a tam už rozhoduje porota bez ohledu na body.
V 5minutové ukázce proto musí zaznít jeden silný příběh. Nesmí to být prohlídka obrazovek.

## 2. Datové sady

Portál má 165 sad. 131 z nich jsou geodata se živou službou ArcGIS FeatureServer, 34 jsou jen stránky bez dat.
Sady se dají propojit přes `kod_obce`.

### Co už používáme (25 sad KHK a 4 externí zdroje)

| Úloha v aplikaci | Sady |
|---|---|
| Portfolio objektů | školy, právní subjekty škol, organizace KHK, sociální služby, nemocnice, následná péče, léčebné ústavy |
| Obsazenost budov | žáci SŠ (kraj i historie), studenti VOŠ, domovy mládeže, přijímačky SŠ a VOŠ |
| Priorita investic | HSOO, dotace do ORP (celkové, individuální, programové) |
| Památková ochrana | památkové zóny, rezervace, NKP plošné a bodové |
| Mapa a filtry | hranice okresů, ORP a kraje |
| Komunikace | adresář obcí (datové schránky, kontakty) |
| Externí | ČÚZK INSPIRE Budovy, Open-Meteo (teplota, PM10, PM2,5), MŠMT (svátky, prázdniny) |

### Co dalšího má smysl

Seřazeno podle přínosu. Počet záznamů je v závorce.

**Vysoký přínos**

| Sada | Co přinese |
|---|---|
| **Muzea a galerie** (160), **Divadla a filharmonie** (33), **Knihovny** (434), **Kulturní domy** (56) | Filtr zřizovatel = kraj přidá do portfolia další budovy kraje. Muzea potřebují stabilní klima a jsou silný případ pro čidla. Kapacita sálů zpřesní obsazenost a CO₂. |
| **Výjezdové základny ZZS** (16) | Budovy kraje s provozem 24/7. Dostanou nepřetržitý provozní profil, takže noční topení u nich není chyba. |
| **Lékařská pohotovostní služba** (11) | Ordinační hodiny v pavilonech nemocnic vysvětlí legitimní večerní a víkendový provoz. |
| **Dojížďka do SŠ** (12 013) a **do VOŠ** (456) | Skutečný počet žáků podle obce bydliště. Dálková a kombinovaná forma vysvětlí sobotní výuku. Pro veřejnost mapa „odkud žáci do školy jezdí“. |
| **NKP – pozemky, liniové**, **indikativní seznam památek** | Doplní skupinu památkové ochrany, která už je napojená (omezení pro zateplení a okna). |
| **Hranice obcí** (448), **Administrativní členění obcí** | Mapa HSOO po obcích a filtr až na obec. |
| **Dotace do okresů a do kraje** (celkem 6 sad) | Doplní kontext financí. Úspěšnost žádostí ukazuje, kde umí žádat o peníze na renovace. |

**Střední přínos**

| Sada | Co přinese |
|---|---|
| **VPS dle ZÚR – bodové** (TR 110/35 kV) | Plánované rozvodny: kontext pro fotovoltaiku a tepelná čerpadla (kapacita sítě). |
| **Specifické oblasti ZÚR – sucho** | U budov v oblasti sucha přidat opatření na zadržování dešťové vody a stínění. |
| **Rozvojové oblasti a osy ZÚR**, **územní rezervy** | Přednost investicím tam, kde poroste počet žáků a obyvatel. |
| **MAS** (17), **DSO** (32) | Kanály pro společné zakázky na energie a energetická společenství. |
| **Kulturní akce se záštitou Rady KHK** | Termíny mimořádného provozu, aby se nehlásily jako anomálie. |

Nízký přínos nebo zamítnuté: záplavová území (jen poznámka „gateway ne do sklepa“), silnice a
doprava, ambulance, turistické POI a sady o firmách.

**Mimo katalog KHK (NKOD):** CKAN Hradce Králové (ckan.mmhk.cz), 51 sad pod CC0. Obsahuje
školy, sociální služby, počty obyvatel na adresních bodech a bezbariérový Hradec.

## 3. Nápady, co přidat

Časy jsou odhad pro soutěžní den (7,5 h kódování). Písmena odkazují na kritéria z tabulky nahoře.

| # | Nápad | Kritéria | Čas |
|---|---|---|---|
| 1 | **Živé demo s IQRF čidlem na stole:** dýchnutí na CO₂ čidlo, graf vyskočí, audit místnosti zčervená do 30 s. Rozhraní už je hotové (obrazovka Senzory). | a, c | 0,5 h nácvik |
| 2 | ✅ **Hotovo** — **„Jak se dýchá ve třídě“ pro rodiče a žáky:** veřejná karta objektu (`/objekty/{kód}`) se semaforem CO₂ po místnostech, přehled `/objekty` a QR kód na dveře třídy. | a, d | — |
| 3 | **Upozornění správci:** e-mail nebo Microsoft Teams (webhook) při překročení CO₂ nebo přetápění o víkendu. Hodí se k partnerovi Microsoft. | c | 1 h |
| 4 | **Kulturní budovy kraje do portfolia** (muzea, divadla, knihovny, KD se zřizovatelem kraj) | b, c | 1 h |
| 5 | **Provoz 24/7:** ZZS a pohotovosti bez falešných nočních anomálií, základny ZZS v portfoliu | b, c | 1 h |
| 6 | **Normalizace spotřeby na denostupně** (Open-Meteo): odborník na energetiku to hned ocení, jinak srovnává nesrovnatelné. | c | 1 h |
| 7 | **Pořadí investic** jako jedno skóre: ztráta v Kč, HSOO, dotace ORP, památková ochrana a trend obsazenosti (dojížďka, přijímačky). Export do XLSX. | b, c | 1,5 h |
| 8 | **„Co když“ posuvník:** noční útlum 18 → 16 °C nebo kratší provozní doba a hned vidět úspora v Kč za rok | a, c | 1 h |
| 9 | **Dojížďka žáků:** mapa, odkud do školy jezdí, a skutečný počet žáků pro výpočet obsazenosti | b, d | 1,5 h |
| 10 | **ZÚR kontext:** rozvodny 110/35 kV a oblast sucha jako doporučení k opatřením (FVE, tepelné čerpadlo, voda) | b | 1 h |
| 11 | **Otevřená sada pro Data KHK:** hodinové agregace kvality vzduchu ve školách s metadaty DCAT-AP-CZ. Kritérium c výslovně zmiňuje publikaci na portálu. | c | 1 h |
| 12 | **Kontrola nasazení v Azure:** deploy aplikace do Azure Container Apps a veřejná URL pro porotu | c | 1 h |

**Doporučené pořadí na soutěžní den:** 1 → 6 → 7 → 3 (nápad 2 je hotový). Nejdřív čidlo a veřejný pohled
(unikátnost a veřejnost), pak odborná věrohodnost (denostupně) a hodnota pro kraj
(pořadí investic). Až potom rozšiřování sad.

## 4. Rizika

- **Simulovaná telemetrie:** v ukázce vždy říct nahlas, co je simulace a co skutečné čidlo.
  Aplikace to rozlišuje odznakem **živě**.
- **Odhad ztráty v Kč** zatím stojí na simulačním profilu, ne na měření. Odpověď na dotaz:
  „Po připojení čidel se počítá z naměřených hodin přetápění.“ To je nápad č. 6.
- **Bezpečnost MQTT:** broker je anonymní, určený jen pro uzavřenou síť. Pro provoz jsou
  připravené přihlašovací údaje a TLS (`docs/SENZORY_IQRF.md`).
