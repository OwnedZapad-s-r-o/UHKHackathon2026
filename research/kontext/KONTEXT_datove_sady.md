# Kontext: Otevřená data Královéhradeckého kraje — hloubkový katalog

Zdroj: `https://open.datakhk.cz/katalog.jsonld` — **165 datových sad**.
U všech 131 geodatových sad ověřeno proti živému ArcGIS API: **počet záznamů, všechna pole a reálné hodnoty číselníků**. Staženo 3. 9. 2026.

## Jak na data

```
<api_url>/query?where=1%3D1&outFields=*&f=geojson&resultRecordCount=2000&resultOffset=0
```

- `&returnGeometry=false` — rychlejší, když stačí atributy
- `&returnDistinctValues=true&outFields=pole` — číselník hodnot
- `&groupByFieldsForStatistics=pole&outStatistics=[{"statisticType":"sum",...}]` — agregace na serveru
- Bodové sady mají souřadnice přímo v atributech jako `x` (lon) a `y` (lat)
- Společná pole skoro všude: `nazev_obce`/`kod_obce`, `nazev_orp`/`kod_orp`, `nazev_okresu`/`kod_okresu`
  → **`kod_obce` je spojovací klíč mezi sadami i s hranicemi obcí (448 polygonů)**

---

## ŠKOLSTVÍ A VZDĚLÁVÁNÍ — 14 sad

### Dojížďka do středních škol

`n=12013` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací středních škol v Královéhradeckém kraji, která obsahuje údaje týkající se dojížďky studentů do středních škol z jejich míst s trvalým bydlištěm a další údaje. Data poskytl Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_skoly`, `izo`, `red_izo`, `zrizovatel`, `typ_tridy`, `obor`, `obor_nazev`, `forma_vzdelavani`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `nazev_okresu_bydliste_studenta`, `kod_okresu_bydliste_studenta`, `nazev_obce_bydliste_studenta`, `kod_obce_bydliste_studenta`, `pocet_zaku`

**Hodnoty:**

- `zrizovatel`: církev · kraj · obec · privátní sektor
- `forma_vzdelavani`: dálková · denní · distanční

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Doj%C3%AD%C5%BE%C4%8Fka_do_st%C5%99edn%C3%ADch_%C5%A1kol/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/c62b3bf8a0dd40b4b1d70bc419d5d6e4

### Počet žáků dle oborů středních škol v Královéhradeckém kraji od školního roku 2021/2022

`n=1754` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací počtu žáků dle oborů středních škol v Královéhradeckém kraji od školního roku 2021/2022. Zdrojem dat je Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `redizo`, `izo`, `izo_sidlo`, `zrizovatel`, `druh_zarizeni`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `kategorie`, `obor_kod`, `obor_nazev`, `skolni_rok`, `pocet_zaku_celkem`, `pocet_absolventu`, `pocet_prijatych`

**Hodnoty:**

- `zrizovatel`: církev · kraj · MŠMT · obec · privátní sektor
- `druh_zarizeni`: SŠ podle § 16 odst. 9 ŠZ · SŠ při vých.zař. · Střední škola
- `kategorie`: Gymnázium čtyřleté - K · Gymnázium osmileté - K · Gymnázium šestileté - K · Lyceum - M · Nástavbové studium - L · Obory bez matur. zkoušky a výuč. listu - C a · Obory s maturitní zkouškou - L · Obory s maturitní zkouškou - M · Obory s výučním listem - E · Obory s výučním listem - H
- `skolni_rok`: 2021/2022 · 2022/2023 · 2023/2024 · 2024/2025 · 2025/2026

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Po%C4%8Det_%C5%BE%C3%A1k%C5%AF_dle_obor%C5%AF_st%C5%99edn%C3%ADch_%C5%A1kol_v_KHK/FeatureServer/4`

Portál: https://www.datakhk.cz/datasets/0be317969b43455b93d08666cabda783

### Školy a školská zařízení

`n=1626` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací všech škol a školských zařízení v Královéhradeckém kraji podle zřizovatele. Zdrojem dat je Školský informační portál Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `zrizovatel`, `redizo`, `zarizeni_druh`, `izo`, `pravni_forma`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `zrizovatel`: církev · kraj · MO · MŠMT · obec · privátní sektor
- `pravni_forma`: organizační složka státu · školská právnická osoba · akciová společnost · obecně prospěšná společnost · příspěvková organizace · školská právnická osoba · společnost s ručením omezeným · ústav · vysoká škola

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Seznam_%C5%A1kol_a_%C5%A1kolsk%C3%BDch_za%C5%99%C3%ADzen%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/fc365d95ea124c6fb18540f5e5a2eef1

### Právní subjekty vykonávající činnost škol a školských zařízení

`n=566` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací právních subjektů vykonávajících činnost škol a školských zařízení v Královéhradeckém kraji. Zdrojem dat je Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `pravni_forma`, `zrizovatel`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `pravni_forma`: Akciová společnost · Obecně prospěšná společnost · Příspěvková organizace zřízená územním samos · Školská právnická osoba · Společnost s ručením omezeným · Státní příspěvková organizace · Ústav
- `zrizovatel`: církev, náboženské hnutí · kraj · obec či dobrovolný svazek obcí · privátní sektor · stát

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pr%C3%A1vn%C3%AD_subjekty_vykon%C3%A1vaj%C3%ADc%C3%AD_%C4%8Dinnost_%C5%A1kol_a_%C5%A1kolsk%C3%BDch_za%C5%99%C3%ADzen%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/f38e5afcb622434bb4dc5a493882ff9a

### Dojížďka do vyšších odborných škol

`n=456` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací vyšších odborných škol v Královéhradeckém kraji, která obsahuje údaje týkající se dojížďky studentů do vyšších odborných škol z jejich míst s trvalým bydlištěm a další údaje. Data poskytl Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_skoly`, `izo`, `red_izo`, `zrizovatel`, `typ_tridy`, `obor`, `obor_nazev`, `forma_vzdelavani`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `nazev_okresu_bydliste_studenta`, `kod_okresu_bydliste_studenta`, `nazev_obce_bydliste_studenta`, `kod_obce_bydliste_studenta`, `pocet_studentu`

**Hodnoty:**

- `nazev_skoly`: Česká lesnická akademie Trutnov - střední šk · Gymnázium, Střední odborná škola a Vyšší odb · Obchodní akademie, Střední pedagogická škola · Střední škola a vyšší odborná škola aplikova · Vyšší odborná škola a Střední průmyslová ško · Vyšší odborná škola zdravotnická a Střední z · Vyšší odborná škola zdravotnická, Střední zd · Vyšší odborná škola, Střední škola, Základní · Zemědělská akademie a Gymnázium Hořice - stř
- `izo`: 110029178 · 110029283 · 110030621 · 110032161 · 110100182 · 150008716 · 151037621 · 181048507 · 181078520
- `red_izo`: 600011631 · 600011682 · 600011798 · 600012069 · 600012905 · 600019772 · 600019829 · 600024016 · 691012458
- `zrizovatel`: kraj · privátní sektor
- `obor`: 2341N02 · 2641N09 · 4132N01 · 5341N11 · 5341N51 · 5343N11 · 5343N21 · 5344N11 · 6341N22 · 6843N02 · 7531N03 · 7533N02
- `obor_nazev`: Diplomovaná dětská sestra · Diplomovaný farmaceutický asistent · Diplomovaný zdravotní laborant · Diplomovaný zubní technik · Energetika · Lesnictví · Obnova a rozvoj venkova · Předškolní a mimoškolní pedagogika · Strojírenství s využitím CAD a CAM · Tlumočnictví českého znakového jazyka · Účetnictví a daně
- `forma_vzdelavani`: denní · kombinovaná

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Doj%C3%AD%C5%BE%C4%8Fka_do_vy%C5%A1%C5%A1%C3%ADch_odborn%C3%BDch_%C5%A1kol/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/86102cc9f02e412b8a74f744b067b92c

### Přijímací řízení na střední školy

`n=352` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací středních škol v Královéhradeckém kraji, která obsahuje údaje týkající se přijímacího řízení a další údaje. Data poskytl Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `zrizovatel`, `pravni_forma`, `redizo`, `izo`, `obor_nazev`, `obor_kod`, `nazev_svp__zamereni_oboru`, `vzdelavani_delka_roky`, `zpusob_ukonceni_vzdelavani`, `pocet_prihl_1_kolo_prij_riz`, `pocet_prijatych_po_1_kole_prij`, `pocet_nabizenych_mist`, `rocni_skolne`, `prijimaci_zkouska`, `forma_vzdelavani`, `druh_vzdelavani`, `obor_urcen_pro_uchaz_s_uk_vzd`, `obor_vhodny_pro_ozp`, `povinna_lekarska_prohlidka`, `poznamka`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `zrizovatel`: církev · kraj · MŠMT · obec · privátní sektor
- `pravni_forma`: příspěvková organizace · školská právnická osoba · společnost s ručením omezeným
- `vzdelavani_delka_roky`: 1 · 2 · 3 · 4 · 5 · 6 · 8
- `zpusob_ukonceni_vzdelavani`: maturitní zkouška · maturitní zkouška + výučný list · výučný list · závěrečná zkouška
- `forma_vzdelavani`: Dálková forma · Denní forma
- `druh_vzdelavani`: nástavbové studium · střední vzdělání s maturitní zkouškou + výuč · střední vzdělávání · střední vzdělávání s maturitní zkouškou · střední vzdělávání s výučním listem · zkrácené studium k získání středního vzdělán · zkrácené studium k získání středního vzdělán
- `obor_urcen_pro_uchaz_s_uk_vzd`: ANO · NE
- `obor_vhodny_pro_ozp`: s maturitní zkouškou · s ukončenou povinnou škol. docházkou · s výučním listem · z 5. tříd ZŠ · ze 7. tříd ZŠ

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99ij%C3%ADmac%C3%AD_%C5%99%C3%ADzen%C3%AD_na_st%C5%99edn%C3%AD_%C5%A1koly/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/cd83e052346b47c8a60c7af76e539bda

### Studenti vysokých škol podle oborů

`n=300` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací studentů vysokých škol podle oborů za rok 2022. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

**Pole:** `nazev`, `rid`, `fakulta_pracoviste`, `nazev_studijního_programu`, `kod_sp`, `nazev_studijniho_oboru`, `kod_so`, `celkem_khk`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `nazev`: Česká zemědělská univerzita v Praze · Metropolitní univerzita Praha, o.p.s. · Univerzita  Hradec Králové · Univerzita Karlova
- `fakulta_pracoviste`: celoškolská pracoviště · Fakulta informatiky a managementu · Farmaceutická fakulta v Hradci  Králové · Filozofická fakulta · Lékařská fakulta v Hradci  Králové · Pedagogická fakulta · Přírodovědecká fakulta · Provozně ekonomická fakulta

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Studenti_vysok%C3%BDch_%C5%A1kol/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/6a862a5ec40241ef87e2f824b64aceeb

### Počet žáků v oborech středních škol zřizovaných krajem

`n=299` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací středních škol zřizovaných Královéhradeckým krajem včetně přehledu počtu žáků v jednotlivých školách. Data jsou za školní rok 2025/2026. Zdrojem dat je Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `redizo`, `izo`, `izo_sidlo`, `zrizovatel`, `druh_zarizeni`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `kategorie`, `obor_kod`, `obor_nazev`, `skolni_rok`, `pocet_zaku_celkem`, `pocet_absolventu`, `pocet_prijatych`

**Hodnoty:**

- `zrizovatel`: kraj
- `druh_zarizeni`: SŠ podle § 16 odst. 9 ŠZ · Střední škola
- `kategorie`: Gymnázium čtyřleté - K · Gymnázium osmileté - K · Gymnázium šestileté - K · Lyceum - M · Nástavbové studium - L · Obory bez matur. zkoušky a výuč. listu - C a · Obory s maturitní zkouškou - L · Obory s maturitní zkouškou - M · Obory s výučním listem - E · Obory s výučním listem - H
- `skolni_rok`: 2025/2026

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Po%C4%8Dty_%C5%BE%C3%A1k%C5%AF_v_oborech_st%C5%99edn%C3%ADch_%C5%A1kol_z%C5%99izovan%C3%BDch_krajem/FeatureServer/4`

Portál: https://www.datakhk.cz/datasets/cc4993a5a0294c3f948481ded8c23673

### Absolventi vysokých škol podle oborů

`n=223` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací absolventů vysokých škol podle oborů za rok 2022. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

**Pole:** `nazev`, `rid`, `fakulta_pracoviste`, `nazev_studijniho_programu`, `kod_sp`, `nazev_studijniho_oboru`, `kod_so`, `celkem_khk`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `nazev`: Česká zemědělská univerzita v Praze · Univerzita  Hradec Králové · Univerzita Karlova
- `fakulta_pracoviste`: celoškolská pracoviště · Fakulta informatiky a managementu · Farmaceutická fakulta v Hradci  Králové · Filozofická fakulta · Lékařská fakulta v Hradci  Králové · Pedagogická fakulta · Přírodovědecká fakulta · Provozně ekonomická fakulta

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Absolventi_vysok%C3%BDch_%C5%A1kol/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/76c3943243574f89aded2f36fff851de

### Počet ubytovaných v domovech mládeže a internátech

`n=46` · bod · aktualizace: unknown

Datová sada zobrazující počet ubytovaných v domovech mládeže a internátech. Zdrojem dat je Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_organizace`, `ico`, `identifikator_reditelstvi`, `zrizovatel`, `typ`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `pocet_luzkove_kapacity`, `celkovy_pocet_pokoju`, `uhrnny_pocet_ubytovanych`, `www`

**Hodnoty:**

- `zrizovatel`: církev · kraj · privátní sektor
- `typ`: Domov mládeže · Internát

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Po%C4%8Det_ubytovan%C3%BDch_v_DDM_a_intern%C3%A1tech/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/97b02c71262b4d31971d2af702d12892

### Základní umělecké školy

`n=32` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací základních uměleckých škol na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `nazev_zrizovatele`, `typ_zrizovatele`, `ico`, `bezbarierovost`, `pozn_bezbarierovost`, `vyucovane_predmety`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `typ_zrizovatele`: církev · obec · soukromá osoba
- `bezbarierovost`: ano · částečně · ne

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1kladn%C3%AD_um%C4%9Bleck%C3%A9_%C5%A1koly/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/8ecc8847b9b54ef1bac90d17b8813617

### Domy dětí a mládeže a střediska volného času

`n=27` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací domů dětí a mládeže a středisek volného času na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `kratky_nazev`, `popis`, `zrizovatel`, `typ_zrizovatele`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `kratky_nazev`: Barák · DDM Domeček · DDM Domino · DDM Hostinné · DDM Jednička · DDM Kamarád · DDM Klíč · DDM Pelíšek · DDM Sion · DDM STONOŽKA · DDM Vamberk · Déčko
- `popis`: Kroužky pro děti a mládež, příměstské a poby · Kroužky pro děti a mládež, příměstské,  poby · Kroužky pro děti a mládež, tábory a příměsts · Kroužky pro děti a mládež, tábory, akce pro  · Kroužky pro děti a mládež, tábory, akce pro  · kroužky pro děti, mládež a dospělé, příměsts · Pořádá zájmové kroužky pro děti a mládež, ak · Pořádá zájmové kroužky pro děti a mládež, pr · Pořádá zájmové kroužky pro děti a mládež, so · Pořádá zájmové kroužky pro děti a mládež. · Pořádá zájmové kroužky pro děti, mládež a do · Pořádá zájmové kroužky pro děti, mládež a do

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Domovy_d%C4%9Bt%C3%AD_a_ml%C3%A1de%C5%BEe_a_st%C5%99ediska_voln%C3%A9ho_%C4%8Dasu/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/d4f253b7bab842be98555eb81f057e70

### Přijímací řízení na vyšší odborné školy

`n=19` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací přijímacích řízení na vyšší odborné školy v Královéhradeckém kraji. Datová sada obsahuje údaje týkající se přijímacího řízení a další údaje. Data poskytl Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `zrizovatel`, `ico`, `pravni_forma`, `redizo`, `izo`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `obor_kod`, `obor_nazev`, `vzdelavani_delka`, `zpusob_ukonceni`, `pocet_prihlasenych_do_1_kola`, `pocet_prijatych_po_1_kole`, `pocet_nabizenych_mist`, `rocni_skolne`, `prijimaci_zkouska`, `forma_vzdelavani`, `druh_vzdelavani`, `obor_ukoncen_pro_uchazece`, `obor_vhodny_pro_ozp`, `povinna_lekarska_prohlidka`, `poznamka`, `psc`, `www`

**Hodnoty:**

- `nazev`: Česká lesnická akademie Trutnov - střední šk · Obchodní akademie, Střední pedagogická škola · Střední škola a vyšší odborná škola aplikova · Vyšší odborná škola zdravotnická a Střední z · Vyšší odborná škola zdravotnická, Střední zd · Vyšší odborná škola, Střední škola, Základní · Zemědělská akademie a Gymnázium Hořice - stř
- `zrizovatel`: kraj · privátní sektor
- `pravni_forma`: příspěvková organizace · společnost s ručením omezeným
- `redizo`: 600011631 · 600011798 · 600012905 · 600019772 · 600019829 · 600024016 · 691012458
- `izo`: 110029178 · 110029283 · 110030621 · 110100182 · 151037621 · 181048507 · 181078520
- `obor_kod`: 26-41-N/24 · 26-47-N/09 · 41-32-N/01 · 53-41-N/11 · 53-41-N/51 · 53-43-N/11 · 53-43-N/21 · 53-44-N/11 · 68-43-N/02 · 75-31-N/03 · 75-33-N/02 · 75-33-N/03
- `obor_nazev`: Diplomovaná dětská sestra · Diplomovaná všeobecná sestra · Diplomovaný farmaceutický asistent · Diplomovaný zdravotní laborant · Diplomovaný zubní technik · Energetika · Lesnictví · Obnova a rozvoj venkova · Počítačová umění a design · Předškolní a mimoškolní pedagogika · Programování · Tlumočnictví českého znakového jazyka
- `vzdelavani_delka`: 3 · 3,5
- `zpusob_ukonceni`: absolutorium
- `prijimaci_zkouska`: ANO · B, CJ · B, CJ, ZR · CH, CJ · ČJ, OP, VZ, PO · PO · pohovor · PZ · talentová
- `forma_vzdelavani`: denní · kombinovaná
- `druh_vzdelavani`: VOŠ
- `obor_ukoncen_pro_uchazece`: s maturitní zkouškou
- `obor_vhodny_pro_ozp`: ANO · NE
- `poznamka`: v Červeném Kostelci · v Trutnově

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99ij%C3%ADmac%C3%AD_%C5%99%C3%ADzen%C3%AD_na_vy%C5%A1%C5%A1%C3%AD_odborn%C3%A9_%C5%A1koly/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/20fb5f528c7f430c9d8d2de07f4b6831

### Počet studentů v oborech vyšších odborných škol zřizovaných krajem

`n=14` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací vyšších odborných škol zřizovaných Královéhradeckým krajem včetně přehledu počtu studentů v jednotlivých školách. Data jsou za školní rok 2024/2025. Zdrojem dat je Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `pravni_forma`, `redizo`, `izo`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `nazev_obce_pracoviste`, `kod_obce_pracoviste`, `nazev_ulice_pracoviste`, `cislo_domovni_pracoviste`, `typ_cisla_domovniho_pracoviste`, `cislo_orientacni_pracoviste`, `psc_pracoviste`, `www`, `vzdelavaci_program_nazev`, `vzdelavaci_program_kod`, `vzdelani_delka_roky`, `vzdelani_forma`, `pocet_studentu_1_rocnik`, `pocet_studentu_2_rocnik`, `pocet_studentu_3_rocnik`, `pocet_studentu_4_rocnik`, `pocet_studentu_celkem`, `pocet_absolventu_2023_2024`, `pocet_prijatych_studentu`

**Hodnoty:**

- `nazev`: Česká lesnická akademie Trutnov - střední šk · Gymnázium, Střední odborná škola a Vyšší odb · Vyšší odborná škola a Střední průmyslová ško · Vyšší odborná škola zdravotnická a Střední z · Vyšší odborná škola zdravotnická, Střední zd · Vyšší odborná škola, Střední škola, Základní · Zemědělská akademie a Gymnázium Hořice - stř
- `pravni_forma`: příspěvková organizace
- `redizo`: 600011682 · 600012069 · 600012905 · 600019772 · 600019829 · 600024016 · 691012458
- `izo`: 110029178 · 110029283 · 110030621 · 110032161 · 150008716 · 151037621 · 181078520
- `typ_cisla_domovniho_pracoviste`: č.p.
- `vzdelavaci_program_nazev`: Diplom.všeobecná sestra · Diplomovaná dětská sestra · Diplomovaná všeobecná sestra · Diplomovaný farmaceutický asistent · Diplomovaný zdravotní laborant · Diplomovaný zubní technik · Lesnictví · Obnova a rozvoj venkova · Strojírenství s CAD,CAM · Tlumočnictví českého znakového jazyka · Účetnictví a daně
- `vzdelavaci_program_kod`: 2341N02 · 4132N01 · 5341N11 · 5341N51 · 5343N11 · 5343N21 · 5344N11 · 6341N22 · 6843N02 · 7533N02
- `vzdelani_delka_roky`: 3 · 3,5
- `vzdelani_forma`: denní forma vzdělávání · kombinovaná forma vzdělávání

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Po%C4%8Dty_student%C5%AF_v_oborech_vy%C5%A1%C5%A1%C3%ADch_odborn%C3%BDch_%C5%A1kol_z%C5%99izovan%C3%BDch_krajem/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/cd2794dd2d334fe5bb360db8956507ae

## DOPRAVA A SILNICE — 12 sad

### Stav povrchu silnic

`n=4888` · linie · aktualizace: unknown

Datová sada s prostorovou lokalizací stavu povrchu silnic ve vlastnictví Královéhradeckého kraje, tedy silnic II. a III. třídy. Zdrojem dat je ÚDRŽBA SILNIC Královéhradeckého kraje a. s.

**Pole:** `stav_sil`, `rok`, `ozn_sil`, `ozn_usek`, `ozn_sil_usek`, `ozn_trida`, `ozn_kat`, `delka`, `nazev_okresu`, `kod_okresu`, `typ_povrchu`

**Hodnoty:**

- `stav_sil`: dobrý · havarijní · nevyhovující · SUPERhavarijní · výborný · vyhovující
- `rok`: 2017 · 2018 · 2019 · 2020 · 2021 · 2022 · 2023 · 2024
- `ozn_kat`: komunikace s místním významem · komunikace významná pro kraj · komunikace významná pro okres · nevýznamná komunikace
- `typ_povrchu`: asfaltobetonový · dlažba · penetrace

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Stav_povrchu_silnic/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/311df195461541069a5690e9491eb092

### Autobusové zastávky VDKHK

`n=4538` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací autobusových zastávek v rámci Veřejné dopravy Královéhradeckého kraje (VDKHK). Nejsou zde obsaženy zastávky MHD. Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `oznaceni`, `stanoviste`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Autobusov%C3%A9_zast%C3%A1vky_IREDO/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/ab928607832141f8bebb36261593107a

### Seznam zaručených přípojů (vyčkávání autobusů na autobusy a vlaky)

`n=3515` · tabulka · aktualizace: irreg

Datová sada obsahuje seznam zaručených přípojů - vyčkávání autobusů na autobusy a vlaky v rámci Veřejné dopravy Královéhradeckého kraje. Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

**Pole:** `linka`, `licence`, `spoj`, `dopravce`, `odjezd`, `zastávka`, `vycka_min`, `typ`, `linka_d`, `licence_d`, `spoj_d`, `dopravce_d`, `z_do_zast`, `nav_cas`

**Hodnoty:**

- `dopravce`: Arriva Autobusy a.s. · AUDIS BUS s.r.o. · BusLine KHK s.r.o. · BusLine LK s.r.o. · BusLine Pardubicko s.r.o. · CAR - TOUR spol. s r.o. · CDS s.r.o. Náchod · ČSAD Liberec, a.s. · KAD BUS spol. s r.o. · KAD,spol. s r.o. · P-transport s.r.o. · UMBRELLA Coach & Buses s.r.o.
- `vycka_min`: 10 · 11 · 12 · 15 · 2 · 20 · 3 · 30 · 4 · 5 · 6 · 7
- `typ`: autobus · vlak
- `dopravce_d`: ARRIVA autobusy a.s. · ARRIVA STŘEDNÍ ČECHY s.r.o. · ARRIVA vlaky s.r.o. · AUDIS BUS s.r.o. · BusLine KHK s.r.o. · BusLine LK s.r.o. · BusLine Pardubicko s.r.o. · CAR - TOUR spol. s r.o. · CDS s.r.o. Náchod · České dráhy, a.s. · KAD BUS spol. s r.o. · KAD,spol. s r.o.

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Seznam_zaru%C4%8Den%C3%BDch_p%C5%99%C3%ADpoj%C5%AF/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/0575292a366649d19580a2389c55fced

### Plán zimní údržby silnic

`n=1660` · linie · aktualizace: annual

Datová sada s prostorovou lokalizací plánu zimní údržby silnic ve vlastnictví Královéhradeckého kraje. Zdrojem dat je ÚDRŽBA SILNIC Královéhradeckého kraje a. s.

**Pole:** `typ_tech`, `ozn_sil`, `ozn_trida`

**Hodnoty:**

- `typ_tech`: chemický posyp · neudržovaný úsek · pluhování · zdrsňující posyp

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pl%C3%A1n_zimn%C3%AD_%C3%BAdr%C5%BEby_silnic/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/4973fa51384f4e7ca5db80e4d9496f03

### Opravy silniční infrastruktury od roku 2016 - liniové

`n=998` · linie · aktualizace: unknown

Datová sada s prostorovou lokalizací oprav silniční infrastruktury od roku 2015 vyjádřených liniově. Jedná se o silnice II. a III. třídy, tedy těch, které jsou ve vlastnictví Královéhradeckého kraje. Zdrojem dat je ÚDRŽBA SILNIC Královéhradeckého kraje a.s.

**Pole:** `rok_real`, `ozn_sil`, `ozn_usek`, `ozn_sil_usek`, `ozn_trida`, `ozn_kat`, `delka`, `nazev_okresu`, `kod_okresu`

**Hodnoty:**

- `rok_real`: 2015 · 2016 · 2017 · 2018 · 2019 · 2020 · 2021 · 2022 · 2023 · 2024
- `ozn_kat`: komunikace s místním významem · komunikace významná pro kraj · komunikace významná pro okres · nevýznamná komunikace

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Opravy_silni%C4%8Dn%C3%AD_infrastruktury_od_roku_2016_liniov%C3%A9/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/229c0cb29b2e4838ac7ff4190c075d7c

### Autobusové linky VDKHK

`n=197` · linie · aktualizace: annual

Datová sada s prostorovou lokalizací autobusových linek využívaných pro Veřejnou dopravu Královéhradeckého kraje (VDKHK). Nejsou zde obsaženy linky MHD a ty, které nejsou součástí dopravy v kompetenci kraje. Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

**Pole:** `trasa`, `linka`, `jizd_rad`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Autobusov%C3%A9_linky_VDKHK/FeatureServer/7`

Portál: https://www.datakhk.cz/datasets/ebc0da36c2bd4fdb9505ddab8b789007

### Železniční stanice a zastávky VDKHK

`n=185` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací železničních stanic a zastávek využívaných pro Veřejnou dopravu Královéhradeckého kraje (VDKHK). Nejedná se o zobrazeních všech železničních stanic a zastávek, které se v Královéhradeckém kraji nacházejí. Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `oznaceni`, `typ`, `oteviraci_doba`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `www`

**Hodnoty:**

- `typ`: Stanice s informačním centrem Veřejné doprav · Stanice Veřejné dopravy Královéhradeckého kr · Zastávka s informačním centrem Veřejné dopra · Zastávka Veřejné dopravy Královéhradeckého k

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/%C5%BDelezni%C4%8Dn%C3%AD_stanice_a_zast%C3%A1vky_VDKHK/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/e64c92bd463e442593e695137e061a35

### Opravy silniční infrastruktury od roku 2016 - bodové

`n=38` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací oprav silniční infrastruktury v letech 2016 až 2022 vyjádřených bodově. Jedná se o silnice II. a III. třídy, tedy těch, které jsou ve vlastnictví Královéhradeckého kraje. Zdrojem dat je ÚDRŽBA SILNIC Královéhradeckého kraje a.s.

**Pole:** `nazev`, `st_id`, `oznaceni_silnice`, `rok_real`, `staniceni`, `zdroj_financi`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`

**Hodnoty:**

- `rok_real`: 2016 · 2017 · 2018 · 2019 · 2020 · 2021 · 2022
- `zdroj_financi`: CIRI · ČR-PL · FRR · FRR/SFDI · FRR+SFDI · IROP · neuvedeno · OPŽP · SFDI

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Opravy_silni%C4%8Dn%C3%AD_infrastruktury_od_roku_2016_bodov%C3%A9/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/e60773d598b14909b749f5e0be403ef1

### Železniční tratě VDKHK

`n=19` · linie · aktualizace: irreg

Datová sada s prostorovou lokalizací vlakových linek v rámci Veřejné dopravy Královéhradeckého kraje (VDKHK). Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

**Pole:** `ozn_linka`, `trasa`, `jizd_rad`

**Hodnoty:**

- `ozn_linka`: 020 · 021 · 023 · 026 · 027 · 030 · 031 · 032 · 040 · 041 · 043 · 044
- `trasa`: Častolovice, žel. st. - Solnice, žel. st. · Chlumec nad Cidlinou, žel. st. - Křinec, žel · Chlumec nad Cidlinou, žel. st. - Trutnov, hl · Choceň, žel. st. - Náchod, žel. st. · Doudleby nad Orlicí, žel. st. - Rokytnice v  · Hradec Králové, hl. n. - Jičín, žel. st. - T · Hradec Králové, hl. n. - Letohrad, žel. st. · Jaroměř, žel. st. - Liberec, hl. n. · Jaroměř, žel. st. - Svoboda nad Úpou, žel. z · Kunčice nad Labem, žel. st. - Vrchlabí, žel. · Lomnice nad Popelkou, žel. zast. - Stará Pak · Mšeno, žel. zast. - Mladá Boleslav, hl. n. -
- `jizd_rad`: https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra · https://www.dopravakhk.cz/vyhledat-jizdni-ra

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Vlakov%C3%A9_linky_IREDO/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/a7f461928f5f49fcaa7d129b6f5ebb71

### Informační centra autobusové dopravy VDKHK

`n=13` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací informačních center autobusové dopravy Veřejné dopravy Královéhradeckého kraje (VDKHK) včetně kontaktů na danou provozovnu. Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

**Pole:** `autobusovy_dopravce`, `oteviraci_doba`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `psc`, `telefon`, `email`, `www`

**Hodnoty:**

- `autobusovy_dopravce`: BusLine KHK s.r.o. · CDS s.r.o. Náchod · Konsorcium Transdev Morava s.r.o. a AUDIS BU · P-transport s.r.o.
- `oteviraci_doba`: Leden - březen, listopad - prosinec: Pondělí · Pondělí - Pátek: 6:30 – 18:00 (mimo svátky) · Pondělí - Pátek: 7:00 – 11:30, 12:00 – 15:00 · Pondělí - Pátek: 7:00 – 12:00, 12:30 – 15:00 · Pondělí - Pátek: 8:00 – 12:00, 13:00 – 17:00 · Pondělí, Středa: 5:55 – 11:00, 11:30 – 15:30 · Pondělí, Středa: 6:00 – 10:10, 10:40 - 12:10 · Pondělí, Středa: 6:00 – 10:30, 11:00 - 11:45 · Pondělí, Středa: 6:00 – 11:30, 12:30 – 18:00 · Pondělí, Středa: 6:30 – 12:00, 12:30 - 17:00 · Úterý - Neděle:  9:00 – 12:00, 12:30 – 16:00

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Informa%C4%8Dn%C3%AD_centra_autobusov%C3%A9_dopravy_VDKHK/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/0d46bed25d274b378af429a53c2d6df0

### Letiště

`n=9` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací letišť na území Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `kod`, `druh`, `draha`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `psc`, `www`, `typ_cisla_domovniho`

**Hodnoty:**

- `nazev`: Letiště Broumov · Letiště Dvůr Králové nad Labem · Letiště Hořice · Letiště Hradec Králové · Letiště Jaroměř · Letiště Jičín · Letiště Nové Město nad Metují · Letiště Velké Poříčí · Letiště Vrchlabí
- `kod`: LKBR · LKDK · LKHC · LKHK · LKJA · LKJC · LKNM · LKVP · LKVR
- `druh`: vnitrostátní veřejné
- `draha`: nezpevněná · zpevněná

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Leti%C5%A1t%C4%9B/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/8f77866fad4848e2aa241d260f557c65

### Plochy pro sportovní létající zařízení

`n=6` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací ploch pro sportovní létající zařízení na území Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `kod`, `druh`, `draha`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `psc`, `www`

**Hodnoty:**

- `nazev`: Plocha pro sportovní létající zařízení Chote · Plocha pro sportovní létající zařízení Horní · Plocha pro sportovní létající zařízení Libáň · Plocha pro sportovní létající zařízení Nácho · Plocha pro sportovní létající zařízení Třebi · Plocha pro sportovní létající zařízení Trutn
- `kod`: LKCHOT · LKLIBA · LKNACH · LKPRIM · LKTRBH · LKTRUT
- `druh`: neveřejná · veřejná
- `draha`: nezpevněná

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Plochy_pro_sportovn%C3%AD_l%C3%A9taj%C3%ADc%C3%AD_za%C5%99%C3%ADzen%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/6b94707caf9141ac9d3a74ac6a6377ac

## ZDRAVOTNICTVÍ — 9 sad

### Praktičtí lékaři pro dospělé

`n=314` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací praktických lékařů pro dospělé. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

**Pole:** `provozovatel`, `ico`, `pravni_forma`, `nazev_oboru`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `provozovatel`: Ordinace praktického lékaře pro dospělé
- `pravni_forma`: Akciová společnost · Fyzická osoba podnikající dle jiných zákonů  · Fyzická osoba podnikající dle živnostenského · Příspěvková organizace · Společnost s ručením omezeným
- `nazev_oboru`: všeobecné praktické lékařství

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Prakti%C4%8Dt%C3%AD_l%C3%A9ka%C5%99i_pro_dosp%C4%9Bl%C3%A9/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/5f3f1b9709db4a0b95f04fb0582f232a

### Zubní lékaři

`n=299` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací zubních lékařů. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

**Pole:** `provozovatel`, `ico`, `pravni_forma`, `nazev_oboru`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `provozovatel`: Ordinace zubního lékaře
- `pravni_forma`: Fyzická osoba podnikající dle jiných zákonů  · Společnost s ručením omezeným
- `nazev_oboru`: zubní lékařství

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Zubn%C3%AD_l%C3%A9ka%C5%99i/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/c5f13e739e184763a7afb709a2e4b198

### Praktičtí lékaři pro děti a dorost

`n=123` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací praktických lékařů pro děti a dorost. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

**Pole:** `provozovatel`, `ico`, `pravni_forma`, `nazev_oboru`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `provozovatel`: Ordinace lékaře pro děti a dorost
- `pravni_forma`: akciová společnost · fyzická osoba podnikající dle jiných zákonů  · příspěvková organizace · společnost s ručením omezeným
- `nazev_oboru`: praktické lékařství pro děti a dorost

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Prakti%C4%8Dt%C3%AD_l%C3%A9ka%C5%99i_pro_d%C4%9Bti_a_dorost/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/a3e7a755cf8840eeb377cb650609c2ac

### Ambulantní gynekologové

`n=83` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací ambulantních gynekologů. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

**Pole:** `provozovatel`, `ico`, `pravni_forma`, `nazev_oboru`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `provozovatel`: Ordinace ambulantního gynekologa
- `pravni_forma`: Akciová společnost · Fyzická osoba podnikající dle jiných zákonů  · Fyzická osoba podnikající dle živnostenského · Společnost s ručením omezeným
- `nazev_oboru`: gynekologie a porodnictví

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Ambulantn%C3%AD_gynekologov%C3%A9/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/6a489fc8b78e4f0ea23f1cf2c83cdeac

### Nemocnice

`n=12` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací nemocnic na území Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `pobocka`, `zrizovatel`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `psc`, `www`

**Hodnoty:**

- `nazev`: Fakultní nemocnice Hradec Králové · Městská nemocnice Dvůr Králové nad Labem, a. · Nemocnice Vrchlabí, s.r.o. · Oblastní nemocnice Jičín, a.s. · Oblastní nemocnice Náchod, a.s. · Oblastní nemocnice Trutnov, a.s.
- `pobocka`: bez pobočky · Nemocnice Broumov · Nemocnice Jaroměř · Nemocnice Jičín · Nemocnice Náchod · Nemocnice Nové Město nad Metují · Nemocnice Nový Bydžov · Nemocnice Rychnov nad Kněžnou · Pracoviště klinické biochemie a diagnostiky 
- `zrizovatel`: Královéhradecký kraj · Ministerstvo zdravotnictví · Penta Hospitals

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Nemocnice/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/590ec69b85e4495a9016c827607fc21f

### Následná lůžková péče

`n=12` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací poskytovatelů následné péče v Královéhradeckém kraji. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `pravni_forma`, `zrizovatel`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `telefon`, `www`

**Hodnoty:**

- `nazev`: AHC Centrum následné péče Trutnov s.r.o. · CELOP s.r.o. · Léčebna dlouhodobě nemocných Opočno · Léčebna pro dlouhodobě nemocné Hradec Králov · Levitovo centrum následné péče Hořice · Městská nemocnice, a.s. · Nemocnice Vrchlabí, s.r.o. · Oblasní charita Červený Kostelec · Oblastní nemocnice Jičín a.s. · Oblastní nemocnice Náchod a.s. · REHAMEDICA Žacléř, a.s.
- `pravni_forma`: akciová společnost · cirkevní právnická osoba · příspěvková organizace · společnost s ručením omezeným
- `zrizovatel`: Biskupství královéhradecké · Královéhradecký kraj · Město Hořice · privátní sektor

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/N%C3%A1sledn%C3%A1_p%C3%A9%C4%8De/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/85d3fd9342ae4f53aeeb383c14fd1cf8

### Lékařská pohotovostní služba

`n=11` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací lékařských pohotovostí pro děti a dospělé (mimo stomatologickou lékařskou pohotovost) v Královéhradeckém kraji. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `cilova_skupina`, `ordinacni_hodiny`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `psc`, `www`

**Hodnoty:**

- `nazev`: Fakultní nemocnice Hradec Králové, Pavilon č · Fakultní nemocnice Hradec Králové, Pavilon č · Oblastní nemocnice Jičín a.s. · Oblastní nemocnice Jičín a.s., Areál nemocni · Oblastní nemocnice Náchod a.s. · Oblastní nemocnice Náchod a.s.,  Nemocnice N · Oblastní nemocnice Náchod a.s., Nemocnice Ry · Oblastní nemocnice Trutnov a.s.
- `cilova_skupina`: děti · dospělí
- `ordinacni_hodiny`: pátek: 14:00 - 20:00, SO, NE, svátek: 08:00  · SO, NE, svátek: 08:00 – 20:00 · SO,NE, svátek: 08:00 – 18:00 · SO,NE, svátek: 08:00 – 20:00 · všední den: 15:30 – 07:00, SO, NE, svátek: n · všední den: 15:30 – 20:00, SO,NE, svátek: 08 · všední den: 16:00 - 20:00, SO, NE, svátek: 0 · všední den: 16:00 – 20:00, SO,NE, svátek: 08 · všední den: 16:00 - 22:00, SO, NE, svátek: 0 · všední den: 16:00 – 22:00, SO,NE, svátek: 08

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/L%C3%A9ka%C5%99sk%C3%A1_pohotovostn%C3%AD_slu%C5%BEba/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/285bc3ecf55b4c718069f05a4471815b

### Lázně

`n=7` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací lázní na území Královéhradeckého kraje. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

**Pole:** `druh_zarizeni`, `poskytovatel`, `ico`, `zrizovatel`, `pozn`, `forma`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `druh_zarizeni`: Lázeňská léčebna
- `poskytovatel`: Lázně 1897, s.r.o. · Lázně Bělohrad a.s. · Státní léčebné lázně Janské Lázně, státní po
- `zrizovatel`: Jiná právnická osoba · Ministersvo zdravotnictví
- `forma`: následná lůžková péče - lázeňská léčebně reh

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/L%C3%A1zn%C4%9B/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/f2e7d14c208f44b99741710aeba77ab1

### Odborné léčebné ústavy

`n=6` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací odborných léčebných ústavů na území Královéhradeckého kraje. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

**Pole:** `druh_zarizeni`, `poskytoval`, `ico`, `zrizovatel`, `forma`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `druh_zarizeni`: Rehabilitační ústav
- `poskytoval`: Lázně Bělohrad a.s. · Sdružení ozdravoven a léčeben okresu Trutnov · Státní léčebné lázně Janské Lázně, státní po
- `zrizovatel`: Jiná právnická osoba · Královéhradecký kraj · Ministerstvo zdravotnictví
- `forma`: následná lůžková péče - rehabilitační a fyzi

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Odborn%C3%A9_l%C3%A9%C4%8Debn%C3%A9_%C3%BAstavy/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/24f82d8f89e54a6898377df6a4cbc831

## IZS A BEZPEČNOST — 3 sad

### Služebny Policie ČR

`n=36` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací služeben a stanic Policie ČR v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_obvodu`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `fax`, `email`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Slu%C5%BEebny_Policie_%C4%8CR/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/042d4ed225654f6b9c1f2b5036887e1e

### Stanice a pracoviště hasičského záchranného sboru

`n=22` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací stanic a pracovišť Hasičského záchranného sboru ČR v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `druh_pracoviste`, `typ_stanice`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `email`

**Hodnoty:**

- `typ_stanice`: C1-B, E-S · C1-B, E-Z · C2-B,F-O · P1-A-Z · P1-B-Z · P1-C-Z

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Stanice_a_pracovi%C5%A1t%C4%9B_hasi%C4%8Dsk%C3%A9ho_z%C3%A1chrann%C3%A9ho_syst%C3%A9mu/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/3ff2f49c7f454870baf51628403ace6e

### Výjezdové základny zdravotnické záchranné služby

`n=16` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací výjezdových základen Zdravotnické záchranné služby Královéhradeckého kraje podle oblasti plošného pokrytí. Zdrojem dat je Zdravotnická záchranná služba Královéhradeckého kraje.

**Pole:** `oblast`, `vyjezdova_zakladna`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `oblast`: Broumov · Dvůr Králové nad Labem · Hořice · Hradec Králové · Jaroměř · Jičín · Náchod · Nová Paka · Nový Bydžov · Opočno · Rychnov nad Kněžnou · Temný Důl
- `vyjezdova_zakladna`: Areál LZS Fakultní nemocnice · Bláhovka · Broumov · Dvůr Králové nad Labem · Hlavní budova · Hořice · Horní Maršov (Temný Důl) · Jaroměř · Jičín · Náchod · Nová Paka · Nový Bydžov

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/V%C3%BDjezdov%C3%A9_z%C3%A1kladny_zdravotnick%C3%A9_z%C3%A1chrann%C3%A9_slu%C5%BEby/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/1c28da5d644445d3a5f4b578ce22ed40

## SOCIÁLNÍ OBLAST — 2 sad

### Poskytovatelé sociálních služeb

`n=461` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací zařízení poskytovatelů sociálních služeb na území Královéhradeckého kraje. Zdrojem dat je Odbor sociálních věcí Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_poskytovatele`, `ico`, `dic`, `typ_soc_sluzby`, `pravni_forma`, `pravni_forma_zrizovatele`, `nazev_zarizeni`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`

**Hodnoty:**

- `pravni_forma`: Akciová společnost · Církve a náboženské společnosti · Fyzická osoba podnikající dle živnostenského · Občanské sdružení · Obec · Obecně prospěšná společnost · Pobočný spolek · Příspěvková organizace zřízená územním samos · Společnost s ručením omezeným · Spolek · Ústav · Zájmové sdružení právnických osob
- `pravni_forma_zrizovatele`: 0 · Bez zřizovatele · Církev · Fyzická osoba nebo skupina osob · Fyzická(é) a právnická(é) osoba(y) · Královehradecký kraj · Ministerstvo práce a sociálních věcí · Obec · Ostatní · Právnická osoba nebo skupina právnických oso

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Poskytovatel%C3%A9_soci%C3%A1ln%C3%ADch_slu%C5%BEeb/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/0a65793445574552b78e7f1f0761ba13

### Veřejné sbírky

`n=86` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací veřejných sbírek probíhajících v Královéhradeckém kraji. Zdrojem dat je Odbor majetkosprávní a krajský živnostenský úřad Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `pravni_forma`, `místo_konani`, `zahajeni`, `ukonceni`, `ucel`, `cislo_bankovniho_uctu`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `pravni_forma`: Akciová společnost · Dobrovolný svazek obcí · Družstvo · Evidované církevní právnické osoby · Nadace · Nadační fond · Obec nebo městská část hlavního města Prahy · Obecně prospěšná společnost · Pobočný spolek · Příspěvková organizace · Spolek · Ústav
- `místo_konani`: Broumovsko · Česká republika · Česká repulika · Hradec Králové, Královéhradecký kraj · Jaroměř · Jičín a okolí · Královéhradecký kraj · město Teplice nad Metují, Královéhradecký kr · město Trutnov · obec Dolní Kalná, obec Horní Kalná · obec Kohoutov · obec Markvartice
- `ukonceni`: 01.06.2023 · 01.09.2023 · 31.12.2024 · na dobu neurčitou

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Ve%C5%99ejn%C3%A9_sb%C3%ADrky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/86ab0a7320d1486fb8b03d7cfb549c5c

## ŽIVOTNÍ PROSTŘEDÍ A RIZIKA — 12 sad

### ÚSES vymezený jako nezávazný koncepční podklad v plánech ÚSES a jiných oborových dokumentech v ZÚR

`n=2924` · plocha · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Dokument`, `Zprac_kdo`, `Zprac_kdy`, `Typ_dok`, `CasH`, `Typ`, `Vloz_BC`, `Oznaceni`, `Varianta`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`, `Poznamka`

**Hodnoty:**

- `Dokument`: Revize ÚSES v KRNAP a jeho OP · ÚSES - nadregionální biocentra - koncepce · ÚZ po vydání aktualizace č. 1,2,3,4,5 ZÚR KH
- `Zprac_kdo`: Ageris - Glos · Ageris - Hájek · Ageris - Ondruška · Ageris - Šimčíková · Ageris s.r.o. · Ateliér Cihlář - Svoboda s.r.o.
- `Typ_dok`: X
- `Typ`: LBC · LBK · NRBC · NRBK · RBC · RBK · RC · RK

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Koncepce_%C3%9ASES/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/769c0fb4f47c411c86f7780e3385b499

### Nadregionální a regionální ÚSES vymezený v ZÚR

`n=526` · plocha · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Kraj`, `Kraj_kod`, `Plati_od`, `RC_ZUR`, `CasH`, `Typ`, `Vloz_BC`, `Oznaceni`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`, `Poznamka`

**Hodnoty:**

- `Typ`: NRBC · NRBK · RBC · RBK

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Nadregion%C3%A1ln%C3%AD_a_region%C3%A1ln%C3%AD_%C3%9ASES_vymezen%C3%BD_v_Z%C3%9AR_KHK/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/89646c784a05469eb7afda20b42c44d3

### Přírodní památky včetně ochranných pásem

`n=170` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací přírodních památek včetně ochranných pásem v Královéhradeckém kraji. Zdrojem dat je Odbor životního prostřední a zemědělství Krajského úřadu Královéhradeckého kraje.

**Pole:** `NAZEV`, `KOD`, `KAT`, `ROZL`, `OP_TYP`, `IUCN`, `PODKLAD`, `ZPRACOVAL`, `VZNIK`, `ZMENA_G`, `ZMENA_T`, `POZNAMKA`, `KAT_OP`

**Hodnoty:**

- `KAT`: OP · PP
- `OP_TYP`: VYH · ZAK
- `IUCN`: III · IV · V
- `PODKLAD`: GP, KMW · KM · KMW · LB · LB, KMW_4 · LB_3 · LB_8 · LTS
- `ZPRACOVAL`: AOPK ČR - OS · AOPK ČR - PK · AOPK ČR - PZ · KS HKA · KÚ HKK · KÚKHK · OKR Jičín · SCHKO BR · SCHKO OH · Správa KRNAP
- `POZNAMKA`: část parcely č. 577/1, odpovídá parcele č. 5 · Chybné očíslování parcel v nařízeníč. 2/1999 · data AOPK, dle VYH (hranice upravena 23.1.20 · p.č.168/2 vyhlášena dle map. příloh 2 a 3 Na · překryv s OP 815 Zebín; na původním PP vyhlá · rozdíl mezi lomovými body a mapovou přílohou · Umístění středu na [-599385;-1039995] po zam · ve vyhlášce nejsou uvedené parcely · zákres neodpovídá vyhlášce
- `KAT_OP`: PP_OP · PP_PP

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99%C3%ADrodn%C3%AD_pam%C3%A1tky_v%C4%8Detn%C4%9B_ochrann%C3%BDch_p%C3%A1sem/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/fa46ed246ad9405ab093e74ff580ea78

### Přírodní zajímavosti

`n=128` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací přírodních zajímavostí na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99%C3%ADrodn%C3%AD_zaj%C3%ADmavosti/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/0d629b7fd2c34af8a3e2ba2196aa03cc

### Záplavové území pětileté vody

`n=78` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací záplavového území pětileté vody. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje. Data stažena zVodohospodářského informačního portálu VODA https://voda.gov.cz/ Průběh polygonů záplavových území v této vrstvě je pouze ORIENTAČNÍ, jedná se o kompilaci dat jednotlivých dílčích záplavových území, v rámci které byly provedeny úpravy (vyhl

**Pole:** `nazev_tok`, `id_tok`, `nazev_tok_vyssiho_radu`, `id_tok_vyssiho_radu`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1plavov%C3%A9_%C3%BAzem%C3%AD_p%C4%9Btilet%C3%A9_vody/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/0445ecebd0624b588488700038715bf5

### Záplavové území dvacetileté vody

`n=77` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací záplavového území dvacetileté vody. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje. Data stažena zVodohospodářského informačního portálu VODA https://voda.gov.cz/ Průběh polygonů záplavových území v této vrstvě je pouze ORIENTAČNÍ, jedná se o kompilaci dat jednotlivých dílčích záplavových území, v rámci které byly provedeny úpravy (v

**Pole:** `nazev_tok`, `id_tok`, `nazev_tok_vyssiho_radu`, `id_tok_vyssiho_radu`

**Hodnoty:**

- `nazev_tok_vyssiho_radu`: Bělá · Bystřice · Cidlina · Čistá · Dědina · Divoká Orlice · Dunajka · Jizera · Kněžná · Labe · Labský náhon · Ličná

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1plavov%C3%A9_%C3%BAzem%C3%AD_dvacetilet%C3%A9_vody/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/8a81ac1b830d42249ed3b38708e95243

### Záplavové území stoleté vody

`n=65` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací záplavového území stoleté vody. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje. Data stažena zVodohospodářského informačního portálu VODA https://voda.gov.cz/ Průběh polygonů záplavových území v této vrstvě je pouze ORIENTAČNÍ, jedná se o kompilaci dat jednotlivých dílčích záplavových území, v rámci které byly provedeny úpravy (vyhla

**Pole:** `nazev_tok`, `id_tok`, `nazev_tok_vyssiho_radu`, `id_tok_vyssiho_radu`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1plavov%C3%A9_%C3%BAzem%C3%AD_stolet%C3%A9_vody/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/7d93751a3221478ca39a6e1ca6578047

### Aktivní zóna záplavového území stoleté vody

`n=63` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací aktivní zóny záplavového území stoleté vody. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje. Data stažena zVodohospodářského informačního portálu VODA https://voda.gov.cz/ Průběh polygonů záplavových území v této vrstvě je pouze ORIENTAČNÍ, jedná se o kompilaci dat jednotlivých dílčích záplavových území, v rámci které byly provedeny 

**Pole:** `nazev_tok`, `id_tok`, `nazev_tok_vyssiho_radu`, `id_tok_vyssiho_radu`

**Hodnoty:**

- `nazev_tok_vyssiho_radu`: Bělá · Bystřice · Cidlina · Čistá · Dědina · Divoká Orlice · Dunajka · Jizera · Kněžná · Labe · Labský náhon · Ličná

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Aktivn%C3%AD_z%C3%B3na_z%C3%A1plavov%C3%A9ho_%C3%BAzem%C3%AD_stolet%C3%A9_vody/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/3861250b6faa45e297b7116a2f0b51cd

### Přírodní rezervace včetně ochranných pásem

`n=61` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací přírodních rezervací včetně ochranných pásem v Královéhradeckém kraji. Zdrojem dat je Odbor životního prostřední a zemědělství Krajského úřadu Královéhradeckého kraje.

**Pole:** `NAZEV`, `KOD`, `KAT`, `ROZL`, `OP_TYP`, `IUCN`, `PODKLAD`, `ZPRACOVAL`, `VZNIK`, `ZMENA_G`, `ZMENA_T`, `POZNAMKA`, `KAT_OP`

**Hodnoty:**

- `KAT`: OP · PR
- `OP_TYP`: VYH · ZAK
- `IUCN`: IV
- `PODKLAD`: KM · KMW · KMW, LMO · LB · LB, KMW_8 · LB_3 · LB_8
- `ZPRACOVAL`: AOPK ČR - OS · AOPK ČR - PK · AOPK ČR - PZ · KS HKA · KÚ HKK · KÚ KHK · MŽP · SCHKO BR · SCHKO CR · SCHKO OH
- `POZNAMKA`: hranice aktualizovana_25_03_2019 · původní lesnická mapa není k dispozici
- `KAT_OP`: PR_OP · PR_PR

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99%C3%ADrodn%C3%AD_rezervace_v%C4%8Detn%C4%9B_ochrann%C3%BDch_p%C3%A1sem/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/75143118a8a14213b51fcf9f5f27fc14

### Záplavové území pětisetleté vody

`n=41` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací záplavového území pětisetleté vody vody. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje. Data stažena zVodohospodářského informačního portálu VODA https://voda.gov.cz/ Průběh polygonů záplavových území v této vrstvě je pouze ORIENTAČNÍ, jedná se o kompilaci dat jednotlivých dílčích záplavových území, v rámci které byly provedeny úpra

**Pole:** `nazev_tok`, `id_tok`, `nazev_tok_vyssiho_radu`, `id_tok_vyssiho_radu`

**Hodnoty:**

- `nazev_tok_vyssiho_radu`: Cidlina · Dědina · Divoká Orlice · Dunajka · Labe · Ličná · Metuje · Oleška · Orlice · Stěnava · Úpa · Zdobnice

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1plavov%C3%A9_%C3%BAzem%C3%AD_p%C4%9Btisetlet%C3%A9_vody/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/39acd9513e8d4e6e9542fea0567d5259

### Vymezení krajin, ke kterým jsou stanoveny cílové kvality v ZÚR

`n=25` · plocha · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Kraj`, `Kraj_kod`, `Plati_od`, `RC_ZUR`, `Ident`, `CilKvality`, `Odkazy`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`, `Poznamka`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Vymezen%C3%AD_krajin/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/d9f3ee622a1247bd92249127371b6ee5

### Přírodní parky

`n=5` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací přírodních parků na území Královéhradeckého kraje. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `dokument`, `vyhlasovatel`, `prohlaseni`

**Hodnoty:**

- `nazev`: Hrádeček · Les Včelný · Orlice · Sýkornice · Údolí Rokytenky a Hvězdné
- `dokument`: nařízení č. 4/2002 · nařízení č. 4/95; nařízení č. 5/96 · nařízení č. KP/371/04/96 · nařízení, kt.se zřiz. PP Hrádeček · vyhláška č. 6
- `vyhlasovatel`: Okresní národní výbor v Jičíně · Okresní úřad Hradec Králové, Magistrát města · Okresní úřad Rychnov nad Kněžnou · Okresní úřad Trutnov
- `prohlaseni`: 17.4.2000 · 19.12.1995 · 27.12.2002 · 29.4.1996 · 31.5.1984

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99%C3%ADrodn%C3%AD_parky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/1adad0a9ee684b23a84c8613f8e5156e

## ÚZEMNÍ PLÁNOVÁNÍ, DOTACE A ROZVOJ — 21 sad

### Plochy a koridory pro veřejně prospěšné stavby, veřejně prospěšná opatření a asanace dle ZÚR – plošné

`n=608` · plocha · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Kraj`, `Kraj_kod`, `Plati_od`, `RC_ZUR`, `Ident`, `Typ`, `Ucel`, `Popis`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`, `Poznamka`

**Hodnoty:**

- `Typ`: V
- `Ucel`: D · T · U

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/VpsVpoAs_ZUR_p/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/3d2f89fd21d0450686d8933fac9a556e

### Hospodářsky a sociálně ohrožené oblasti

`n=448` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací hospodářsky a sociálně ohrožených oblastí na území Královéhradeckého kraje. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `hsoo`, `urov_ohr`, `nazev_obce`, `kod_obce`, `nazev_pou`, `kod_pou`, `nazev_orp`, `kod_orp`, `nazev_okresu`, `kod_okresu`, `turist_reg`

**Hodnoty:**

- `hsoo`: ano · ne
- `urov_ohr`: nízká míra ohrožení · střední míra ohrožení · vysoká míra ohrožení
- `turist_reg`: Broumovsko · Český ráj · Hradecko · Kladské pomezí · Krkonoše · Orlické hory a Podorlicko · Podkrkonoší

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hospod%C3%A1%C5%99sky_a_soci%C3%A1ln%C4%9B_ohro%C5%BEen%C3%A9_oblasti/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/2b8f4c0b14934ab5ad6f9d88af314808

### Podíl obydlených domů dle připojení na kanalizaci

`n=448` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací podílu obydlených domů dle připojení na kanalizaci v obcích na území Královéhradeckého kraje. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_obce`, `kod_obce`, `kanal_sit`, `cov`, `zumpa`, `bez_a_nezj`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pod%C3%ADl_obydlen%C3%BDch_dom%C5%AF_dle_p%C5%99ipojen%C3%AD_na_kanalizaci/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/c398bd20e9da4cb6a8ff790a4d84b3c7

### Počet obydlených domů dle připojení na kanalizaci v obcích

`n=448` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací počtu obydlených domů dle připojení na kanalizaci v obcích na území Královéhradeckého kraje. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_obce`, `kod_obce`, `kanal_sit`, `cov`, `zumpa`, `bez_a_nezj`, `celkem`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Po%C4%8Det_obydlen%C3%BDch_dom%C5%AF_dle_p%C5%99ipojen%C3%AD_na_kanalizaci_v_obc%C3%ADch/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/b6240a2af1584a40b4f1315bc62f2bb1

### Schválené individuální a programové dotace do ORP v letech 2021 - 2025

`n=15` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací schválených individuálních a programových dotací Královéhradeckého kraje do území správních obvodů obcí s rozšířenou působností v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_orp`, `kod_orp`, `poc_dotaci`, `finance`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_a_programov%C3%A9_dotace_do_ORP/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/ea6f7adad2b640ee9e4a07e2979000d4

### Schválené individuální dotace do ORP v letech 2021 - 2025

`n=15` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací schválených individuálních dotací Královéhradeckého kraje do území správních obvodů obcí s rozšířenou působností v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_orp`, `kod_orp`, `poc_dotaci`, `finance`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_dotace_do_ORP/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/4fcd25394ac141f899aaba4ac75e108c

### Schválené programové dotace do ORP v letech 2021 - 2025

`n=15` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací schválených programových dotací Královéhradeckého kraje do území správních obvodů obcí s rozšířenou působností v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_orp`, `kod_orp`, `poc_dotaci`, `finance`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_programov%C3%A9_dotace_do_ORP/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/f41559b6c6b54f42a97caf8b45371e54

### Plochy a koridory pro veřejně prospěšné stavby, veřejně prospěšná opatření a asanace dle ZÚR – bodové

`n=13` · bod · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Kraj`, `Kraj_kod`, `Plati_od`, `RC_ZUR`, `Ident`, `Typ`, `Ucel`, `Popis`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`, `Poznamka`

**Hodnoty:**

- `Typ`: V
- `Ucel`: R · T
- `Popis`: Melounka, Všestary - výstavba poldru · oznaceni v UP: PP-01 · oznaceni v UP: PP-02 · oznaceni v UP: PP-03 · poldr Skalice · RS Česká Skalice · Suchá nádrž Mlýnec · transformovna TR 110/35 kV Broumov · transformovna TR 110/35 kV Horní Maršov · transformovna TR 110/35 kV Hradec Králové –  · transformovna TR 110/35 kV Jaroměř · transformovna TR 110/35kV Hořice

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Plochy_a_koridory/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/1c47f9e8b72e48a98c379e9a2b82871b

### Územní rezervy dle ZÚR

`n=9` · plocha · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Kraj`, `Kraj_kod`, `Plati_od`, `RC_ZUR`, `Ident`, `Typ_skup`, `Popis`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`, `Poznamka`

**Hodnoty:**

- `Typ_skup`: D · W
- `Popis`: Babí (na vodním toku Babí potok) · Fořt (na vodním toku Čistá) · Lukavice (na vodním toku Kněžná) · Modernizace,elektrizace trati Jaroměř-Náchod · Pěčín (na vodním toku Zdobnice) · silnice I/11 – v prostoru Kostelce nad Orlic · silnice II/298 – v prostoru Očelic a Ledcí · silnice II/303 – v prostoru Pěkova · žamberk (na vodním toku Rokytenka)

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/%C3%9Azemn%C3%AD_rezervy_dle_Z%C3%9AR/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/a2d0970a92b6452899730e90a1ce5e29

### Plochy a koridory pro veřejně prospěšné stavby, veřejně prospěšná opatření a asanace dle ZÚR – liniové

`n=6` · linie · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Kraj`, `Kraj_kod`, `Plati_od`, `RC_ZUR`, `Ident`, `Typ`, `Ucel`, `Popis`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`

**Hodnoty:**

- `Ucel`: R
- `Popis`: Broumov – Velká Ves · Dědina, Mělčany, suchá retenční nádrž · Kvasiny, opěrná zeď vodního toku Bělá · suchá nádrž Rusek - Bukovina · Suchá nádrž Žireč (PPO13)

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/VpsVpoAs_ZUR_l/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/61acbd8d632d4dae854cdf6ae7bf48fc

### Schválené individuální a programové dotace do okresů v letech 2021 - 2025

`n=5` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací schválených individuálních a programových dotací Královéhradeckého kraje do území okresů v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_okresu`, `kod_okresu`, `poc_dotaci`, `finance`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_a_programov%C3%A9_dotace_do_okres%C5%AF/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/6630227b83774e5b84e5a5105d37c2ec

### Schválené individuální dotace do okresů v letech 2021 - 2025

`n=5` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací schválených individuálních dotací Královéhradeckého kraje do území okresů v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_okresu`, `kod_okresu`, `poc_dotaci`, `finance`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_dotace_do_okres%C5%AF/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/0f2746acd5b94749858240b3dc828af5

### Schválené programové dotace do okresů v letech 2021 - 2025

`n=5` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací schválených programových dotací Královéhradeckého kraje do území okresů v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_okresu`, `kod_okresu`, `poc_dotaci`, `finance`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_programov%C3%A9_dotace_do_okres%C5%AF/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/42d36c17bbac4b6f9f4bf45c1e1dcb88

### Vymezení rozvojových os v ZÚR

`n=5` · plocha · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Ident`, `Nazev`, `Vyznam`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`, `Poznamka`

**Hodnoty:**

- `Nazev`: Rozvojová osa Hradec Králové-Jičín-Liberecký · Rozvojová osa Jičín-Středočeský kraj · Rozvojová osa Kostelec nad Orlicí-Vamberk-Pa · Rozvojová osa Náchodsko-Rychnov nad Kněžnou · Rozvojová osa Praha-Hradec Králové/Pardubice
- `Vyznam`: R · S
- `Poznamka`: zpřesnění vymezení z PÚR ČR; celý název: Roz

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Vymezen%C3%AD_rozvojov%C3%BDch_os_v_Z%C3%9AR/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/6a883a213b934395a0b9eaff9698daa1

### Vymezení specifických oblastí v ZÚR

`n=5` · plocha · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Ident`, `Nazev`, `Typ`, `Vyznam`, `Popis`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`, `Poznamka`

**Hodnoty:**

- `Nazev`: Specifická oblast - sucho · Specifická oblast Broumovsko · Specifická oblast jičínsko · Specifická oblast Krkonoše - Jizerské hory · Specifická oblast Orlické hory
- `Typ`: P · S
- `Vyznam`: R · S
- `Poznamka`: řešení A5 ZUR · zpřesnění vymezení z PÚR ČR

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Vymezen%C3%AD_specifick%C3%BDch_oblast%C3%AD_v_Z%C3%9AR/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/8d9e15cc77b048f181c2d9ddba6fa8be

### Vymezení rozvojových oblastí v ZÚR

`n=3` · plocha · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Ident`, `Nazev`, `Vyznam`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`, `Poznamka`

**Hodnoty:**

- `Nazev`: Ropzvojová oblast Náchodsko · Rozvojová oblast Hradec Králové/Pardubice · Rozvojová oblast Vrchlabí
- `Vyznam`: R · S
- `Poznamka`: zpřesnění vymezení z PÚR ČR

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Vymezen%C3%AD_rozvojov%C3%BDch_oblast%C3%AD_v_Z%C3%9AR/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/f2e4e8c2701d44e482ae33b243de3bc5

### Významné oborové záměry ekonomického rozvoje v ZÚR

`n=2` · bod · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

**Pole:** `Ident`, `Typ`, `Popis`, `Jev`, `Pasport_Id`, `Zdroj`, `Dat_Aktual`, `Poznamka`

**Hodnoty:**

- `Typ`: P
- `Popis`: plocha průmyslové zóny Kvasiny - Rychnov nad · plocha průmyslové zóny Vrchlabí

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/V%C3%BDznamn%C3%A9_oborov%C3%A9_z%C3%A1m%C4%9Bry_ekonomick%C3%A9ho_rozvoje_v_Z%C3%9AR/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/dd836a48467c42bdb23303b4c590daa5

### Plochy jejichž využití je podmíněno zpracováním regulačního plánu nebo územní studie v ZÚR

`n=1` · plocha · aktualizace: irreg

Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR). Jedná se o nově vytvořenou vrstvu, jejíž přesná definice bude upřesněna v aktualizované verzi datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK.

**Pole:** `CISLO`, `TYP`, `Jev`, `POPIS`

**Hodnoty:**

- `TYP`: US
- `POPIS`: územní studie jihozápadního obchvatu města N

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Plochy_jejich%C5%BE_vyu%C5%BEit%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/28e8746d8e1c4049a8140092bd7f4d8d

### Schválené individuální a programové dotace do kraje v letech 2021 - 2025

`n=1` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací schválených individuálních a programových dotací Královéhradeckého kraje do území Královéhradeckého kraje v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `poc_dotaci`, `finance`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_a_programov%C3%A9_dotace_do_kraje/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/4e000240e332439b82f1f7d63d9d087d

### Schválené individuální dotace do kraje v letech 2021 - 2025

`n=1` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací schválených individuálních dotací Královéhradeckého kraje do území Královéhradeckého kraje v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `poc_dotaci`, `finance`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_dotace_do_kraje/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/9c9f2a398b4241a9b97898826f3dd4fd

### Schválené programové dotace do kraje v letech 2021 - 2025

`n=1` · plocha · aktualizace: unknown

Datová sada s prostorovou lokalizací schválených programových dotací Královéhradeckého kraje do území Královéhradeckého kraje v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `poc_dotaci`, `finance`, `poc_podanych_zad`, `uspesnost`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_programov%C3%A9_dotace_do_kraje/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/651132ee12214f63b40e76253ca8e3ea

## EKONOMIKA A PRÁCE — 6 sad

### Inovativní firmy

`n=211` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací inovativních firem na území Královéhradeckého kraje. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest. Vysvětlivky k datové sadě jsou k dispozici zde

**Pole:** `nazev`, `ico`, `pravni_forma`, `hlavni_nace_popis`, `hlavni_nace_kod`, `kategorie_poctu_zamestancu`, `kategorie_obratu`, `zdroj`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_popisne`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `pravni_forma`: Akciová společnost · Družstvo · N/A · Příspěvková organizace · Společnost s ručením omezeným · Veřejná obchodní společnost · Zemědělský podnikatel - fyzická osoba
- `kategorie_poctu_zamestancu`: 1 - 5 zaměstnanců · 10 - 19 zaměstnanců · 100 - 199 zaměstnanců · 20 - 24 zaměstnanci · 200 - 249 zaměstnanců · 2000 - 2499 zaměstnanců · 25 - 49 zaměstnanců · 250 - 499 zaměstnanců · 50 - 99 zaměstnanců · 500 - 999 zaměstnanců · 5000 - 9999 zaměstnanců · 6 - 9 zaměstnanců
- `kategorie_obratu`: 1 - 199 999 Kč · 1 000 000 - 2 999 999 Kč · 1 000 000 000 - 1 499 999 999 Kč · 1 500 000 000 Kč a více · 10 000 000 - 29 999 999 Kč · 100 000 000 - 199 999 999 Kč · 200 000 - 499 999 Kč · 200 000 000 - 299 999 999 Kč · 3 000 000 - 4 999 999 Kč · 30 000 000 - 59 999 999 Kč · 300 000 000 - 499 999 999 Kč · 5 000 000 - 9 999 999 Kč
- `zdroj`: IS VaVaI · NACE_72 · PCT · potencial · PV_PUV · WiW

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Inovativn%C3%AD_firmy/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/e3f61ebe1c354fe5996a00159c1bbea5

### Investiční pobídky

`n=84` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací investičních pobídek na území Královéhradeckého kraje. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

**Pole:** `nazev_spolecnosti`, `cislo_pobidky`, `ico`, `pravni_forma`, `cz_nace`, `sektor`, `druh_investicni_akce`, `země_puvodu_skupiny`, `země_puvodu_zadatele`, `investice_mil__EUR`, `investice_mil__USD`, `investice_mil__CZK`, `nova_pm`, `dane`, `rekvalifikace`, `mira_verejne_podpory`, `strop_verejne_podpory_mil__CZK`, `rok_podani_zameru`, `rok_rozhodnuti`, `mpsv`, `rozhodnuti_zruseni_odstoupeni`, `investor_investuje_bez_pobidek`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `pravni_forma`: akciová společnost · společnost s ručením omezeným
- `sektor`: chemický a farmaceutický · dřevozpracující a papírenský · elektronický a elektrotechnický · gumárenský a plastikářský · kovodělný a kovozpracující · ostatní · potravinářský · strojírenský · textilní · výroba dopravních prostředků
- `druh_investicni_akce`: technologické centrum · výroba
- `země_puvodu_skupiny`: Austrálie · Česká republika · Japonsko · Německo · neuvedeno · Nizozemsko
- `země_puvodu_zadatele`: Belgie · Česká republika · Francie · Itálie · Japonsko · Lucembursko · Německo · Nizozemí · Polsko · Rakousko · USA · Velká Británie a Severní Irsko
- `dane`: ano
- `rekvalifikace`: ano · neuvedeno
- `rok_podani_zameru`: 1998 · 1999 · 2000 · 2001 · 2003 · 2004 · 2005 · 2006 · 2007 · 2009 · 2011 · 2012
- `rok_rozhodnuti`: 1998 · 1999 · 2000 · 2001 · 2002 · 2005 · 2006 · 2007 · 2008 · 2009 · 2011 · 2012
- `mpsv`: ano · ne
- `rozhodnuti_zruseni_odstoupeni`: ano · neuvedeno

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Investi%C4%8Dn%C3%AD_pob%C3%ADdky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/d537a3ae646e4819b47cb37cbbec60a7

### Právnické osoby v klastrech

`n=31` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací právnických osob v klastrech. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

**Pole:** `nazev_klastru`, `ico_klastru`, `nazev_firmy`, `ico_firmy`, `pravni_forma`, `vznik`, `kategorie_poctu_zamestnancu`, `cz_nadace`, `kod_cz_nadace`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `nazev_klastru`: Czech Stone Cluster, družstvo · Klastr výrobců obalů, družstvo (OMNIPACK) · Technologický klastr, z.s.
- `pravni_forma`: Akciová společnost · Fyzická osoba podnikající dle jiných zákonů  · Fyzická osoba podnikající dle živnostenského · Obec (obecní úřad) · Příspěvková organizace · Společnost s ručením omezeným · Ústav
- `kategorie_poctu_zamestnancu`: 1 - 5 · 10 - 19 · 100 - 199 · 20 - 24 · 25 - 49 · 50 - 99 · 500 - 999 · 6 - 9 · Bez zaměstnanců · Neuvedeno

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pr%C3%A1vnick%C3%A9_osoby_v_klastrech/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/38402dfafbf4467cbb6fa29c546ad501

### Průmyslové zóny

`n=14` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací průmyslových zón. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

**Pole:** `nazev`, `rozloha`, `verejna_podpora`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`

**Hodnoty:**

- `nazev`: Hořice · JARO · Jičín I · Jičín II · Krkonošská · Letiště · Lipovka · Solnice - Kvasiny · Solnice - Kvasiny - rozšíření · Stolín · Vrchlabí - JIH · Vysokov
- `verejna_podpora`: ano · ne

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pr%C5%AFmyslov%C3%A9_z%C3%B3ny/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/10547be46d01470186fe5d8d1a8ca758

### Inovační infrastruktury

`n=8` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací inovačních infrastruktur. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

**Pole:** `nazev`, `ico`, `pravni_forma`, `vlastnictvi`, `vedeckotechnicky_park`, `typ_inovacniho_centra`, `cowork`, `otevrena_dilna`, `aktivni`, `pasivni`, `nefunkcni`, `dotace_op_mpo`, `dotace_op_msmt`, `dotace_op_mpsv`, `dotace_op_praha`, `dotace_kraje`, `dotace_jine`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `nazev`: Centrum inovací a podnikání Trutnov · Coworking TCHK · CTTV INOTEX (Centrum textilních technologií  · Liána Coworking · Podnikatelské centrum Hradec Králové · Spolupracovna Bio Central · Technologické centrum Hradec Králové · Technologické centrum Tesla
- `pravni_forma`: společnost s ručením omezeným · spolek · ústav
- `vlastnictvi`: soukromé · veřejné
- `vedeckotechnicky_park`: ano · ne
- `typ_inovacniho_centra`: ano · ne
- `cowork`: ano · ne
- `otevrena_dilna`: ne
- `aktivni`: ano · ne
- `pasivni`: ano · ne
- `nefunkcni`: ne
- `dotace_op_mpo`: ano · ne
- `dotace_op_msmt`: ne
- `dotace_op_mpsv`: ne
- `dotace_op_praha`: ne
- `dotace_kraje`: ne
- `dotace_jine`: ano · ne

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Inova%C4%8Dn%C3%AD_infrastruktury/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/36498157e8984a039af973cd8406af63

### Klastry Národní klastrové asociace

`n=3` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací klastrů v evidenci Národní klastrové asociace. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

**Pole:** `nazev`, `ico`, `pravni_forma`, `vznik`, `pocet_firem`, `cz_nadace`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `nazev`: Czech Stone Cluster, družstvo · Klastr výrobců obalů, družstvo (OMNIPACK) · Technologický klastr, z.s.
- `pravni_forma`: Družstvo · Spolek
- `vznik`: 16.4.2016 · 23.3.2007 · 4.11.2005
- `cz_nadace`: Činnosti ostatních organizací sdružujících o · Ostatní poradenství v oblasti podnikání a ří · Ostatní specializované stavební činnosti j. 

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Klastry_N%C3%A1rodn%C3%AD_klastrov%C3%A9_asociace/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/95055055d7744702b33706846e513628

## OBCE A SPRÁVA — 11 sad

### Administrativní členění obcí

`n=448` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací obcí na území Královéhradeckého kraje, které jsou začleněny do administrativního členění České republiky včetně kódů dle Českého statistického úřadu a do turistického regionu dle agentury CzechTourism. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_obce`, `kod_obce`, `nazev_pou`, `kod_pou`, `nazev_orp`, `kod_orp`, `nazev_okresu`, `kod_okresu`, `turist_reg`

**Hodnoty:**

- `turist_reg`: Broumovsko · Český ráj · Hradecko · Kladské pomezí · Krkonoše · Orlické hory a Podorlicko · Podkrkonoší

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Administrativn%C3%AD_%C4%8Dlen%C4%9Bn%C3%AD_obc%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/17ef6289f56048d3adeac5425f7e9c41

### Adresář obcí

`n=448` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací adresářů na jednotlivé obecní/městské úřady obcí v Královéhradeckém kraji. Zdrojem dat je Odbor kancelář ředitele Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_obce`, `kod_obce`, `ds`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `posta`, `www`, `telefon_1`, `telefon_2`, `telefon_3`, `telefon_4`, `email_1`, `email_2`, `email_3`, `email_4`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Adres%C3%A1%C5%99_obc%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/3ec5df3a89d7470ea40330baadca74f4

### Hranice obcí v Královéhradeckém kraji

`n=448` · plocha · aktualizace: irreg

Datová sada s prostorovým vymezením obcí v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_obce`, `kod_obce`, `pozn`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hranice_obc%C3%AD_v_Kr%C3%A1lov%C3%A9hradeck%C3%A9m_kraji/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/dae76ca859cd46d4b54bf99d5d66fcf5

### Organizace Královéhradeckého kraje

`n=126` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací organizací založených nebo zřízených Královéhradeckým krajem. Zdrojem dat je Odbor analýz a podpory řízení Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `pravni_forma`, `odbor`, `typ`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www_1`, `www_2`, `fotografie_zdroj`, `fotografie_1`, `fotografie_2`

**Hodnoty:**

- `pravni_forma`: akciová společnost · obecně prospěšná společnost · příspěvková organizace · společnost s ručením omezeným · veřejná výzkumná instituce
- `odbor`: dopravy a silničního hospodářství · kultury a památkové péče · majetkosprávní a krajský živnostenský úřad · regionálního rozvoje, grantů a dotací · školství · sociálních věcí · zdravotnictví · životního prostředí a zemědělství
- `typ`: cestovní ruch · domovy pro seniory a domovy se zvláštním rež · jiná zařízení sociálních služeb · kulturní zařízení · léčebny pro dlouhodobě nemocné · ostatní · školská zařízení · speciální školy · spoluzakladatel Královéhradecký kraj · střední školy a vyšší odborné školy

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Organizace_Kr%C3%A1lov%C3%A9hradeck%C3%A9ho_kraje/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/ee4afea713fe47829462a50e98e44a05

### Hranice správních obvodů obcí s pověřeným obecním úřadem v Královéhradeckém kraji

`n=35` · plocha · aktualizace: irreg

Datová sada s prostorovým vymezením správních obvodů obcí s pověřeným obecním úřadem v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_pou`, `kod_pou`, `pozn`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hranice_spr%C3%A1vn%C3%ADch_obvod%C5%AF_obc%C3%AD_s_pov%C4%9B%C5%99en%C3%BDm_obecn%C3%ADm_%C3%BA%C5%99adem_v_Kr%C3%A1lov%C3%A9hradeck%C3%A9m_kraji/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/3c3c3e7efd49417390af27d90e1b099b

### Dobrovolné svazky obcí

`n=32` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací dobrovolných svazků obcí na území Královéhradeckého kraje. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Dobrovoln%C3%A9_svazky_obc%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/3bea9f6296d74b668baff1c1d6eb0af5

### Místní akční skupiny

`n=17` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací místních akčních skupin na území Královéhradeckého kraje. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `por_cislo`

**Hodnoty:**

- `nazev`: Hradecký venkov o.p.s. · MAS Brána do Českého ráje, z.s. · MAS Broumovsko+, z. s. · MAS Holicko, o.p.s. · MAS Královédvorsko, z. s. · MAS Podchlumí, z. s. · MAS Region Kunětické hory, z.s. · Místní akční skupina Království - Jestřebí h · Místní akční skupina Krkonoše, z. s. · Místní akční skupina Mezi Úpou a Metují, z.  · Místní akční skupina POHODA venkova, z.s. · Místní akční skupina Stolové hory, z. s.

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/M%C3%ADstn%C3%AD_ak%C4%8Dn%C3%AD_skupiny/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/ddfc69c2044646638778da3314a801ad

### Hranice správních obvodů obcí s rozšířenou působností v Královéhradeckém kraji

`n=15` · plocha · aktualizace: irreg

Datová sada s prostorovým vymezením správních obvodů obcí s rozšířenou působností v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_orp`, `kod_orp`, `pozn`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hranice_spr%C3%A1vn%C3%ADch_obvod%C5%AF_obc%C3%AD_s_roz%C5%A1%C3%AD%C5%99enou_p%C5%AFsobnost%C3%AD_v_Kr%C3%A1lov%C3%A9hradeck%C3%A9m_kraji/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/41cf8498bbe944db8f5b6457fb39e5b9

### Turistické regiony

`n=7` · plocha · aktualizace: irreg

Datová sada s prostorovým vymezením turistických regionů v Královéhradeckém kraji vymezených dle agentury CzechTourism. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `poznamka`

**Hodnoty:**

- `nazev`: Broumovsko · Český ráj · Hradecko · Kladské pomezí · Krkonoše · Orlické hory a Podorlicko · Podkrkonoší
- `poznamka`: Plošné vymezení turistického regiony Broumov · Plošné vymezení turistického regiony Český r · Plošné vymezení turistického regiony Hradeck · Plošné vymezení turistického regiony Kladské · Plošné vymezení turistického regiony Krkonoš · Plošné vymezení turistického regiony Orlické · Plošné vymezení turistického regiony Podkrko

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Turistick%C3%A9_regiony/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/e94f5a8cb916419d92a8d0bd35029ea7

### Hranice okresů v Královéhradeckém kraji

`n=5` · plocha · aktualizace: irreg

Datová sada s prostorovým vymezením okresů v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev_okresu`, `kod_okresu`, `pozn`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hranice_okres%C5%AF_v_Kr%C3%A1lov%C3%A9hradeck%C3%A9m_kraji/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/e1e7b8f72f974591881e7cc68d8dc5f0

### Hranice Královéhradeckého kraje

`n=1` · plocha · aktualizace: irreg

Datová sada s prostorovým vymezením Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `pozn`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hranice_Kr%C3%A1lov%C3%A9hradeck%C3%A9ho_kraje/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/f8a73250419c48f6a8118cfed2720566

## CESTOVNÍ RUCH, KULTURA A PAMÁTKY — 41 sad

### Válečné hroby

`n=1423` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací všech válečných hrobů na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `pocet_hrobu`, `počet_pohrbenych`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `katastr`, `cislo_parcely`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/V%C3%A1le%C4%8Dn%C3%A9_hroby/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/f51fcdb253344157a658e9fe3a4cd385

### Knihovny

`n=434` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací knihoven na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `evidencni_cislo`, `zrizovatel`, `typ_knihovny`, `bezbarierovost`, `strediskovost`, `pravni_forma`, `pocet_pobocek`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `typ_knihovny`: archivnictví · církevní/farní · justice, vláda, parlament, prezident · knihovna při základní škole · krajská (Studijní a vědecká knihovna v Hradc · lékařská · muzejní · Národní památkový ústav · neprofesionální (knihovna s prac. úvazkem kn · neprofesionální (knihovna s pracovním úvazke · pobočka (organizační část knihovny, lokálně  · pověřená (knihovna pověřená výkonem regionál
- `bezbarierovost`: ano · ne
- `strediskovost`: ano · ne
- `pravni_forma`: jiná · organizační složka obce · organizační složka státu · příspěvková organizace kraje · příspěvková organizace obce

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Knihovny/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/441d3dc35a064a45940f12f10f1d04ce

### Církevní památky

`n=162` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací církevních památek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/C%C3%ADrkevn%C3%AD_pam%C3%A1tky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/9b6e39773a3e4a05a67edf39005562b7

### Muzea a galerie

`n=160` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací muzeí a galerií na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `zrizovatel`, `bezbarierovost`, `pozn_bezbarierovost`, `typ_muzea`, `zamereni_muzea`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `is_id`

**Hodnoty:**

- `bezbarierovost`: ano · částečně · ne · nezjištěno

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Muzea_a_galerie/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/ccf33734938a4e638af65c9d8fa106e5

### Národní kulturní památky - plošné objekty

`n=129` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací plošných objektů (budovy), které jsou součástí národních kulturních památek na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

**Pole:** `nazev`, `objekt`, `ico`, `pravni_forma`, `uskp`, `ucinnost`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `pravni_forma`: Akciová společnost · Evidované církevní právnické osoby · Fyzická osoba · Fyzická osoba podnikající dle živnostenského · Obec · Obec nebo městská část hlavního města Prahy · Příspěvková organizace · Společnost s ručením omezeným · Státní podnik
- `ucinnost`: 1. července 2008 · 1. července 2010 · 1. července 2017 · 1. července 2022 · 1. července 2024 · 1. ledna 2002 · 1. října 2014 · 1. srpna 2002 · 15. července 1999

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/N%C3%A1rodn%C3%AD_kulturn%C3%AD_pam%C3%A1tky_plo%C5%A1n%C3%A9_objekty/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/57b023b1110340c7b539701c532d81b6

### Regionální potraviny

`n=102` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací sídel regionálních potravin na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

**Pole:** `nazev`, `region`, `primarni_zatrideni`, `sekundarni_zatrideni`, `tercialni_zatrideni`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `telefon_1`, `telefon_2`, `mobil_1`, `mobil_2`, `mobil_3`, `fax`, `email_1`, `email_2`, `email_3`, `email_4`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Region%C3%A1ln%C3%AD_potraviny/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/5c0304dbcdfd432fb6a7363b75a00ed1

### Válečné hroby ve vlastnictví kraje

`n=93` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací zejména pomníků války 1866, které jsou v majetku Královéhradeckého kraje a jsou svěřené do správy Muzea východních Čech v Hradci Králové. Sada dále obsahuje několik dalších pomníků a pamětních desek 1. a 2. světové války umístěných v objektech škol zřizovaných Královéhradeckým krajem nebo na pozemcích Správy silnic KHK. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruc

**Pole:** `charakter_hrobu`, `pocet_pohrbenych`, `historicka_udalost`, `popis`, `autor`, `vznik`, `cevh_id`, `cislo_hrobu_jine`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `katastr`, `cislo_parcely`, `popis_lokality`, `vlastnik_lokality`, `vlastnik_hrobu`, `verejne_misto`, `cislo_kulturni_pamatky`, `poznamka`, `www_pk`, `www_cevh`, `fotografie_1`, `fotografie_2`

**Hodnoty:**

- `charakter_hrobu`: Evidované místo · Osárium · Pietní místo - deska · Pietní místo - objekt · Válečný hrob s ostatky
- `historicka_udalost`: 1. světová válka · 2. světová válka · Prusko-rakouská válka
- `autor`: Adolf Loos st., Brno · Antonín Wasserburger, Vídeň · E. Noël, Drážďany · Emanuel Max · Gustav von Lichtenberg · Hořická škola · J. Kotovic, J. Taticek · Jan Jandera, Hořice · M. Ježek, Hradec Králové · Moriz Černil · Petr Novák, Jaroměř · V. Weinzettl
- `vznik`: 1866 · 1867 · 1868 · 1877 · 1891 · 1893 · 1896 · 1897 · 1899 · 1900 · 1901 · 1902
- `vlastnik_lokality`: Královéhradecký kraj, Pivovarské nám. 1245,  · Obec Dohalice, Dohalice 17, 503 13 Dohalice · Obec Všestary, Všestary 35, 503 12 Všestary · Rezek František,	1/2 Šebejová Jaroslava,	1/2 · Římskokatolická farnost Všestary, čp. 1, 503
- `vlastnik_hrobu`: Římskokatolická farnost Všestary Všestary · Správa nemovitostí ve vlastnictví kraje - Gy · Správa nemovitostí ve vlastnictví kraje - Le · Správa nemovitostí ve vlastnictví kraje - Mu · Správa nemovitostí ve vlastnictví kraje - Mu · Správa nemovitostí ve vlastnictví kraje - St · Správa nemovitostí ve vlastnictví kraje - St · Správa nemovitostí ve vlastnictví kraje - St · Správa nemovitostí ve vlastnictví kraje - Ze · Správa silnic Královéhradeckého kraje Kutnoh · Správa silnic Královéhradeckého kraje, Kutno
- `verejne_misto`: Ano · Ne

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/V%C3%A1le%C4%8Dn%C3%A9_hroby_ve_vlastnictv%C3%AD_kraje/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/1901ac57194d4598bbf52242f87bbb5f

### Jezdectví

`n=89` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací areálů určených pro jezdectví na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

**Pole:** `nazev`, `region`, `primarni_zatrideni`, `sekundarni_zatrideni`, `tercialni_zatrideni`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `mobil_1`, `mobil_2`, `fax`, `email_1`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Jezdectv%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/d69b0c15eaf44e46a5918a4d6f95877a

### Architektonické památky

`n=74` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací architektonických památek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Architektonick%C3%A9_pam%C3%A1tky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/93d143d3fdbd492197fc23413b4a9542

### Infocentra

`n=74` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací infocenter na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Infocentra/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/82ef86e3f1ae448ba9ce25127f95e4f5

### Cyklovýlety

`n=73` · linie · aktualizace: irreg

Datová sada s prostorovou lokalizací vybraných cyklovýletů na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `telefon`, `www`, `delka`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Cyklov%C3%BDlety/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/14bb7a922bfc49caa07a70d5ead3982c

### Technické památky

`n=67` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací technických památek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Technick%C3%A9_pam%C3%A1tky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/86a1029c7b624bd29502dee734139d11

### Řemeslné výrobky

`n=64` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací sídel řemeslných výrobků na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

**Pole:** `nazev`, `region`, `primarni_zatrideni`, `sekundarni_zatrideni`, `tercialni_zatrideni`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `telefon_1`, `telefon_2`, `mobil_1`, `mobil_2`, `mobil_3`, `fax`, `email_1`, `email_2`, `www`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/%C5%98emesln%C3%A9_v%C3%BDrobky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/a58cd530e36e418d8be4857c01912fe1

### Ostatní letní sporty

`n=61` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací areálů určených pro ostatní letní sporty na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

**Pole:** `nazev`, `region`, `primarni_zatrideni`, `sekundarni_zatrideni`, `tercialni_zatrideni`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon_1`, `telefon_2`, `mobil_1`, `mobil_2`, `mobil_3`, `fax`, `email_1`, `email_2`, `email_3`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Ostatn%C3%AD_letn%C3%AD_sporty/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/baf74cda7f4f49dd99686d8ba1f0a8d8

### Kulturní domy a kongresová centra

`n=56` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací kulturních domů a kongresových center na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `typ_od`, `typ_zarizeni`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `ico`, `ico_subjektu_zrizovatele_nebo_provozovatele`, `zrizovatel_provozovatel`, `ico_zrizovatele_provozovatele`, `nazev_vusc_zrizovatele_provozovatele`, `kod_vusc_zrizovatele_provozovatele`, `nazev_okresu_zrizovatele_provozovatele`, `kod_okresu_zrizovatele_provozovatele`, `nazev_orp_zrizovatele_provozovatele`, `kod_orp_zrizovatele_provozovatele`, `nazev_obce__zrizovatele_provozovatele`, `kod_obce_zrizovatele_provozovatele`, `nazev_ulice_zrizovatele_provozovatele`, `cislo_domovni_zrizovatele_provozovatele`, `typ_cisla_domovniho_zrizovatele_provozovatele`, `cislo_orientacni_zrizovatele_provozovatele`, `psc_zrizovatele_prozovatele`, `www`, `bezbarierovost`, `bezbarierovost_poznamka`, `vyucovane_predmety_realizovane_aktivity`, `technicka_vybavenost`, `kapacita`, `kapacita_poznamka`

**Hodnoty:**

- `typ_od`: Kulturní domy a kongresová centra
- `typ_zarizeni`: Kongresové centrum · Kulturní dům
- `nazev_okresu_zrizovatele_provozovatele`: Hradec Králové · Jičín · Náchod · Rychnov nad Kněžnou · Trutnov
- `nazev_orp_zrizovatele_provozovatele`: Broumov · Dobruška · Dvůr Králové nad Labem · Hořice · Hradec Králové · Jaroměř · Jičín · Kostelec nad Orlicí · Náchod · Nová Paka · Nové Město nad Metují · Nový Bydžov
- `bezbarierovost`: ANO · ČÁSTEČNĚ · NE
- `kapacita_poznamka`: 650 míst v přízemí + 250 míst balkón. · 80 míst na sezení, 140 na stání · Dále je k dipozici několik jednacích místnos · dva navzájem propojené sály; v každém je kap · I. patro: kavárna 40 míst;                   · Kapacita přízemí a prvního patra. 
Dále je k · konferenční sál pro 50-100 osob - dle uspořá · Při konfiguraci sálu k sezení u stolů je kap · společenský sál -     70 - 80  míst k sezení · Stolová úprava 320 míst k sezení, přednášky  · v rozložení pro divadlo · V rozmístění divadlo: Velký sál: 1170, Malý 

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Kulturn%C3%AD_domy/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/cccf864fdc654c3fae39a54ed20c2183

### Letní koupání

`n=54` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací areálů určených pro letní koupání na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

**Pole:** `nazev`, `region`, `primarni_zatrideni`, `sekundarni_zatrideni`, `tercialni_zatrideni`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon_1`, `telefon_2`, `telefon_3`, `mobil_1`, `mobil_2`, `fax`, `email_1`, `email_2`, `email_3`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Letn%C3%AD_koup%C3%A1n%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/f7c3fbb0791549bd8fa8c8dcb4818013

### Kina

`n=51` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací kin na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `provozovatel`, `ico`, `bezbarierovost`, `pozn_bezbarierovost`, `typ_kina`, `pocet_salu`, `kapacita`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `bezbarierovost`: ANO · ČÁSTEČNĚ · NE · NEZJIŠTĚNO
- `pozn_bezbarierovost`: bezbarérové WC · bezbariérové WC · bezbariérové WC, balkon není bezbariérový · bezbariérový vstup ano, není bezbariérové WC · bezbariérový vstup je v divadle i v zahradě, · galerie je bezbariérová s asistencí druhé os · kino nemá bezbariérové WC, restaurace hned v · momentálně probíhá rekonstrukce, která zahrn · není bezbariérové WC · není bezbariérové WC (šíře zárubní nesplňuje · není bezbariérové WC, od jara 2025 plánovaná · NEZJIŠTĚNO
- `typ_kina`: kino s letním provozem · kino s nepravidelným provozem · kino s nepravidelným provozem, kino s letním · kino se stálým provozem · multikino se stálým provozem

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Kina/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/66de083248634ad1bce9bb98265a5b27

### Ostatní historické památky

`n=51` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací ostatních historických památek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Ostatn%C3%AD_historick%C3%A9_pam%C3%A1tky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/ddba05ea835746b7a4248b63af93288f

### Rozhledny a vyhlídky

`n=49` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací rozhleden a vyhlídek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Rozhledny_a_vyhl%C3%ADdky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/d3ea6d68bb3b421a8b988c4a58024a0f

### Pevnosti a opevnění

`n=38` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací pevností a opevnění na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pevnosti_a_opevn%C4%9Bn%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/539847974bf8492c9d7fdf9a369b9f63

### Hudební kluby a festival parky

`n=36` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací hudebních klubů a festival parků na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `provozovatel`, `popis`, `typ_provozu`, `kapacita`, `kapacita_pozn`, `bezbariérovost`, `bezbariérovost_pozn`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `typ_provozu`: festival park · hudební kavárna · hudební klub · letní festivalový park · letní hudební klub a hudební klub · multifunkční prostor
- `kapacita_pozn`: 100 na sezení a 150 na stání · 110 míst k sezení, 90 míst k stání · 120 míst k sezení · 1256 míst k sezení · 130 míst k sezení nebo přes 300 míst k stání · 180 míst k sezení a 120 míst ke stání · 200 až 300 míst · 200 míst na sezení · 40 míst k sezení · 50 míst k sezení v klubu, 50 míst k sezení v · 500 až 1000 míst na stání. K sezení mobilní  · 60 míst na sezení
- `bezbariérovost`: ano · částečně · ne
- `bezbariérovost_pozn`: Areál je částečně bezbariérový. Na větších a · bezbariérová toaleta · bezbariérová toaleta a sprcha · bezbariérový přístup je do 1. patra (kavárna · Do areálu je bezbariérový přístup po štěrku  · není bezbariérová toaleta · osoby na vozíku mají vstup zdarma. · Tři schody u vstupu. Pět schodů na toaletu. · U vstupu do objektu jsou 3 schody, vnitřní p · vnitřní prostor je částečně bezbariérový, u 

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hudebn%C3%AD_kluby_a_festival_parky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/f16bb2cb44c64ee9882eefaa3c263dd6

### Památkové zóny

`n=35` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací památkových zón na území Královéhradeckého kraje. Jsou zde obsaženy městské, vesnické i krajinné památkové zóny.  Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

**Pole:** `nazev`, `vyhlasovatel`, `dokument`, `uskp`, `prohlaseni`, `ucinnost`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `psc`, `www`

**Hodnoty:**

- `vyhlasovatel`: Ministerstvo kultury ČR · Východočeský KNV Hradec Králové
- `dokument`: Opatření obecné povahy č. 1/2016 · Vyhláška č. 108/2003 Sb. · Vyhláška č. 249/1995 Sb. · Vyhláška MK ČR č. 208/1996 Sb. · Vyhláška MK ČR č. 413/2004 Sb. · Vyhláška Vč KNV Hradec Králové ze dne 17.10.
- `prohlaseni`: 1. července 1996 · 1. dubna 2003 · 12. května 2016 · 17. října 1990 · 22. září 1995 · 24. června 2004
- `ucinnost`: 1. července 1996 · 1. ledna 2005 · 1. listopadu 1990 · 1. září 2003 · 22. září 1995 · 9. června 2016

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pam%C3%A1tkov%C3%A9_z%C3%B3ny/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/d417e5eaaf2844439842bb0ada976f5d

### Pivovary

`n=34` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací pivovarů na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

**Pole:** `nazev`, `region`, `primarni_zatrideni`, `sekundarni_zatrideni`, `tercialni_zatrideni`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `mobil_1`, `mobil_2`, `fax`, `email_1`, `email_2`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pivovary/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/a22dfb6709864e6984f01527491ae70b

### Divadla a filharmonie

`n=33` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací divadel a filharmonií na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `popis`, `typ_zarizeni`, `zrizovatel`, `kapacita`, `bezbarierovost`, `typ_bezbarierovosti`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `email`

**Hodnoty:**

- `typ_zarizeni`: divadlo · filharmonie + hudba · multifunkční prostor · přírodní divadlo
- `bezbarierovost`: ANO · NE
- `typ_bezbarierovosti`: lze po předchozí domluvě zajistit bezbariéro · tělesně postižené · tělesně postižené, sluchově postižené · tělesně postižené, zrakově postižené, slucho

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Divadla_a_filharmonie/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/fde032684cd848d4a076fa978b8d5624

### Zámky

`n=33` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací zámků na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1mky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/b03f47566c9a4ce4aa6ed8db765ae824

### Kulturní akce pořádané s trvalou záštitou Rady Královéhradeckého kraje 2026–2029

`n=30` · bod · aktualizace: unknown

Kulturní akce pořádané s trvalou záštitou Rady Královéhradeckého kraje 2026–2029. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `zamereni`, `termin`, `poradatel`, `ico`, `popis_akce`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_popisne`, `cislo_orientační`, `psc`, `www`

**Hodnoty:**

- `zamereni`: divadelní · folklórní, tradiční a řemeslné · hudební · multioborové
- `termin`: aktuální termín je uveden na webových stránk · březen–duben · červen · červenec · červenec–srpen · červen–srpen · květen–září · leden–květen, říjen–listopad · listopad · říjen · říjen–listopad · srpen

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Trval%C3%A9_z%C3%A1%C5%A1tity_Rady_Kr%C3%A1lov%C3%A9hradeck%C3%A9ho_kraje/FeatureServer/4`

Portál: https://www.datakhk.cz/datasets/63be156a63564852b29cdaa27e420024

### Rybaření

`n=30` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací areálů pro rybaření na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

**Pole:** `nazev`, `region`, `primarni_zatrideni`, `sekundarni_zatrideni`, `tercialni_zatrideni`, `popis`, `nazev_kraje`, `kod_kraje`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `psc`, `telefon`, `mobil_1`, `mobil_2`, `fax`, `email_1`, `email_2`, `www`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Ryba%C5%99en%C3%AD/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/2dfef5cd6dab461d8eb5596de9dfc2e2

### Zábavní centra

`n=27` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací zábavních center na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1bavn%C3%AD_centra/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/f239c6222745417ba617c8d54afad1ad

### Národní kulturní památky - bodové objekty

`n=24` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací bodových objektů (sochy apod.), které jsou součástí národních kulturních památek na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

**Pole:** `nazev`, `objekt`, `ico`, `pravni_forma`, `uskp`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `nazev`: Babiččino údolí · Broumovský klášter · Hospital Kuks · Hřbitovní kostel Panny Marie v Broumově · Kaple P. Marie Sněžné na Hvězdě · Kostel sv. Anny · Kostel sv. Barbory · Kostel sv. Jakuba Většího s farou · Kostel sv. Jiří a sv. Martina · Kostel sv. Maří Magdaleny · Kostel sv. Markéty · Kostel sv. Michala archanděla
- `pravni_forma`: Církev · Fyzická osoba podnikající dle živnostenského · Příspěvková organizace · Společnost s ručením omezeným
- `uskp`: 11782/6-1584 · 19798/6-3580 · 20791/6-1539 · 24307/6-1637 · 29136/6-1832 · 29424/6-1813 · 30182/6-1558 · 31488/6-1818 · 31494/6-1533 · 33657/6-1867 · 35502/6-1496 · 35975/6-1604

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/N%C3%A1rodn%C3%AD_kulturn%C3%AD_pam%C3%A1tky_bodov%C3%A9_objekty/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/d0043d8d7dc64e48a2fc06518314b269

### Hrady

`n=21` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací hradů na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hrady/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/40a4d224a8b346e89350f5bc9327e29e

### Lanovky

`n=20` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací lanovek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

**Hodnoty:**

- `nazev`: Kabinová lanovka Janské Lázně - Černohorský  · Kabinová lanovka Janské Lázně - Hofmanky exp · Kabinová lanovka Pec pod Sněžkou - Růžová ho · Sedačková lanovka Černý Důl - Family Express · Sedačková lanovka Černý Důl - Špičák, Bönisc · Sedačková lanovka Deštné v Orlických horách  · Sedačková lanovka Herlíkovice - Bubákov (zim · Sedačková lanovka Herlíkovice - Žalý · Sedačková lanovka Janské Lázně, Černá hora – · Sedačková lanovka Mladé Buky - Pekelský vrch · Sedačková lanovka Pec pod Sněžkou – Hnědý vr · Sedačková lanovka Říčky v Orlických horách (
- `popis`: Čtyřsedačková lanová dráha s celoročním prov · Čtyřsedačková lanová dráha se zimním provoze · Čtyřsedačková lanovka s letním i zimním prov · Čtyřsedačková lanovka se zimním provozem. · Čtyřsedačková lanovka vede ze Špindlerova Ml · Čtyřsedačková lanovka z Říček na vrch Zaklet · Dvousedačková lanovka z Deštného na Studený  · Lanová dráha Kejnos se zimním i letním provo · Moderní šestisedačková lanovka s bublinou pr · Osmimístná kabinová lanová dráha vedoucí na  · Tradiční dvousedačková lanovka vyvážela celo · Třísedačková lanová dráha se zimním provozem

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/L/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/80787793513d464bac25abebebfa4aaa

### Národní kulturní památky - pozemky

`n=20` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací pozemků národních kulturních památek na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

**Pole:** `nazev`, `ico`, `pravni_forma`, `uskp`, `ucinnost`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovní`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `nazev`: Babiččino údolí v Ratibořicích · Betlém v Novém lese u Kuksu · dům čp. 92 "Dřevěnka" v Úpici · Hospital Kuks · hrad Kost · kaple Zjevení Páně ve Smiřicích · klášter v Broumově · kostel sv. archanděla Michaela ve Vernéřovic · kostel sv. Jana Křtitele se zvonicí a márnic · kostel sv. Jiří a sv. Martina v Martínkovicí · kostel sv. Petra a Pavla se zvonicí a farou  · Městské muzeum (Wenkeův obchodní dům) v Jaro
- `pravni_forma`: Akciová společnost · Evidované církevní právnické osoby · Fyzická osoba · Fyzická osoba podnikající dle živnostenského · Obec nebo městská část hlavního města Prahy · Příspěvková organizace · Společnost s ručením omezeným · Státní podnik
- `uskp`: 150 · 208 · 209 · 210 · 211 · 212 · 213 · 268 · 269 · 270 · 318 · 319
- `ucinnost`: 1. července 2008 · 1. července 2010 · 1. července 2017 · 1. července 2022 · 1. ledna 2002 · 1. října 2014 · 1. srpna 2002 · 15. července 1999

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/N%C3%A1rodn%C3%AD_kulturn%C3%AD_pam%C3%A1tky_pozemky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/6c7673331dd54ccf9585dcb3410ca0e7

### Letecké sporty

`n=17` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací areálů určených pro letecký sport na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

**Pole:** `nazev`, `region`, `primarni_zatrideni`, `sekundarni_zatrideni`, `tercialni_zatrideni`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon_1`, `telefon_2`, `mobil_1`, `mobil_2`, `fax`, `email_1`, `email_2`

**Hodnoty:**

- `nazev`: Aeroklub Dvůr Králové nad Labem · Aeroklub Hořice · Aeroklub Nové Město nad Metují · DSA – Letiště, Hradec Králové · FLY FOR FUN, Hradec Králové · Letecká škola Vrchlabí · Letecké služby Hradec Králové · Letiště Broumov · Letiště Jaroměř – Josefov, Jaroměř · Letiště Jičín - Vokšice · Paragliding na Zvičině · Paragliding school - Škola Paraglidingu
- `region`: Broumov / Broumovsko · Dolní Kalná / Krkonoše · Dvůr Králové nad Labem / Podkrkonoší · Hradec Králové / Hradecko · Jaroměř / Kladské pomezí · Jičín / Český ráj · Nové Město nad Metují / Kladské pomezí · Ostroměř / Podkrkonoší · Špindlerův Mlýn / Krkonoše · Třebihošť / Podkrkonoší · Vrchlabí / Krkonoše
- `popis`: Dále pronajímá benzínovou stanici v areálu l · Detailní popis historie hořického létání nal · Historie létání ve Dvoře Králové nad Labem s · Je však také možné zaletět o kus dál a pozna · Letecká škola ve Vrchlabí provádí teoreticko · Letiště Hradec Králové zprostředkovává vyhlí · Letiště nabízí vyhlídkové lety nad Broumovsk · Nabídka je vhodná jak pro jednotlivce, tak p · Směry drah: 12/30, 263m. n. m. 
 Komunikační · Společnost Yellow Point nabízí let na tandem · Tato škola byla založena již roku 1994 panem · Ve strojovém parku naleznete letadla a vrtul

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Leteck%C3%A9_sporty/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/270d30d318204f3ebbc73cc3adb5ca30

### Národní kulturní památky - liniové objekty

`n=16` · linie · aktualizace: irreg

Datová sada s prostorovou lokalizací liniových objektů (zdi, terasy, brány), které jsou součástí národních kulturních památek na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

**Pole:** `nazev`, `objekt`, `ico`, `pravni_forma`, `uskp`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`

**Hodnoty:**

- `nazev`: Babičino údolí · Hospital Kuks · hrad Kost · Hrádek u Nechanic · klášter v Broumově · kostel sv. Jana Křtitele se zvonicí a márnic · zámek Opočno
- `objekt`: brána · domek čp. 49, ohradní zeď dvorku s branami · hradební zeď · jižní brána zahrady · ohradní zeď · ohradní zeď hřbitova s brankou · ohradní zeď s brankami a branami · ohradní zeď s branou a brankou · ohradní zeď se souborem soch ctností · ohradní zeď se souborem soch neřestí · ohrazení parku · ohrazení parku včetně plovoucích vrat
- `pravni_forma`: Akciová společnost · Evidované církevní právnické osoby · Příspěvková organizace · Společnost s ručením omezeným
- `uskp`: 11782/6-1584 · 16867/6-1880 · 18278/6-619 · 19798/6-3580 · 31494/6-1533 · 36329/6-1332 · 37808/6-2351

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/N%C3%A1rodn%C3%AD_kulturn%C3%AD_pam%C3%A1tky_liniov%C3%A9_objekty/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/50bbfec8c7ec43a69448c6d6132282de

### Solné jeskyně

`n=15` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací solných jeskyní na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

**Pole:** `nazev`, `region`, `primarni_zatrideni`, `sekundarni_zatrideni`, `tercialni_zatrideni`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon_1`, `telefon_2`, `mobil_1`, `mobil_2`, `fax`, `email_1`, `email_2`

**Hodnoty:**

- `nazev`: Penzion Martin · Solná jeskyně AniKa, Hradec Králové · Solná jeskyně Lilie, Špindlerův Mlýn · Solná jeskyně Relax Hradec Králové · Solná jeskyně Rychnov nad Kněžnou · Solná jeskyně v Centru Walzel · Solná jeskyně v Lipkách, Hradec Králové · Solná jeskyně V Lipové aleji, Jičín · Solná jeskyně Vrchlabí · Solná jeskyně Žacléř · Solná jeskyně, Dvůr Králové nad Labem · Solná jeskyně, Jičín
- `region`: Dvůr Králové nad Labem / Podkrkonoší · Hradec Králové / Hradecko · Janské Lázně / Krkonoše · Jaroměř / Kladské pomezí · Jičín / Český ráj · Meziměstí / Broumovsko · Náchod / Kladské pomezí · Rychnov nad Kněžnou / Orlické hory a Podorli · Špindlerův Mlýn / Krkonoše · Vrchlabí / Krkonoše · Žacléř / Krkonoše
- `popis`: Celý interiér obsahuje více než 14 tun soli. · Sůl z Mrtvého moře je výjimečná především ob · Jedna hodina pobytu v solné jeskyni je podle · Jeskyně je tvořena pouze dřevem a solí z Mrt · Naše solná jeskyně je vystavěna z kombinace  · Obrovská léčivá síla soli při 50 min. terapi · Penzion je vzdálen 20 metrů od lyžařských vl · Solná jeskyně je vhodná pro všechny návštěvn · Solná jeskyně se nachází v příjemném prostře · Solná jeskyňka je vybudovaná z přírodní kame · Unikátní konstrukce solné jeskyně je vybudov · Unikátní minerální složení zajišťuje maximál

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Soln%C3%A9_jeskyn%C4%9B/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/3b31adc233464ff3ad482313a4010377

### Židovské památky

`n=14` · bod · aktualizace: annual

Datová sada s prostorovou lokalizací židovských památek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

**Pole:** `nazev`, `popis`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`, `www`, `telefon`, `ds_id`

**Hodnoty:**

- `nazev`: Mikve, Dobruška · Synagoga a židovský hřbitov v Novém Bydžově · Synagoga a židovský hřbitov v Rychnově nad K · Synagoga a židovský hřbitov, Dobruška · Synagoga Jičín · Synagoga v Hořicích · Synagoga v Hradci Králové · Židovské památky, Jičín · Židovský hřbitov a synagoga v Hořicích · Židovský hřbitov Podbřezí · Židovský hřbitov v Dobrušce · Židovský hřbitov v Jičíně
- `popis`: Barokní synagoga ze 70. let 18. století. Nyn · Budova synagogy je kromě hřbitova jedinou za · Ghetto z roku 1721, do kterého bylo přesídle · Jičínská synagoga je jednou z mála poměrně d · Rekonstruovaný lesní hřbitov ze 17. století  · Rituální očistná lázeň z 18. století · Synagoga pocházející ze 17. století byla zni · Synagoga se nachází na nároží Pospíšilovy tř · V Jičíně se dochovalo několik významných žid · Vznik židovského hřbitova ve Vamberku se úda · Židé se v Hořicích poprvé připomínají v 1. p · Židovský hřbitov připomíná bývalou židovskou

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/%C5%BDidovsk%C3%A9_pam%C3%A1tky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/42b28ab17ca84687ba2c3cfebe513ddb

### Kulturní památky na indikativním seznamu

`n=13` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací nemovitých kulturních památek ČR zapsaných na Indikativní seznam národních kulturních památek ČR v Královéhradeckém kraji. Jedná se o seznam kulturních památek, které mají potenciál být prohlášeny národními kulturními památkami. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `cislo_orientacni`, `psc`

**Hodnoty:**

- `nazev`: Hlubinný důl - Jáma Julie · Katedrální kostel sv. Ducha · Labská vodní elektrárna · Mateřská škola · Městské opevnění II., Horní pevnost · Sbor kněze Ambrože s kolumbáriem · Škola obecná a měšťanská · Střední škola - gymnázium J. K. Tyla · Uhelný důl hlubinný Jan · Základní škola · Zámek Doudleby nad Orlicí · Zámek Karlova Koruna - areál

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Kulturn%C3%AD_pam%C3%A1tky_na_indikativn%C3%ADm_seznamu/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/60df95fa7034480c80bc41628834bd04

### Golfová hřiště

`n=9` · bod · aktualizace: unknown

Datová sada s prostorovou lokalizací golfových hřišť v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `pocet_jamek`, `par`, `verejne_hriste`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `psc`, `telefon`, `email`, `www`

**Hodnoty:**

- `nazev`: Golf - Nová Amerika Country Club · Golf & Country Club Prosečné · Golf Club Mladé Buky · Golf Club Myštěves · Golf resort Na Vrších · Golfové hřiště Hrádek u Nechanic · Golfový Klub Symbióza Nebeská Rybná · Grosshof Golf Club · Park Golf Club Hradec Králové
- `verejne_hriste`: ano · ne

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Golfov%C3%A1_h%C5%99i%C5%A1t%C4%9B/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/5087e0aa719b4742a8f696c38f1042bf

### Památkové rezervace

`n=7` · plocha · aktualizace: irreg

Datová sada s prostorovou lokalizací památkových rezervací na území Královéhradeckého kraje. Jsou zde obsaženy městské a vesnické památkové zóny. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

**Pole:** `nazev`, `vyhlasovatel`, `dokument`, `uskp`, `prohlaseni`, `ucinnost`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `psc`, `www`

**Hodnoty:**

- `nazev`: Městská památková rezervace Hradec Králové · Městská památková rezervace Jičín · Městská památková rezervace Josefov · Městská památková rezervace Nové Město nad M · Památková rezervace Kuks s přilehlým komplex · Vesnická památková rezervace Křinice · Vesnická památková rezervace Vesec u Sobotky
- `vyhlasovatel`: Ministerstvo kultury ČSR · Vláda České republiky
- `dokument`: Nařízení Vlády České republiky č. 127/1995 S · Výnos ministerstva kultury ČSR č. 16 417/87- · Výnos ministra kultury ČSR č. 15 106/1969-II
- `uskp`: 1010 · 1020 · 1024 · 1026 · 1029 · 1099 · 1100
- `prohlaseni`: 21. prosince 1987 · 24. května 1995 · 3. prosince 1969
- `ucinnost`: 1. ledna 1970 · 25. července 1995 · 29. prosince 1987

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pam%C3%A1tkov%C3%A9_rezervace/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/cedb0284df244b499e0e34ed8b5f4dda

### Botanické zahrady a arboreta

`n=2` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací botanických zahrad a arboret na území Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `ico`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `psc`, `telefon`, `email`, `www`

**Hodnoty:**

- `nazev`: Botanická zahrada léčivých rostlin Farmaceut · Genofondová zahrada Správy Krkonošského náro

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Botanick%C3%A9_zahrady_a_arboreta/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/70487254d6104e7d83a027e83bfeb47e

### ZOO a zooparky

`n=2` · bod · aktualizace: irreg

Datová sada s prostorovou lokalizací zoologických zahrad a zooparků na území Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

**Pole:** `nazev`, `typ`, `ico`, `nazev_okresu`, `kod_okresu`, `nazev_orp`, `kod_orp`, `nazev_obce`, `kod_obce`, `nazev_ulice`, `cislo_domovni`, `typ_cisla_domovniho`, `psc`, `telefon`, `email`, `www`, `OBJECTID_1`

**Hodnoty:**

- `nazev`: Safari Park Dvůr Králové · ZOO park Stěžery
- `typ`: zoologická zahrada · zoopark

API: `https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/ZOO_a_zooparky/FeatureServer/0`

Portál: https://www.datakhk.cz/datasets/49e9f6f95b084caeaf52a09d972838ca

## METADATA PORTÁLU (bez využitelných dat) — 34 sad

Stránky portálu, nikoli data: Dashboardy, Feedback, Hackathon 2022 mentoři a porotci, Hackathon 2022 partneři, Hackathon 2023 mentoři a porotci , Hackathon 2023 partneři, Hackathon 2024 mentoři a porotci, Hackathon 2024 partneři, Hackathon 2025 mentoři a porotci, Hackathon 2025 partneři, Hackathon 2026 mentoři a porotci, Hackathon 2026 partneři, Infografiky, analýzy a publikace, Informace o stránce, Jak otevíráme data, Kontakty, tým, Královéhradecký kraj publikuje nejvíce otevřených sad dat mezi kraji ČR, Kultura, cestovní ruch, Mapa stránek, Mapy MAS a DSO, Mapy z oblasti kultury, Novinky, O webu Data KHK, Rozcestník aplikací, Rozcestník portálů, Statistická data v grafech, Strategická mapa kraje, Zdroje dat na portálu, Zpráva a datové karty, Zpráva a datové karty 2020, Zpráva a datové karty 2021, Zpráva a datové karty 2022 , Zpráva a datové karty 2023, Zpráva a datové karty 2024
