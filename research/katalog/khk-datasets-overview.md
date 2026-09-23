# Přehled datových sad KHK – struktura a ukázka dat

## Služebny Policie ČR

> Datová sada s prostorovou lokalizací služeben a stanic Policie ČR v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Policie ČR, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **36** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Slu%C5%BEebny_Policie_%C4%8CR/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_obvodu | Název obvodního oddělění / policejní stanice | String | Obvodní oddělení 1 Hradec Králové |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Kralovehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Gočárova třída |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1653 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50002 |
| www | Webové stránky | String | https://www.policie.cz/SCRIPT/imapa.aspx?area=vc2&docid=716& |
| telefon | Telefon | String | +420974526651 |
| fax | Fax | String | +420974523531 |
| email | Email | String | hk.hk1@pcr.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.2114 15.8097) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8097 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2114 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PCR1 |

## Záplavové území pětileté vody

> Datová sada s prostorovou lokalizací záplavového území pětileté vody. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje. Data stažena zVodohospodářského informačního portálu VODA https://voda.gov.cz/ Průběh polygonů záplavových území v této vrstvě je pouze ORIENTAČNÍ, jedná se o kompilaci dat jednotlivých dílčích záplavových území, v rámci které byly provedeny úpravy (vyhlazení) geometrie v místě návaznosti jednotlivých částí. Pro závazné informace ohledně průběhu polygonů kontaktujte správce daného vodního toku nebo místě příslušný vodoprávní úřad.

Klíčová slova: Záplava, Povodeň, Pětiletá voda, Q5, Životní prostředí, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **78** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1plavov%C3%A9_%C3%BAzem%C3%AD_p%C4%9Btilet%C3%A9_vody/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_tok | Název vodního toku | String | HOZ Kukleny |
| id_tok | Označení vodního toku | Double | 104480000200 |
| nazev_tok_vyssiho_radu | Název vodního toku, do kterého se daný vodní tok vlévá | String | Labský náhon |
| id_tok_vyssiho_radu | Označení vodního toku, do kterého se daný vodní tok vlévá | Double | 104450000100 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| dp_id | Jedinečný identifikátor va katalogu otevřených dat Data KHK | String | QPET1 |

## Královéhradecký kraj publikuje nejvíce otevřených sad dat mezi kraji ČR

> .

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Seznam zaručených přípojů (vyčkávání autobusů na autobusy a vlaky)

> Datová sada obsahuje seznam zaručených přípojů - vyčkávání autobusů na autobusy a vlaky v rámci Veřejné dopravy Královéhradeckého kraje. Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Přípoje, Veřejná doprava VDKHK, Doprava, Autobusy, Vlaky, Královéhradecký kraj

- Geometrie: — | Záznamů: **3515** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Seznam_zaru%C4%8Den%C3%BDch_p%C5%99%C3%ADpoj%C5%AF/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| linka | Linka | String | IREDO 504 |
| licence | Číslo vydané licence čekajícího spoje | String | 631504 |
| spoj | Označení čísla čekajícího spoje dle schváleného jízdního řádu | String | 1 |
| dopravce | Dopravní společnost, která daný konkrétní čekající spoj zajišťuje | String | BusLine KHK s.r.o. |
| odjezd | Pravidelný odjezd konkrétního čekajícího spoje v souladu se schváleným jízdním řádem | TimeOnly | 05:25:00 |
| zastávka | Název zastávky, ve které spoj čeká | String | Hořice,aut.st. |
| vycka_min | Maximální doba čekání spoje v minutách (po uplynutí této doby spoj může odjet, návaznost nemusí být dodržena),výši čekání může operativně změnit pouze centrální dispečink IREDO | String | 5 |
| typ | Typ dopravního prostředku na který je čekáno (vlak/bus) | String | autobus |
| linka_d | U typu A - číslo vydané licence spoje, na který je čekáno, u typu V - číslo vlakového spoje, na který je čekáno | String | IREDO 524 |
| licence_d | Číslo vydané licence čekajícího spoje, na který je čekáno | String | 631524 |
| spoj_d | U typu A - označení čísla spoje, na který je čekáno dle schváleného jízdního řádu, u typu V - neuvedeno | String | 2 |
| dopravce_d | Dopravní společnost, která daný konkrétní spoj, na který je čekáno zajišťuje | String | BusLine KHK s.r.o. |
| z_do_zast | Směr, z kterého nebo kam spoj, na který je čekáno, přijede (informace převážně pro řidiče, který na spoj čeká) | String | Cerekvice n.Bystř. |
| nav_cas | Čas příjezdu spoje, na který je čekáno dle pravidelného jízdního řádu v minutách | TimeOnly | 05:20:00 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | SZP301 |

## Novinky

> Přehledný seznam všech novinek na portálu Data KHK.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Poskytovatelé sociálních služeb

> Datová sada s prostorovou lokalizací zařízení poskytovatelů sociálních služeb na území Královéhradeckého kraje. Zdrojem dat je Odbor sociálních věcí Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Sociální služby, Poskytovatelé, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **461** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Poskytovatel%C3%A9_soci%C3%A1ln%C3%ADch_slu%C5%BEeb/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_poskytovatele | Název poskytovatele | String | AHC Centrum následné péče Trutnov s.r.o. |
| ico | IČO | String | 10913416 |
| dic | DIČ | String | 0 |
| typ_soc_sluzby | Typ sociální služby dle 108/2006 Sb. Zákona o sociálních službách | String | sociální služby poskytované ve zdravotnických zařízeních lůž |
| pravni_forma | Právní forma | String | Společnost s ručením omezeným |
| pravni_forma_zrizovatele | Právní forma zřizovatele | String | 0 |
| nazev_zarizeni | Název zařízení | String | AHC Centrum následné péče Trutnov |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ0525 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Trutnov |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579025 |
| nazev_ulice | Název ulice | String | Novodvorská |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 949 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String | 0 |
| psc | PSČ | String | 54101 |
| wkt | Zápis vektorové geometrie | String | POINT(50.5655, 15.9149) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.9149 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.5655 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PSS1 |

## Počet žáků dle oborů středních škol v Královéhradeckém kraji od školního roku 2021/2022

> Datová sada s prostorovou lokalizací počtu žáků dle oborů středních škol v Královéhradeckém kraji od školního roku 2021/2022. Zdrojem dat je Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Střední školy, Školství, Obory, Počet, Žáci, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **1754** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Po%C4%8Det_%C5%BE%C3%A1k%C5%AF_dle_obor%C5%AF_st%C5%99edn%C3%ADch_%C5%A1kol_v_KHK/FeatureServer/4

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | nazev | String | ACADEMIA MERCURII soukromá střední škola, s.r.o. |
| redizo | redizo | String | 600012271 |
| izo | izo | String | 108024148 |
| izo_sidlo | izo_sidlo | String | 108024148 |
| zrizovatel | zrizovatel | String | privátní sektor |
| druh_zarizeni | druh_zarizeni | String | Střední škola |
| nazev_vusc | nazev_vusc | String | Královéhradecký kraj |
| kod_vusc | kod_vusc | String | CZ052 |
| nazev_okresu | nazev_okresu | String | Náchod |
| kod_okresu | kod_okresu | String | CZ0523 |
| nazev_orp | nazev_orp | String | Náchod |
| kod_orp | kod_orp | String | 5209 |
| nazev_obce | nazev_obce | String | Náchod |
| kod_obce | kod_obce | String | 573868 |
| nazev_ulice | nazev_ulice | String | Smiřických |
| cislo_domovni | cislo_domovni | String | 740 |
| typ_cisla_domovniho | typ_cisla_domovniho | String | cp |
| cislo_orientacni | cislo_orientacni | String |  |
| kategorie | kategorie | String | Obory s maturitní zkouškou - M |
| obor_kod | obor_kod | String | 6542M02 |
| obor_nazev | obor_nazev | String | Cestovní ruch |
| skolni_rok | skolni_rok | String | 2021/2022 |
| pocet_zaku_celkem | pocet_zaku_celkem | Double | 98 |
| pocet_absolventu | pocet_absolventu | Double | 18 |
| pocet_prijatych | pocet_prijatych | Double | 25 |
| wkt | wkt | String | POINT(50.2063044  15.8336947) |
| x | x | Double | 15.8336947 |
| y | y | Double | 50.2063044 |
| id | id | String | PSSS1 |

## Informační centra autobusové dopravy VDKHK

> Datová sada s prostorovou lokalizací informačních center autobusové dopravy Veřejné dopravy Královéhradeckého kraje (VDKHK) včetně kontaktů na danou provozovnu. Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Informační centra, Autobusová doprava VDKHK, Veřejná doprava Královéhradeckého kraje, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **13** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Informa%C4%8Dn%C3%AD_centra_autobusov%C3%A9_dopravy_VDKHK/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| autobusovy_dopravce | Název autobusového dopravce | String | BusLine KHK s.r.o. |
| oteviraci_doba | Otevírací doba | String | Pondělí - Pátek: 7:00 – 11:30, 12:00 – 15:00 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Dvůr Králové nad Labem |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5203 |
| nazev_obce | Název obce | String | Dvůr Králové nad Labem |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579203 |
| nazev_ulice | Název ulice | String | 17. listopadu |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1076 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| psc | PSČ | String | 54401 |
| telefon | Telefon | String | +420481368111 |
| email | Email | String | dotazy@busline.cz |
| www | Webové stránky | String | https://www.dopravakhk.cz |
| wkt | Zápis vektorové geometrie | String | POINT(15.8164 50.4288) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8164 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4288 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ICAD1 |

## Přírodní zajímavosti

> Datová sada s prostorovou lokalizací přírodních zajímavostí na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Přírodní zajímavosti, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **128** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99%C3%ADrodn%C3%AD_zaj%C3%ADmavosti/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Rokle Americká sluj |
| popis | Popis | String | Rokle nacházející se v Prachovských skalách. |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Jičín |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 572659 |
| nazev_ulice | Náztev ulice | String |  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String |  |
| typ_cisla_domovniho | Typ čísla domovního | String |  |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50601 |
| www | Webové stránky | String | http://www.prachovskeskaly.com/ |
| telefon | Telefon | String | +420777901211 |
| wkt | Zápis vektorové geometrie | String | POINT(50.4629 15.291) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.291 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4629 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PRIZAJ1 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 554 |

## Schválené individuální dotace do okresů v letech 2021 - 2025

> Datová sada s prostorovou lokalizací schválených individuálních dotací Královéhradeckého kraje do území okresů v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dotace, Individuální dotace, Okresy, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **5** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_dotace_do_okres%C5%AF/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| poc_dotaci | Počet dotací | Double | 768 |
| finance | Množství financí v Kč | Double | 633606988 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | IDOTOKR1 |

## Průmyslové zóny

> Datová sada s prostorovou lokalizací průmyslových zón. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

Klíčová slova: Průmyslové zóny, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **14** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pr%C5%AFmyslov%C3%A9_z%C3%B3ny/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Hořice |
| rozloha | Rozloha v metrech čtverečních | Double | 56671 |
| verejna_podpora | Poskytnutí veřejné podpory | String | ano |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hořice |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5204 |
| nazev_obce | Název obce | String | Hořice |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 572926 |
| wkt | Zápis vektorové geometrie | String | POINT(50.3655 15.6243) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.6243 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.3655 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PZ1 |

## Cyklovýlety

> Datová sada s prostorovou lokalizací vybraných cyklovýletů na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Cyklovýlety, Královéhradecký kraj

- Geometrie: esriGeometryPolyline | Záznamů: **73** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Cyklov%C3%BDlety/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Jičín - Turnov |
| popis | Popis | String | Cykloturistika: Jičín - Turnov (32 km) |
| telefon | Telefon | String | +420493534390 |
| www | Webové stránky | String | http://www.jicin.org |
| delka | Délka cyklovýletu v metrech | Integer | 32263 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku | String | CZ052 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | CYKLOV1 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 350 |

## Administrativní členění obcí

> Datová sada s prostorovou lokalizací obcí na území Královéhradeckého kraje, které jsou začleněny do administrativního členění České republiky včetně kódů dle Českého statistického úřadu a do turistického regionu dle agentury CzechTourism. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Členění, Hranice, Obce, Regiony, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **448** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Administrativn%C3%AD_%C4%8Dlen%C4%9Bn%C3%AD_obc%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_obce | Název obce | String | Červená Hora |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 505099 |
| nazev_pou | Název správního obvodu obce s pověřeným obecním úřadem | String | Červený Kostelec |
| kod_pou | Kód správního obvodu obce s pověřeným obecním úřadem dle číselníku ČSÚ | String | 52091 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Náchod |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5209 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| turist_reg | Turistický region dle agentury CzechTourism | String | Kladské pomezí |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ACO53 |

## Válečné hroby ve vlastnictví kraje

> Datová sada s prostorovou lokalizací zejména pomníků války 1866, které jsou v majetku Královéhradeckého kraje a jsou svěřené do správy Muzea východních Čech v Hradci Králové. Sada dále obsahuje několik dalších pomníků a pamětních desek 1. a 2. světové války umístěných v objektech škol zřizovaných Královéhradeckým krajem nebo na pozemcích Správy silnic KHK. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Památky, Hroby, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **93** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/V%C3%A1le%C4%8Dn%C3%A9_hroby_ve_vlastnictv%C3%AD_kraje/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| charakter_hrobu | Charakter hrobu | String | Pietní místo - objekt |
| pocet_pohrbenych | Počet pohřbených | Double | 16 |
| historicka_udalost | Historická událost | String | 1. světová válka |
| popis | Popis | String | Pomník ve tvaru obelisku, zchátralý, text špatně čitelný: De |
| autor | Autor | String |   |
| vznik | Rok vzniku | String |   |
| cevh_id | Číslo z Centrální evidence válečných hrobů | String | CZE5201-53868 |
| cislo_hrobu_jine | Číslo hrobu jiné | String |   |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Broumov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5201 |
| nazev_obce | Název obce | String | Teplice nad Metují |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 574538 |
| katastr | Katastrální území | String | Lachov |
| cislo_parcely | Číslo parcely | String | 693/1 |
| popis_lokality | Popis lokality | String | v zatáčce u silnice směr Pěkov |
| vlastnik_lokality | Vlastník lokality | String | Královéhradecký kraj, Pivovarské nám. 1245,  500 03 Hradec K |
| vlastnik_hrobu | Vlastník hrobu | String | Správa silnic Královéhradeckého kraje, Kutnohorská 59/23, Pl |
| verejne_misto | Veřejně přístupné místo | String | Ano |
| cislo_kulturni_pamatky | Číslo kulturní památky z Ústředního seznamu kulturních památek ČR | String |  |
| poznamka | Poznámka | String | Původní výzdoba na obelisku, patrně kartuš, zničena |
| www_pk | Webové stránky Památkového katalogu | String |   |
| www_cevh | Webové stránky Centrální evidence válečných hrobů | String | http://www.evidencevh.army.cz/evidence/detail-hrobu-ci-mista |
| fotografie_1 | Fotografie v rozlišení 2000x1500 pixelů | String | https://photos.app.goo.gl/KV6Z1wyVVdJTQmUb9 |
| fotografie_2 | Fotografie v rozlišení 200x150 pixelů | String | https://photos.app.goo.gl/YHgAdeWACfKqDaRq8 |
| wkt | Zápis vektorové geometrie | String | POINT(50.5801 16.1912) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.1912 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.5801 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | VHKHK1 |

## Zpráva a datové karty 2021

> Výroční zprávy o stavu kraje v přehledných infografikách.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Přírodní parky

> Datová sada s prostorovou lokalizací přírodních parků na území Královéhradeckého kraje. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Přírodní parky, Životní prostředí, Ochrana životního prostředí, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **5** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99%C3%ADrodn%C3%AD_parky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Údolí Rokytenky a Hvězdné |
| dokument | Dokument | String | nařízení č. 4/2002 |
| vyhlasovatel | Vyhlašovatel | String | Okresní úřad Rychnov nad Kněžnou |
| prohlaseni | Datum prohlášení za přírodní park | String | 27.12.2002 |

## Zpráva a datové karty 2020

> Výroční zpráva o stavu kraje v přehledných infografikách.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Výjezdové základny zdravotnické záchranné služby

> Datová sada s prostorovou lokalizací výjezdových základen Zdravotnické záchranné služby Královéhradeckého kraje podle oblasti plošného pokrytí. Zdrojem dat je Zdravotnická záchranná služba Královéhradeckého kraje.

Klíčová slova: Zdravotnická záchranná služba, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **16** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/V%C3%BDjezdov%C3%A9_z%C3%A1kladny_zdravotnick%C3%A9_z%C3%A1chrann%C3%A9_slu%C5%BEby/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| oblast | Oblast plošného pokrytí kraje výjezdovými skupinami ZZS KHK | String | Hradec Králové |
| vyjezdova_zakladna | Název výjezdové základny | String | Hlavní budova |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Hradecká |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1690 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String | 2 |
| psc | PSČ | String | 50012 |
| www | Webové stránky | String | https://www.zzskhk.cz/vedeni-zzs-a-vyjezdove-zakladny-hradec |
| wkt | Zápis vektorové geometrie | String | POINT(50.1984  15.8337) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8337 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.1984 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ZZS1 |

## Plochy a koridory pro veřejně prospěšné stavby, veřejně prospěšná opatření a asanace dle ZÚR – bodové

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Veřejně prospěšné stavby, Veřejně prospěšná opatření, Asanace, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **13** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Plochy_a_koridory/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Kraj | Název kraje | String | Královéhradecký kraj |
| Kraj_kod | Kód kraje dle RÚIAN | Integer | 86 |
| Plati_od | Datum nabytí účinnosti příslušné verze ZÚR | Date | 1618531200000 |
| RC_ZUR | Registrační číslo příslušné verze ZÚR v systému iKAS | Integer | 641 |
| Ident | Identifikátor (označení) plochy v daných ZÚR | String | TR1 |
| Typ | Typ plochy z hlediska možnosti vyvlastnění | String | V |
| Ucel | Účel možného vyvlastnění | String | T |
| Popis | Popis záměru | String | RS Česká Skalice |
| Jev | Jev A118 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů | String | A118 |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String |   |
| Zdroj | Název nebo popis zdroje informace | String | ÚZ ZÚR po vydání změn č. 1,2,3,4,5 |
| Dat_Aktual | Datum aktualizace údaje | Date | 1683590400000 |
| Poznamka | Poznámka | String |   |

## Přijímací řízení na vyšší odborné školy

> Datová sada s prostorovou lokalizací přijímacích řízení na vyšší odborné školy v Královéhradeckém kraji. Datová sada obsahuje údaje týkající se přijímacího řízení a další údaje. Data poskytl Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: VOŠ, Přijímací řízení, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **19** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99ij%C3%ADmac%C3%AD_%C5%99%C3%ADzen%C3%AD_na_vy%C5%A1%C5%A1%C3%AD_odborn%C3%A9_%C5%A1koly/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název školy | String | Česká lesnická akademie Trutnov - střední škola a vyšší odbo |
| zrizovatel | Zřizovatel | String | kraj |
| ico | IČO | String | 60153296 |
| pravni_forma | Právní forma | String | příspěvková organizace |
| redizo | Resortní identifikátor právnické osoby | String | 600012905 |
| izo | Identifikační znak organizace | String | 110029283 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Trutnov |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579025 |
| nazev_ulice | Název ulice | String | Lesnická |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 9 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String | 0 |
| obor_kod | Kód studijního oboru | String | 41-32-N/01 |
| obor_nazev | Název studijního oboru | String | Lesnictví |
| vzdelavani_delka | Délka vzdělávání v letech | String | 3 |
| zpusob_ukonceni | Způsob ukončení vzdělávání | String | absolutorium |
| pocet_prihlasenych_do_1_kola | Počet přihlášených do 1. kola přijímacího řízení pro školní rok 2024/2025 | Double | 0 |
| pocet_prijatych_po_1_kole | Počet přijatých po 1. kole přijímacího řízení pro školní rok 2024/2025 | Double | 0 |
| pocet_nabizenych_mist | Počet nabízených míst pro školní rok 2025/2026 | Double | 25 |
| rocni_skolne | Roční školné | Double | 2000 |
| prijimaci_zkouska | Přijímací zkouška | String | PZ |
| forma_vzdelavani | Forma vzdělávání | String | denní |
| druh_vzdelavani | Druh vzdělávání | String | VOŠ |
| obor_ukoncen_pro_uchazece | Obor ukončen pro uchazeče | String | s maturitní zkouškou |
| obor_vhodny_pro_ozp | Obor vhodný pro osoby se zdravotním postižením | String | NE |
| povinna_lekarska_prohlidka | Povinná lékařská prohlídka | String | ANO |
| poznamka | Poznámka | String |   |
| psc | PSČ | String | 541 01 |
| www | Webové stránky | String | www.clatrutnov.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.5585669 15.9119539) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.911954 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.558567 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PRVOS1 |

## Opravy silniční infrastruktury od roku 2016 - liniové

> Datová sada s prostorovou lokalizací oprav silniční infrastruktury od roku 2015 vyjádřených liniově. Jedná se o silnice II. a III. třídy, tedy těch, které jsou ve vlastnictví Královéhradeckého kraje. Zdrojem dat je ÚDRŽBA SILNIC Královéhradeckého kraje a.s.

Klíčová slova: Silnice, Opravy, Královéhradecký kraj

- Geometrie: esriGeometryPolyline | Záznamů: **998** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Opravy_silni%C4%8Dn%C3%AD_infrastruktury_od_roku_2016_liniov%C3%A9/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| rok_real | Rok realizace | String | 2020 |
| ozn_sil | Silnice | String | 03535 |
| ozn_usek | Úsek | String | 1 |
| ozn_sil_usek | Označení silnice s úsekem | String | 03535.1 |
| ozn_trida | Třída | String | 3. třída |
| ozn_kat | Kategorie | String | komunikace s místním významem |
| delka | Délka v metrech | Double | 999 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód výššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | OSIL511 |

## Odborné léčebné ústavy

> Datová sada s prostorovou lokalizací odborných léčebných ústavů na území Královéhradeckého kraje. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Léčba, Ústavy, Zdravotnictví, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **6** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Odborn%C3%A9_l%C3%A9%C4%8Debn%C3%A9_%C3%BAstavy/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| druh_zarizeni | Druh zařízení | String | Rehabilitační ústav |
| poskytoval | Poskytoval | String | Lázně Bělohrad a.s. |
| ico | IČO | String | 46504834 |
| zrizovatel | Zřizovatel | String | Jiná právnická osoba |
| forma | Forma zdravotní péče | String | následná lůžková péče - rehabilitační a fyzikální medicína |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Lázně Bělohrad |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573094 |
| nazev_ulice | Název ulice | String | Lázeňská |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 640 |
| typ_cisla_domovniho | Typ čísla domovního | String | cp |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50781 |
| www | Webové stránky | String | http://www.belohrad.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.4303492 15.5912492) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.5912492 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4303492 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | OLU1 |

## Letecké sporty

> Datová sada s prostorovou lokalizací areálů určených pro letecký sport na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

Klíčová slova: Letecké sporty, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **17** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Leteck%C3%A9_sporty/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Aeroklub Dvůr Králové nad Labem |
| region | Region | String | Dvůr Králové nad Labem / Podkrkonoší |
| primarni_zatrideni | Primární zatřídění | String | Sport a relaxace / Letecké sporty |
| sekundarni_zatrideni | Sekundární zatřídění | String |  |
| tercialni_zatrideni | Terciální zatřídění | String |  |
| popis | Popis | String | Historie létání ve Dvoře Králové nad Labem sahá do začátku m |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Dvůr Králové nad Labem |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5203 |
| nazev_obce | Název obce | String | Dvůr Králové nad Labem |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579203 |
| nazev_ulice | Název ulice | String | Letiště Žireč |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 152 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54404 |
| www | Webové stránky | String | www.akdk.cz |
| telefon_1 | Telefon č. 1 | String | 420 499 622 602 |
| telefon_2 | Telefon č. 2 | Double |  |
| mobil_1 | Mobil č. 1 | Double |  |
| mobil_2 | Mobil č. 2 | Double |  |
| fax | Fax | Double |  |
| email_1 | Email č. 1 | String | letiste@akdk.cz |
| email_2 | Email č. 2 | String |  |
| wkt | Zápis vektorové geometrie | String | POINT(50.41423806 15.83705694) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.83705694 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.41423806 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | LS1 |

## Lékařská pohotovostní služba

> Datová sada s prostorovou lokalizací lékařských pohotovostí pro děti a dospělé (mimo stomatologickou lékařskou pohotovost) v Královéhradeckém kraji. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Lékařská pohotovostní služba, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **11** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/L%C3%A9ka%C5%99sk%C3%A1_pohotovostn%C3%AD_slu%C5%BEba/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Fakultní nemocnice Hradec Králové, Pavilon č. 50 Emergency,  |
| cilova_skupina | Cílová skupina | String | dospělí |
| ordinacni_hodiny | Ordinační hodiny | String | všední den: 16:00 – 22:00, SO,NE, svátek: 08:00 – 22:00 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Sokolská |
| cislo_domovni | Číslo domovní | String | 581 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| psc | PSČ | String | 500 05 |
| www | Webové stránky | String | https://www.fnhk.cz/pacient-a-jeho-blizci/lekarska-pohotovos |
| wkt | Zápis vektorové geometrie | String | POINT(50.1983 15.8285) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8285 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.1983 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | LPS1 |

## Rozcestník portálů

> Rozcestník všech webových portálů Královéhradeckého kraje.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Plochy jejichž využití je podmíněno zpracováním regulačního plánu nebo územní studie v ZÚR

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR). Jedná se o nově vytvořenou vrstvu, jejíž přesná definice bude upřesněna v aktualizované verzi datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Územní studie, Regulační plány, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **1** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Plochy_jejich%C5%BE_vyu%C5%BEit%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| CISLO | Cislo záměru | String | US01 |
| TYP | Typ záměru | String | US |
| Jev | Jev A119 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů | String | prověření změn územní studií |
| POPIS | Popis záměru | String | územní studie jihozápadního obchvatu města Nové Město nad Me |

## Hackathon 2026 partneři



_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Hospodářsky a sociálně ohrožené oblasti

> Datová sada s prostorovou lokalizací hospodářsky a sociálně ohrožených oblastí na území Královéhradeckého kraje. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: HSOO, Ohrožené oblasti, Regionální rozvoj, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **448** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hospod%C3%A1%C5%99sky_a_soci%C3%A1ln%C4%9B_ohro%C5%BEen%C3%A9_oblasti/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| hsoo | Hospodářsko-sociálně ohrožená oblast | String | ano |
| urov_ohr | Úroveň ohrožení | String | střední míra ohrožení |
| nazev_obce | Název obce | String | Červená Hora |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 505099 |
| nazev_pou | Název správního obvodu obce s pověřeným obecním úřadem | String | Červený Kostelec |
| kod_pou | Kód správního obvodu obce s pověřeným obecním úřadem dle číselníku ČSÚ | String | 52091 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Náchod |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5209 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| turist_reg | Turistický region dle agentury CzechTourism | String | Kladské pomezí |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ACO53 |

## Rybaření

> Datová sada s prostorovou lokalizací areálů pro rybaření na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

Klíčová slova: Rybaření, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **30** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Ryba%C5%99en%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Český rybářský svaz - Východočeský územní svaz  |
| region | Region | String | Hradec Králové / Hradecko |
| primarni_zatrideni | Primární zatřídění | String | Sport a relaxace / Rybaření |
| sekundarni_zatrideni | Sekundární zatřídění | String |  |
| tercialni_zatrideni | Terciální zatřídění | String |  |
| popis | Popis | String | Povolenky získáte u příslušné místní organizace rybářského s |
| nazev_kraje | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_kraje | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Kovová |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1121 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| psc | PSČ | String | 50003 |
| telefon | Telefon | String | 420 495 214 940 |
| mobil_1 | Mobil č. 1 | String |  |
| mobil_2 | Mobil č. 2 | Double |  |
| fax | Fax | String |  |
| email_1 | Email č. 1 | String | info@crshradec.cz |
| email_2 | Email č. 2 | String |  |
| www | Webové stránky | String | www.vcus.regis.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.22629694 15.86816889) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.86816889 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.22629694 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | RYB1 |

## Stav povrchu silnic

> Datová sada s prostorovou lokalizací stavu povrchu silnic ve vlastnictví Královéhradeckého kraje, tedy silnic II. a III. třídy. Zdrojem dat je ÚDRŽBA SILNIC Královéhradeckého kraje a. s.

Klíčová slova: Slinice, Povrch, Stav, Královéhradecký kraj

- Geometrie: esriGeometryPolyline | Záznamů: **4888** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Stav_povrchu_silnic/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| stav_sil | Stav povrchu silnice | String | výborný |
| rok | Stav povrchu silnice k uvedenému roku | String | 2022 |
| ozn_sil | Silnice | String | 03535 |
| ozn_usek | Úsek | String | 1 |
| ozn_sil_usek | Označení silnice s úsekem | String | 03535.1 |
| ozn_trida | Třída | String | 3. třída |
| ozn_kat | Kategorie | String | komunikace s místním významem |
| delka | Délka v metrech | Double | 866 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód výššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | OSIL297 |
| typ_povrchu | Typ povrchu | String | asfaltobetonový |

## Inovační infrastruktury

> Datová sada s prostorovou lokalizací inovačních infrastruktur. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

Klíčová slova: Inovační infrastruktury, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **8** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Inova%C4%8Dn%C3%AD_infrastruktury/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Centrum inovací a podnikání Trutnov |
| ico | IČO | String | 05146828 |
| pravni_forma | Právní forma | String | spolek |
| vlastnictvi | Typ vlastnictví | String | soukromé |
| vedeckotechnicky_park | Vědeckotechnický park | String | ne |
| typ_inovacniho_centra | Inkubátor, akcelerátor nebo inovační centrum | String | ne |
| cowork | Sdílený pracovní prostor | String | ano |
| otevrena_dilna | Otevřená dílna | String | ne |
| aktivni | Aktivní inovační infrastruktura | String | ne |
| pasivni | Pasivní inovační infrastruktura | String | ano |
| nefunkcni | Nefunkční inovační infrastruktura | String | ne |
| dotace_op_mpo | Dotace z operačního pogramu Ministerstva průmyslu a obchodu | String | ne |
| dotace_op_msmt | Dotace z operačního pogramu Ministerstva školství, mládeže a tělovýchovy | String | ne |
| dotace_op_mpsv | Dotace z operačního pogramu Ministerstva práce a sociálních věcí | String | ne |
| dotace_op_praha | Dotace z operačního pogramu Hlavního města Prahy | String | ne |
| dotace_kraje | Krajské dotace | String | ne |
| dotace_jine | Jiné dotace | String | ano |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Trutnov |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579025 |
| nazev_ulice | Název ulice | String | Nádražní |
| cislo_domovni | Číslo domovní | String | 189 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54101 |
| www | Webové stránky | String | facebook.com/CIPTrutnov/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.564 15.9104) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.9104 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.564 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | II1 |

## Právnické osoby v klastrech

> Datová sada s prostorovou lokalizací právnických osob v klastrech. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

Klíčová slova: Klastry, Právnické osoby, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **31** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pr%C3%A1vnick%C3%A9_osoby_v_klastrech/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_klastru | Název klastru | String | Klastr výrobců obalů, družstvo (OMNIPACK) |
| ico_klastru | IČO klastru | String | 27483274 |
| nazev_firmy | Název firmy | String | YATE spol. s r.o. |
| ico_firmy | IČO firmy | String | 43226990 |
| pravni_forma | Právní forma | String | Společnost s ručením omezeným |
| vznik | Datum vzniku | String | 11.11.1991 |
| kategorie_poctu_zamestnancu | Kategorie počtu zaměstnanců | String | 50 - 99  |
| cz_nadace | Typ odvětví | String | Výroba plastových obalů |
| kod_cz_nadace | Kód typu odvětví | String | 222200 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Brněnská  |
| cislo_domovni | Číslo domovní | String | 371 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50006 |
| www | Webové stránky | String | https://www.yate.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.1922 15.851) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.851 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.1922 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | FVK1 |

## Aktivní zóna záplavového území stoleté vody

> Datová sada s prostorovou lokalizací aktivní zóny záplavového území stoleté vody. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje. Data stažena zVodohospodářského informačního portálu VODA https://voda.gov.cz/ Průběh polygonů záplavových území v této vrstvě je pouze ORIENTAČNÍ, jedná se o kompilaci dat jednotlivých dílčích záplavových území, v rámci které byly provedeny úpravy (vyhlazení) geometrie v místě návaznosti jednotlivých částí. Pro závazné informace ohledně průběhu polygonů kontaktujte správce daného vodního toku nebo místě příslušný vodoprávní úřad.

Klíčová slova: Aktivní zóna, Záplava, Povodeň, Stoletá voda, Q100, Životní prostředí, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **63** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Aktivn%C3%AD_z%C3%B3na_z%C3%A1plavov%C3%A9ho_%C3%BAzem%C3%AD_stolet%C3%A9_vody/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_tok | Název vodního toku | String | Chaloupská svodnice |
| id_tok | Označení vodního toku | Double | 104470000100 |
| nazev_tok_vyssiho_radu | Název vodního toku, do kterého se daný vodní tok vlévá | String | Labský náhon |
| id_tok_vyssiho_radu | Označení vodního toku, do kterého se daný vodní tok vlévá | Double | 104450000100 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | AZQSTO1 |

## Záplavové území pětisetleté vody

> Datová sada s prostorovou lokalizací záplavového území pětisetleté vody vody. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje. Data stažena zVodohospodářského informačního portálu VODA https://voda.gov.cz/ Průběh polygonů záplavových území v této vrstvě je pouze ORIENTAČNÍ, jedná se o kompilaci dat jednotlivých dílčích záplavových území, v rámci které byly provedeny úpravy (vyhlazení) geometrie v místě návaznosti jednotlivých částí. Pro závazné informace ohledně průběhu polygonů kontaktujte správce daného vodního toku nebo místě příslušný vodoprávní úřad.

Klíčová slova: Záplava, Povodeň, Pětisetletá voda, Q500, Životní prostředí, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **41** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1plavov%C3%A9_%C3%BAzem%C3%AD_p%C4%9Btisetlet%C3%A9_vody/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_tok | Název vodního toku | String | Javorka |
| id_tok | Označení vodního toku | Double | 108740300100 |
| nazev_tok_vyssiho_radu | Název vodního toku, do kterého se daný vodní tok vlévá | String | Cidlina |
| id_tok_vyssiho_radu | Označení vodního toku, do kterého se daný vodní tok vlévá | Double | 108540000100 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | QPETSET1 |

## Solné jeskyně

> Datová sada s prostorovou lokalizací solných jeskyní na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

Klíčová slova: Solné jeskyně, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **15** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Soln%C3%A9_jeskyn%C4%9B/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Penzion Martin |
| region | Region | String | Janské Lázně / Krkonoše |
| primarni_zatrideni | Primární zatřídění | String | Ubytování a stravování / Penziony |
| sekundarni_zatrideni | Sekundární zatřídění | String | Sport a relaxace / Wellness a relaxace |
| tercialni_zatrideni | Terciální zatřídění | String | Sport a relaxace / Solné jeskyně |
| popis | Popis | String | Penzion je vzdálen 20 metrů od lyžařských vleků skiareálu SK |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Janské Lázně |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579351 |
| nazev_ulice | Název ulice | String | Sportovní |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 205 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54225 |
| www | Webové stránky | String | www.penzionmartin.cz |
| telefon_1 | Telefon č. 1 | Double |  |
| telefon_2 | Telefon č. 2 | String |  |
| mobil_1 | Mobil č. 1 | Double | 420731495600 |
| mobil_2 | Mobil č. 2 | Double |  |
| fax | Fax | String |  |
| email_1 | Email č. 1 | String | info@penzionmartin.cz |
| email_2 | Email č. 2 | String |  |
| wkt | Zápis vektorové geometrie | String | POINT(50.6353000015.78222583) |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.6353 |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.78222583 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | SJ1 |

## Dobrovolné svazky obcí

> Datová sada s prostorovou lokalizací dobrovolných svazků obcí na území Královéhradeckého kraje. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: DSO, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **32** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Dobrovoln%C3%A9_svazky_obc%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Svazek obcí Východní Krkonoše |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | DSO32 |

## Hranice správních obvodů obcí s pověřeným obecním úřadem v Královéhradeckém kraji

> Datová sada s prostorovým vymezením správních obvodů obcí s pověřeným obecním úřadem v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Hranice, POÚ, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **35** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hranice_spr%C3%A1vn%C3%ADch_obvod%C5%AF_obc%C3%AD_s_pov%C4%9B%C5%99en%C3%BDm_obecn%C3%ADm_%C3%BA%C5%99adem_v_Kr%C3%A1lov%C3%A9hradeck%C3%A9m_kraji/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_pou | Název správního obvodu obce s pověřeným obecním úřadem | String | Chlumec nad Cidlinou |
| kod_pou | Kód správního obvodu obce s pověřeným obecním úřadem dle číselníku ČSÚ | String | 52052 |
| pozn | Poznámka | String | Hranice správního obvodu obce s pověřeným obecním úřadem |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | POU8 |

## Kontakty, tým

> Za portálem Data KHK stojí celý tým složený ze zaměstnanců Krajského úřadu Královéhradeckého kraje, CIRI a externistů.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Plochy a koridory pro veřejně prospěšné stavby, veřejně prospěšná opatření a asanace dle ZÚR – plošné

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Veřejně prospěšné stavby, Veřejně prospěšná opatření, Asanace, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **608** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/VpsVpoAs_ZUR_p/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Kraj | Název kraje | String | Královéhradecký kraj |
| Kraj_kod | Kód kraje dle RÚIAN  | Integer | 86 |
| Plati_od | Datum nabytí účinnosti příslušné verze ZÚR | Date | 1683590400000 |
| RC_ZUR | Registrační číslo příslušné verze ZÚR v systému iKAS | Integer | 641 |
| Ident | Identifikátor (označení) plochy v daných ZÚR | String | TE2 |
| Typ | Typ plochy z hlediska možnosti vyvlastnění | String | V |
| Ucel | Účel možného vyvlastnění | String | T |
| Popis | Popis záměru | String | koridor nadzemního vedení 2x110 kV Librantice – Svinary – Hr |
| Jev | Jev A118 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů | String | A118 |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String |   |
| Zdroj | Název nebo popis zdroje informace | String | ÚZ ZÚR po vydání změn č. 1,2,3,4,5 |
| Dat_Aktual | Datum aktualizace údaje | Date | 1685404800000 |
| Poznamka | Poznámka | String |   |

## Adresář obcí

> Datová sada s prostorovou lokalizací adresářů na jednotlivé obecní/městské úřady obcí v Královéhradeckém kraji. Zdrojem dat je Odbor kancelář ředitele Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Adresář obcí, Obecní úřady, Městské úřady, Kontakty, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **448** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Adres%C3%A1%C5%99_obc%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_obce | Název obce | String | Červená Hora |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 505099 |
| ds | Datová schránka | String | 65ra6gy |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Náchod |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5209 |
| nazev_ulice | Název ulice | String | Červená Hora |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 58 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |   |
| psc | PSČ | String | 54941 |
| posta | Pošta | String | Červený Kostelec |
| www | Webové stránky | String | www.cervenahora.cz |
| telefon_1 | Telefon č. 1 | String | 775862317 |
| telefon_2 | Telefon č. 2 | String |   |
| telefon_3 | Telefon č. 3 | String | 602705408 |
| telefon_4 | Telefon č. 4 | String |   |
| email_1 | Email č. 1 | String | obec@cervenahora.cz |
| email_2 | Email č. 2 | String |   |
| email_3 | Email č. 3 | String | mistostarosta@cervenahora.cz |
| email_4 | Email č. 4 | String |   |
| wkt | Zápis vektorové geometrie | String | POINT(50.4500684 16.05882152) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.05882152 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4500684 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | AO53 |

## Stanice a pracoviště hasičského záchranného sboru

> Datová sada s prostorovou lokalizací stanic a pracovišť Hasičského záchranného sboru ČR v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Hasičský záchranný systém, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **22** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Stanice_a_pracovi%C5%A1t%C4%9B_hasi%C4%8Dsk%C3%A9ho_z%C3%A1chrann%C3%A9ho_syst%C3%A9mu/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| druh_pracoviste | Druh pracoviště HZS | String | Krajské operační a informační středisko |
| typ_stanice | Typ stanice HZS dle Vyhlášky 247/2001 Sb. | String |   |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Kralovehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Pražská |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 230 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String | 153z |
| psc | PSČ | String | 50004 |
| www | Webové stránky | String | https://www.hzscr.cz/clanek/hzs-kralovehradeckeho-kraje-menu |
| telefon | Telefon | String | +420950530100 |
| email | Email | String | sekretariat@hkk.izscr.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.2013 15.7779) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.7779 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2013 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | HZS3 |

## Hrady

> Datová sada s prostorovou lokalizací hradů na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Hrady, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **21** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hrady/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Hrad Kost |
| popis | Popis | String | Hrad Kost je jedním z nejzachovalejších vrcholně gotických h |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Libošovice |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573116 |
| nazev_ulice | Název ulice | String | Podkost |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50601 |
| www | Webové stránky | String | http://www.kost-hrad.cz |
| telefon | Telefon | String | +420607106324 |
| wkt | Zápis vektorové geometrie | String | POINT(50.4902 15.1353) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.1353 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4902 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | HRAD1 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 1190 |

## Hranice správních obvodů obcí s rozšířenou působností v Královéhradeckém kraji

> Datová sada s prostorovým vymezením správních obvodů obcí s rozšířenou působností v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Hranice, ORP, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **15** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hranice_spr%C3%A1vn%C3%ADch_obvod%C5%AF_obc%C3%AD_s_roz%C5%A1%C3%AD%C5%99enou_p%C5%AFsobnost%C3%AD_v_Kr%C3%A1lov%C3%A9hradeck%C3%A9m_kraji/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| pozn | Poznámka | String | Plošné vymezení správního obvodu obce s rozšířenou působnost |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ORP5 |

## Židovské památky

> Datová sada s prostorovou lokalizací židovských památek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Židovské památky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **14** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/%C5%BDidovsk%C3%A9_pam%C3%A1tky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Synagoga a židovský hřbitov v Rychnově nad Kněžnou |
| popis | Popis | String | Synagoga pocházející ze 17. století byla zničena požárem. |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Rychnov nad Kněžnou |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0524 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Rychnov nad Kněžnou |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5213 |
| nazev_obce | Název obce | String | Rychnov nad Kněžnou |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 576069 |
| nazev_ulice | Název ulice | String | Fischerova  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 602 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 51601 |
| www | Webové stránky | String | https://www.rychnov-city.cz/synagoga/d-3314%3C/ |
| telefon | Telefon | String | +420494534450 |
| wkt | Zápis vektorové geometrie | String | POINT(50.1627 16.2762) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.2762 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.1627 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ZIDP9 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 100610 |

## Schválené programové dotace do okresů v letech 2021 - 2025

> Datová sada s prostorovou lokalizací schválených programových dotací Královéhradeckého kraje do území okresů v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dotace, Programové dotace, Okresy, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **5** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_programov%C3%A9_dotace_do_okres%C5%AF/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| poc_dotaci | Počet dotací | Double | 1758 |
| finance | Množství financí v Kč | Double | 248752623 |
| dp_id | Jedinečný identifikátor v katalagu otevřených dat Data KHK | String | PDOTOKR1 |

## Knihovny

> Datová sada s prostorovou lokalizací knihoven na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Knihovny, Kultura, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **434** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Knihovny/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Biskupská knihovna Biskupství královéhradeckého |
| ico | IČO | String | 00445134 |
| evidencni_cislo | Evidenční číslo | String | 6331 |
| zrizovatel | Zřizovatel | String | Biskupství královéhradeckého |
| typ_knihovny | Typ knihovny | String | církevní/farní |
| bezbarierovost | Bezbariérovost | String |  |
| strediskovost | Střediskovost | String | ne |
| pravni_forma | Právní forma | String |  |
| pocet_pobocek | Počet poboček | Double |  |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Velké náměstí |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 35 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String | 44 |
| psc | PSČ | String | 50003 |
| www | Webové stránky | String | https://www.bihk.cz/vzdelani-media/biskupska-knihovna |
| wkt | Zápis vektorové geometrie | String | POINT(50.2087 15.8325) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8325 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2087 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | KNIH1 |

## Kultura, cestovní ruch

> Stránka slouží jako rozcestník pro vybrané ukazatele o Královéhradeckém kraji z oblasti kulturu a cestovního ruchu

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Strategická mapa kraje

> Přehledný seznam všech strategických a koncepčních dokumentů Královéhradeckého kraje na jednom místě.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Plán zimní údržby silnic

> Datová sada s prostorovou lokalizací plánu zimní údržby silnic ve vlastnictví Královéhradeckého kraje. Zdrojem dat je ÚDRŽBA SILNIC Královéhradeckého kraje a. s.

Klíčová slova: Zimní údržba, Silnice, Královéhradecký kraj

- Geometrie: esriGeometryPolyline | Záznamů: **1660** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pl%C3%A1n_zimn%C3%AD_%C3%BAdr%C5%BEby_silnic/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| typ_tech | Typ použité technologie | String | pluhování |
| ozn_sil | Označení silnice | String | 29835 |
| ozn_trida | Označení třídy silnice | String | III. třída |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PZU411 |

## ZOO a zooparky

> Datová sada s prostorovou lokalizací zoologických zahrad a zooparků na území Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: ZOO, Zooparky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **2** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/ZOO_a_zooparky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Safari Park Dvůr Králové |
| typ | Typ | String | zoologická zahrada |
| ico | IČO | String | 27478246 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Dvůr Králové nad Labem |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5203 |
| nazev_obce | Název obce | String | Dvůr Králové nad Labem |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579203 |
| nazev_ulice | Název ulice | String | Štefánikova |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1029 |
| typ_cisla_domovniho | Typ čísla domovního | String | cp |
| psc | PSČ | String | 54401 |
| telefon | Telefon | String | 770123218 |
| email | Email | String | info@zoodvurkralove.cz |
| www | Webové stránky | String | https://safaripark.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.4346722 15.7987117) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.7987117 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4346722 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ZOO1 |

## Schválené individuální a programové dotace do kraje v letech 2021 - 2025

> Datová sada s prostorovou lokalizací schválených individuálních a programových dotací Královéhradeckého kraje do území Královéhradeckého kraje v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dotace, Individuální dotace, Programové dotace, Kraj, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **1** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_a_programov%C3%A9_dotace_do_kraje/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| poc_dotaci | Počet dotací | Double | 9686 |
| finance | Množství financí v Kč | Double | 2920260718 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | IPDOT1 |

## Schválené individuální dotace do ORP v letech 2021 - 2025

> Datová sada s prostorovou lokalizací schválených individuálních dotací Královéhradeckého kraje do území správních obvodů obcí s rozšířenou působností v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dotace, Individuální dotace, ORP, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **15** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_dotace_do_ORP/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| poc_dotaci | Počet dotací | Double | 752 |
| finance | Množství financí v Kč | Double | 539136988 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | IDOTORP1 |

## Golfová hřiště

> Datová sada s prostorovou lokalizací golfových hřišť v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Golf, Hřiště, Sport, Cestovní ruch, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **9** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Golfov%C3%A1_h%C5%99i%C5%A1t%C4%9B/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Golf & Country Club Prosečné |
| pocet_jamek | Počet jamek | Integer | 9 |
| par | Par golfového hřiště | Integer | 70 |
| verejne_hriste | Otevřeno pro veřejnost | String | ne |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Vrchlabí |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5215 |
| nazev_obce | Název obce | String | Prosečné |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579602 |
| nazev_ulice | Název ulice | String | Prosečné |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 179 |
| typ_cisla_domovniho | Typ čísla domovního | String | c.p. |
| psc | PSČ | String | 54373 |
| telefon | Telefon | String | +420732358500 |
| email | Email | String | golfcountry.prosecne@seznam.cz |
| www | Webové stránky | String | https://www.gccprosecne.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.569140 15.678720) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.67872 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.56914 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | GH1 |

## Národní kulturní památky - liniové objekty

> Datová sada s prostorovou lokalizací liniových objektů (zdi, terasy, brány), které jsou součástí národních kulturních památek na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

Klíčová slova: Národní kulturní památky, Liniové objekty, Památky, Královéhradecký kraj

- Geometrie: esriGeometryPolyline | Záznamů: **16** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/N%C3%A1rodn%C3%AD_kulturn%C3%AD_pam%C3%A1tky_liniov%C3%A9_objekty/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | zámek Opočno |
| objekt | Předmět ochrany památkové péče | String | ohrazení parku |
| ico | IČO | String | 75032333  |
| pravni_forma | Právní forma | String | Příspěvková organizace |
| uskp | Rejstříkové číslo v Ústředním seznamu kulturních památek | String | 37808/6-2351 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Rychnov nad Kněžnou |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0524 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Dobruška |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5202 |
| nazev_obce | Název obce | String | Opočno |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 576590 |
| nazev_ulice | Název ulice | String | Trčkovo náměstí |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |   |
| psc | PSČ | String | 51773 |
| www | Webové stránky | String | https://pamatkovykatalog.cz/zamek-opocno-1192320 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | NKPL11 |

## Pevnosti a opevnění

> Datová sada s prostorovou lokalizací pevností a opevnění na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Pevnosti a opevnění, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **38** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pevnosti_a_opevn%C4%9Bn%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Městské opevnění Jičín |
| popis | Popis | String | Z pozdně gotického opevnění města se dodnes zachovaly zbytky |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Jičín |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 572659 |
| nazev_ulice | Název ulice | String |  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String |  |
| typ_cisla_domovniho | Typ čísla domovního | String |  |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50601 |
| www | Webové stránky | String |  |
| telefon | Telefon | String | +420493534390 |
| wkt | Zápis vektorové geometrie | String | POINT(50.4357 15.3519) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.3519 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4357 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PEVN1 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 889 |

## Národní kulturní památky - plošné objekty

> Datová sada s prostorovou lokalizací plošných objektů (budovy), které jsou součástí národních kulturních památek na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

Klíčová slova: Národní kulturní památky, Plošné objekty, Památky, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **129** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/N%C3%A1rodn%C3%AD_kulturn%C3%AD_pam%C3%A1tky_plo%C5%A1n%C3%A9_objekty/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | zámek Humprecht |
| objekt | Předmět ochrany památkové péče | String | zámek Humprecht |
| ico | IČO | String | 272124 |
| pravni_forma | Právní forma | String | Obec nebo městská část hlavního města Prahy |
| uskp | Rejstříkové číslo v Ústředním seznamu kulturních památek | String | 37630/6-1386 |
| ucinnost | Datum účinnosti prohlášení za národní kulturní památku | String | 1. července 2008 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Sobotka |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573493 |
| nazev_ulice | Název ulice | String |   |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 363 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |   |
| psc | PSČ | String | 50743 |
| www | Webové stránky | String | https://www.pamatkovykatalog.cz/zamek-humprecht-13733931 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | NKPPO94 |

## Nemocnice

> Datová sada s prostorovou lokalizací nemocnic na území Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Nemocnice, Zdravotnictví, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **12** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Nemocnice/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Fakultní nemocnice Hradec Králové |
| pobocka | Pobočka | String | bez pobočky |
| zrizovatel | Zřizovatel | String | Ministerstvo zdravotnictví |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Sokolská |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 581 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| psc | PSČ | String | 50005 |
| www | Webové stránky | String | https://www.fnhk.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.1983 15.8285) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8285 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.1983 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | NEM1 |

## Regionální potraviny

> Datová sada s prostorovou lokalizací sídel regionálních potravin na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

Klíčová slova: Regionální potraviny, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **102** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Region%C3%A1ln%C3%AD_potraviny/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Babiččiny sirupy |
| region | Region | String | Vrchlabí / Krkonoše |
| primarni_zatrideni | Primární zatřídění | String | Recepty a produkty / Regionální potraviny |
| sekundarni_zatrideni | Sekundární zatřídění | String |  |
| tercialni_zatrideni | Terciální zatřídění | String |  |
| popis | Popis | String | Babiččiny sirupy jsou zpracovávány za studena, macerováním.  |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Vrchlabí |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5215 |
| nazev_obce | Název obce | String | Vrchlabí |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579858 |
| nazev_ulice | Název ulice | String | Školní |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1396 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54301 |
| telefon_1 | Telefon č. 1 | String |  |
| telefon_2 | Telefon č. 2 | String |  |
| mobil_1 | Mobil č. 1 | String | 420 603 212 286 |
| mobil_2 | Mobil č. 2 | Double |  |
| mobil_3 | Mobil č. 3 | String |  |
| fax | Fax | String |  |
| email_1 | Email č. 1 | String | marek.susko@seznam.cz |
| email_2 | Email č. 2 | String |  |
| email_3 | Email č. 3 | String |  |
| email_4 | Email č. 4 | String |  |
| wkt | Zápis vektorové geometrie | String | POINT(50.61319194 15.61200306) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.61200306 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.61319194 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | RP1 |

## Praktičtí lékaři pro dospělé

> Datová sada s prostorovou lokalizací praktických lékařů pro dospělé. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Praktičtí lékaři, Dospělí, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **314** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Prakti%C4%8Dt%C3%AD_l%C3%A9ka%C5%99i_pro_dosp%C4%9Bl%C3%A9/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| provozovatel | Provozovatel | String | Ordinace praktického lékaře pro dospělé |
| ico | IČO | String | neuvedeno |
| pravni_forma | Právní forma | String | Společnost s ručením omezeným |
| nazev_oboru | Název oboru | String | všeobecné praktické lékařství |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Velké Svatoňovice |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579785 |
| nazev_ulice | Název ulice | String | Velké Svatoňovice |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 210 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String |  542 35 |
| www | Webové stránky | String | https://www.znamylekar.cz/zarizeni/1122-s-r-o-prakticky-leka |
| wkt | Zápis vektorové geometrie | String | POINT(50.5267839  16.0329244) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.0329244 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.5267839 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PLD1 |

## Kulturní památky na indikativním seznamu

> Datová sada s prostorovou lokalizací nemovitých kulturních památek ČR zapsaných na Indikativní seznam národních kulturních památek ČR v Královéhradeckém kraji. Jedná se o seznam kulturních památek, které mají potenciál být prohlášeny národními kulturními památkami. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Kulturní památky, Indikativní seznam, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **13** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Kulturn%C3%AD_pam%C3%A1tky_na_indikativn%C3%ADm_seznamu/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Hlubinný důl - Jáma Julie |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Lampertice |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 548804 |
| nazev_ulice | Název ulice | String |  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String |  |
| typ_cisla_domovniho | Typ čísla domovního | String |  |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 541 01 |
| wkt | Zápis vektorové geometrie | String | POINT(50.6681 15.9289) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.9289 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.6681 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | INKP1 |

## Plochy a koridory pro veřejně prospěšné stavby, veřejně prospěšná opatření a asanace dle ZÚR – liniové

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Veřejně prospěšné stavby, Veřejně prospěšná opatření, Asanace, Královéhradecký kraj

- Geometrie: esriGeometryPolyline | Záznamů: **6** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/VpsVpoAs_ZUR_l/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Kraj | Název kraje | String | Královéhradecký kraje |
| Kraj_kod | Kód kraje dle RÚIAN | Integer | 86 |
| Plati_od | Datum nabytí účinnosti příslušné verze ZÚR | Date | 1618531200000 |
| RC_ZUR | Registrační číslo příslušné verze ZÚR v systému iKAS | Integer | 641 |
| Ident | Identifikátor (označení) plochy v daných ZÚR | String | PPO1 |
| Typ | Typ plochy z hlediska možnosti vyvlastnění | String |   |
| Ucel | Účel možného vyvlastnění | String | R |
| Popis | Popis záměru | String | Dědina, Mělčany, suchá retenční nádrž |
| Jev | Jev A118 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů | String | A118 |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String |   |
| Zdroj | Název nebo popis zdroje informace | String | ÚZ ZÚR po vydání změn č. 1,2,3,4,5 |
| Dat_Aktual | Datum aktualizace údaje | Date | 1683590400000 |

## Kulturní akce pořádané s trvalou záštitou Rady Královéhradeckého kraje 2026–2029

> Kulturní akce pořádané s trvalou záštitou Rady Královéhradeckého kraje 2026–2029. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Trvalé záštity, Rada KHK, Kultura, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **30** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Trval%C3%A9_z%C3%A1%C5%A1tity_Rady_Kr%C3%A1lov%C3%A9hradeck%C3%A9ho_kraje/FeatureServer/4

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Benefice Police Symphony Orchestra |
| zamereni | Zaměření | String | hudební |
| termin | Obvyklý termín konání | String | červen–srpen |
| poradatel | Pořadatel | String | Police Symphony Orchestra, z.s. |
| ico | IČO pořadatele | String | 1510207 |
| popis_akce | Popis akce | String | Skály, louky nebo snové šapitó… Tisíce diváků putují na Brou |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Náchod |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5209 |
| nazev_obce | Název obce | String | Police nad Metují |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 574341 |
| nazev_ulice | Název ulice | String |  |
| cislo_popisne | Číslo popisné nebo evidenční podle typu čísla domovního | String |  |
| cislo_orientační | Číslo orientační | String |  |
| psc | PSČ | String | 54754 |
| www | Webové stránky | String | https://www.policesymphonyorchestra.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.5365778 16.2327908) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.2327908 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.5365778 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | TZRKHK1 |

## Schválené programové dotace do kraje v letech 2021 - 2025

> Datová sada s prostorovou lokalizací schválených programových dotací Královéhradeckého kraje do území Královéhradeckého kraje v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dotace, Programové dotace, Kraj, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **1** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_programov%C3%A9_dotace_do_kraje/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| poc_dotaci | Počet dotací | Double | 6152 |
| finance | Množství financí v Kč | Double | 1042278273 |
| poc_podanych_zad | Počet podaných žádostí | Double | 7067 |
| uspesnost | Úspěšnost podaných žádostí | Double | 87.1 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PDOT1 |

## Schválené individuální a programové dotace do okresů v letech 2021 - 2025

> Datová sada s prostorovou lokalizací schválených individuálních a programových dotací Královéhradeckého kraje do území okresů v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dotace, Individuální dotace, Programové dotace, Okres, yKrálovéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **5** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_a_programov%C3%A9_dotace_do_okres%C5%AF/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| poc_dotaci | Počet dotací | Double | 2526 |
| finance | Množství financí v Kč | Double | 882359612 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | IDOTOKR1 |

## Kina

> Datová sada s prostorovou lokalizací kin na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Kina, Kultura, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **51** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Kina/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | 3D kino Luník |
| provozovatel | Provozovatel | String | Městské kulturní středisko |
| ico | IČO | String | 00188557 |
| bezbarierovost | Bezbariérovost | String | ANO |
| pozn_bezbarierovost | Poznámka k bezbariérovosti | String | bezbariérové WC |
| typ_kina | Typ kina | String | kino se stálým provozem |
| pocet_salu | Počet sálů | Double | 1 |
| kapacita | Kapacita | Double | 220 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Náchod |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5209 |
| nazev_obce | Název obce | String | Červený Kostelec |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573965 |
| nazev_ulice | Název ulice | String | Manželů Burdychových |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 367 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 549 41     |
| www | Webové stránky | String | http://www.kinolunik.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.4726 16.0954) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.0954 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4726 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | KINA1 |

## Statistická data v grafech

> Statistické ukazatele o Královéhradeckém kraji setřídené do deseti tematických oblastí.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Hackathon 2025 partneři



_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Ambulantní gynekologové

> Datová sada s prostorovou lokalizací ambulantních gynekologů. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Ambulantní gynekologové, Zdravotnictví, Primární péče, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **83** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Ambulantn%C3%AD_gynekologov%C3%A9/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| provozovatel | Provozovatel | String | Ordinace ambulantního gynekologa  |
| ico | IČO | String | neuvedeno |
| pravni_forma | Právní forma | String | Společnost s ručením omezeným |
| nazev_oboru | Název oboru | String | gynekologie a porodnictví |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Rychnov nad Kněžnou |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0524 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Kostelec nad Orlicí  |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5208 |
| nazev_obce | Název obce | String | Týniště nad Orlicí |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 576859 |
| nazev_ulice | Název ulice | String | Mírové nám. |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 88 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 517 21 |
| www | Webové stránky | String | https://www.znamylekar.cz/zarizeni/1-orlicka-lekarska-s-r-o- |
| wkt | Zápis vektorové geometrie | String | POINT(50.1477933  16.0723158) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.0723158 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.1477933 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | AG1 |

## Studenti vysokých škol podle oborů

> Datová sada s prostorovou lokalizací studentů vysokých škol podle oborů za rok 2022. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

Klíčová slova: Studenti, Vysoké školy, Školství, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **300** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Studenti_vysok%C3%BDch_%C5%A1kol/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název vysoké školy | String | Česká zemědělská univerzita v Praze |
| rid | Rezortní identifikátor | String | 41110 |
| fakulta_pracoviste | Název fakulty nebo pracoviště | String | Provozně ekonomická fakulta |
| nazev_studijního_programu | Název studijního programu | String | Veřejná správa a regionální rozvoj |
| kod_sp | Kód studijního programu | String | B0312A050002 |
| nazev_studijniho_oboru | Název studijního oboru | String |  |
| kod_so | Kód studijního oboru | String |  |
| celkem_khk | Počet studentů v rámci Královéhradeckého kraje za rok 2022 | Double | 46 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové  |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností  | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové  |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Veverkova  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1343 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50002 |
| www | Webové stránky | String | https://www.pef.czu.cz/cs/r-17582-uchazeci/r-19592-centra-vz |
| wkt | Zápis vektorové geometrie | String | POINT(50.2064533  15.8148219) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8148219 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2064533 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | SVS1 |

## Vymezení rozvojových os v ZÚR

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Rozvojové osy, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **5** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Vymezen%C3%AD_rozvojov%C3%BDch_os_v_Z%C3%9AR/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Ident | Identifikátor osy | String | NOS1 |
| Nazev | Název osy | String | Rozvojová osa Hradec Králové-Jičín-Liberecký kraj |
| Vyznam | Význam osy | String | R |
| Jev | Jev B006 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů  | String | B006 |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String |   |
| Zdroj | Název nebo popis zdroje informace | String | ÚZ ZUR po změně 1,2,3,4,5 |
| Dat_Aktual | Datum aktualizace údaje | Date | 1683590400000 |
| Poznamka | Poznámka | String |   |

## Plochy pro sportovní létající zařízení

> Datová sada s prostorovou lokalizací ploch pro sportovní létající zařízení na území Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Létání, Sport, Plochy, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **6** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Plochy_pro_sportovn%C3%AD_l%C3%A9taj%C3%ADc%C3%AD_za%C5%99%C3%ADzen%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Plocha pro sportovní létající zařízení Horní Přím |
| kod | Kód | String | LKPRIM |
| druh | Přístup pro veřejnost | String | neveřejná |
| draha | Povrch dráhy | String | nezpevněná |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Dolní Přím |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569984 |
| nazev_ulice | Název ulice | String | Horní Přím |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 22 |
| typ_cisla_domovniho | Typ čísla domovního | String | cp |
| psc | PSČ | String | 50315 |
| www | Webové stránky | String |  |
| wkt | Zápis vektorové geometrie | String | POINT(50.2294 15.7036) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Single | 15.7036 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Single | 50.2294 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PPSLZ1 |

## Rozcestník aplikací

> Rozcestník webových a mapových aplikací zpracovaných Královéhradeckým krajem nebo jeho organizacemi.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Národní kulturní památky - pozemky

> Datová sada s prostorovou lokalizací pozemků národních kulturních památek na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

Klíčová slova: Národní kulturní památky, Pozemky, Památky, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **20** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/N%C3%A1rodn%C3%AD_kulturn%C3%AD_pam%C3%A1tky_pozemky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Muzeum v Hradci Králové |
| ico | IČO | String | 88382 |
| pravni_forma | Právní forma | String | Příspěvková organizace |
| uskp | Rejstříkové číslo v Ústředním seznamu kulturních památek | String | 208 |
| ucinnost | Datum účinnosti prohlášení za národní kulturní památku | String | 1. srpna 2002 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Eliščino nábřeží |
| cislo_domovní | Číslo popisné nebo evidenční podle typu čísla domovního | String | 465 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String | 7 |
| psc | PSČ | String | 50003 |
| www | Webové stránky | String | https://pamatkovykatalog.cz/pravni-ochrana/muzeum-v-hradci-k |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | NKPP13 |

## Botanické zahrady a arboreta

> Datová sada s prostorovou lokalizací botanických zahrad a arboret na území Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Botanika, Zahrady, Arboreta, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **2** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Botanick%C3%A9_zahrady_a_arboreta/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Botanická zahrada léčivých rostlin Farmaceutické fakulty Uni |
| ico | IČO | String | 00216208 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Akademika Heyrovskéh |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1203 |
| typ_cisla_domovniho | Typ čísla domovního | String | c.p. |
| psc | PSČ | String | 50003 |
| telefon | Telefon | String | 495067443 |
| email | Email | String | ZLRprohlidka@faf.cuni.cz |
| www | Webové stránky | String | https://www.faf.cuni.cz/Zahrada-lecivych-rostlin/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.203896 15.838800) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Single | 15.8388 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Single | 50.2039 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | BZA1 |

## Hackathon 2025 mentoři a porotci



_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Přírodní rezervace včetně ochranných pásem

> Datová sada s prostorovou lokalizací přírodních rezervací včetně ochranných pásem v Královéhradeckém kraji. Zdrojem dat je Odbor životního prostřední a zemědělství Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Přírodní rezervace, Ochranná pásma, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **61** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99%C3%ADrodn%C3%AD_rezervace_v%C4%8Detn%C4%9B_ochrann%C3%BDch_p%C3%A1sem/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| NAZEV | Název | String | Kovačská bažantnice |
| KOD | Evidenční číslo | Integer | 191 |
| KAT | Kategorie | String | PR |
| ROZL | Rozloha v hektarech | Double | 30.9463 |
| OP_TYP | Typ ochranného pásma | String | ZAK |
| IUCN | Kategorie IUCN | String | IV |
| PODKLAD | Podkladová mapa | String | KMW |
| ZPRACOVAL | Zpracovatel | String | KS HKA |
| VZNIK | Vznik | Integer | 20090713 |
| ZMENA_G | Změna grafická | Integer | 20090713 |
| ZMENA_T | Změna textová | Integer | 20170901 |
| POZNAMKA | Poznámka | String |   |
| KAT_OP | Kategorie, ochranné pásmo | String | PR_PR |

## ÚSES vymezený jako nezávazný koncepční podklad v plánech ÚSES a jiných oborových dokumentech v ZÚR

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Územní systém ekologické stability, Životní prostředí, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **2924** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Koncepce_%C3%9ASES/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Dokument | Název dokumentace | String | Revize ÚSES v KRNAP a jeho OP |
| Zprac_kdo | Název zpracovatele | String | Ageris s.r.o. |
| Zprac_kdy | Datum zpracování | Date | 1590883200000 |
| Typ_dok | Typ dokumentace | String | X |
| CasH | Časový horizont prvku | Integer | 0 |
| Typ | Typ prvku ÚSES | String | RBC |
| Vloz_BC | Vyznačení biocentra vloženého do biokoridoru vyšší úrovně | String |   |
| Oznaceni | Označení prvku ÚSES včetně případného názvu | String | RC1654 |
| Varianta | Označení případné varianty | String |   |
| Jev | Jev A119 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů | String | A119 |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String | 6850/UP/2021 |
| Zdroj | Název nebo popis zdroje informace | String | Správa Krkonošského národního parku |
| Dat_Aktual | Datum aktualizace údaje | Date | 1614211200000 |
| Poznamka | Poznámka | String | systémový typ: S |

## Absolventi vysokých škol podle oborů

> Datová sada s prostorovou lokalizací absolventů vysokých škol podle oborů za rok 2022. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

Klíčová slova: Vysoké školy, Absolventi, Školství, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **223** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Absolventi_vysok%C3%BDch_%C5%A1kol/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název vysoké školy | String | Česká zemědělská univerzita v Praze |
| rid | Rezortní identifikátor | String | 41110 |
| fakulta_pracoviste | Název fakulty nebo pracoviště | String | Provozně ekonomická fakulta |
| nazev_studijniho_programu | Název studijního programu | String | Hospodářská politika a správa |
| kod_sp | Kód studijního programu | String | B6202 |
| nazev_studijniho_oboru | Název studijního oboru | String |  |
| kod_so | Kód studijního oboru | String |  |
| celkem_khk | Počet absolventů v rámci Královéhradeckého kraje za rok 2022 | Double | 4 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové  |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové  |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Veverkova  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1343 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50002 |
| www | Webové stránky | String | https://www.pef.czu.cz/cs/r-17582-uchazeci/r-19592-centra-vz |
| wkt | Zápis vektorové geometrie | String | POINT(50.2064533  15.8148219) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8148219 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2064533 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | AVS1 |

## Hackathon 2023 mentoři a porotci 



_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Záplavové území stoleté vody

> Datová sada s prostorovou lokalizací záplavového území stoleté vody. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje. Data stažena zVodohospodářského informačního portálu VODA https://voda.gov.cz/ Průběh polygonů záplavových území v této vrstvě je pouze ORIENTAČNÍ, jedná se o kompilaci dat jednotlivých dílčích záplavových území, v rámci které byly provedeny úpravy (vyhlazení) geometrie v místě návaznosti jednotlivých částí. Pro závazné informace ohledně průběhu polygonů kontaktujte správce daného vodního toku nebo místě příslušný vodoprávní úřad.

Klíčová slova: Záplava, Povodeň, Stoletá voda, Q100, Životní prostředí, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **65** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1plavov%C3%A9_%C3%BAzem%C3%AD_stolet%C3%A9_vody/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_tok | Název vodního toku | String | Chaloupská svodnice |
| id_tok | Označení vodního toku | Double | 104470000100 |
| nazev_tok_vyssiho_radu | Název vodního toku, do kterého se daný vodní tok vlévá | String | Labský náhon |
| id_tok_vyssiho_radu | Označení vodního toku, do kterého se daný vodní tok vlévá | Double | 104450000100 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| dp_id | Jedinečný identifikátor va katalogu otevřených dat Data KHK | String | QSTO1 |

## Hackathon 2024 partneři



_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Zpráva a datové karty 2022 

> Výroční zprávy o stavu kraje v přehledných infografikách.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Lanovky

> Datová sada s prostorovou lokalizací lanovek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Lanovky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **20** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/L/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Kabinová lanovka Janské Lázně - Černohorský express (celoroč |
| popis | Popis | String | Osmimístná kabinová lanová dráha vedoucí na vrchol Černé hor |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Janské Lázně |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579351 |
| nazev_ulice | Název ulice | String | Černohorská  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 265 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54225 |
| www | Webové stránky | String | http://www.skiresort.cz |
| telefon | Telefon | String | +420840888229 |
| wkt | Zápis vektorové geometrie | String | POINT(50.6315 15.7706) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.7706 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.6315 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | LAN1 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 102 |

## Infocentra

> Datová sada s prostorovou lokalizací infocenter na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Infocentra, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **74** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Infocentra/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Městské informační centrum Jičín |
| popis | Popis | String | Jak se říká, „Všechny cesty vedou do Říma“, u nás v Jičíně ř |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Jičín |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 572659 |
| nazev_ulice | Název ulice | String | Valdštejnovo nám. |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50601 |
| www | Webové stránky | String | http://www.kzmj.cz |
| telefon | Telefon | String | +420493534390 |
| wkt | Zápis vektorové geometrie | String | POINT(50.4363 15.3522) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.3522 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4363 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | INFO1 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 1888 |

## Následná lůžková péče

> Datová sada s prostorovou lokalizací poskytovatelů následné péče v Královéhradeckém kraji. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Následná péče, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **12** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/N%C3%A1sledn%C3%A1_p%C3%A9%C4%8De/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Léčebna pro dlouhodobě nemocné Hradec Králové |
| ico | IČO | String | 180092 |
| pravni_forma | Právní forma | String | příspěvková organizace |
| zrizovatel | Zřizovatel | String | Královéhradecký kraj |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Pospíšilova |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 351 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String | 4 |
| psc | PSČ | String | 50003 |
| telefon | Telefon | String | +420495070211 |
| www | Webové stránky | String | https://ldnhk.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.2121 15.8399) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8399 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2121 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | NP1 |

## Dojížďka do vyšších odborných škol

> Datová sada s prostorovou lokalizací vyšších odborných škol v Královéhradeckém kraji, která obsahuje údaje týkající se dojížďky studentů do vyšších odborných škol z jejich míst s trvalým bydlištěm a další údaje. Data poskytl Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dojížďka, VOŠ, Školství, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **456** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Doj%C3%AD%C5%BE%C4%8Fka_do_vy%C5%A1%C5%A1%C3%ADch_odborn%C3%BDch_%C5%A1kol/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_skoly | Název školy | String | Česká lesnická akademie Trutnov - střední škola a vyšší odbo |
| izo | Identifikační znak organizace | String | 110029283 |
| red_izo | Resortní identifikátor právnické osoby | String | 600012905 |
| zrizovatel | Zřizovatel | String | kraj |
| typ_tridy | Typ třídy | String | běžná |
| obor | Označení oboru | String | 4132N01 |
| obor_nazev | Název oboru | String | Lesnictví |
| forma_vzdelavani | Forma vzdělávání | String | kombinovaná |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Kralovehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Trutnov |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579025 |
| nazev_ulice | Název ulice | String | Na Struze |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 8 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54101  |
| www | Webové stránky | String | https://www.clatrutnov.cz/ |
| nazev_okresu_bydliste_studenta | Název okresu bydliště studenta | String | Jeseník |
| kod_okresu_bydliste_studenta | Kód okresu bydliště studenta dle číselníku ČSÚ | String | CZ0711 |
| nazev_obce_bydliste_studenta | Název obce bydliště studenta | String | Javorník |
| kod_obce_bydliste_studenta | Kód obce bydliště studenta dle číselníku ČSÚ | String | 536148 |
| pocet_studentu | Počet studentů s trvalým bydlištěm v dané obci dojíždějící do vyšší odborné školy | Double | 1 |
| wkt | Zápis vektorové geometrie | String | POINT(50.558954 15.912539) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.912539 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.558954 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | SPVOS1 |

## Technické památky

> Datová sada s prostorovou lokalizací technických památek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Technické památky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **67** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Technick%C3%A9_pam%C3%A1tky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Hvězdárna Jičín |
| popis | Popis | String | Hvězdárna vznikla začátkem 60. letech 20. století v objektu  |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Jičín |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 572659 |
| nazev_ulice | Název ulice | String | Raisova  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 310 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50601 |
| www | Webové stránky | String | http://www.hvezdarnajicin.cz |
| telefon | Telefon | String | +420737203504 |
| wkt | Zápis vektorové geometrie | String | POINT(50.4344 15.3405) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.3405 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4344 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | TECHP1 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 4499 |

## Veřejné sbírky

> Datová sada s prostorovou lokalizací veřejných sbírek probíhajících v Královéhradeckém kraji. Zdrojem dat je Odbor majetkosprávní a krajský živnostenský úřad Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Veřejné sbírky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **86** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Ve%C5%99ejn%C3%A9_sb%C3%ADrky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Agentura splněných přání z.s. |
| ico | IČO | String | 8639396 |
| pravni_forma | Právní forma | String | Spolek |
| místo_konani | Místo konání | String | Česká republika |
| zahajeni | Datum zahájení | Date | 1574899200000 |
| ukonceni | Datum ukončení | String | na dobu neurčitou |
| ucel | Účel | String | pomoc dětem z dětských domovů, opuštěným seniorům, osobám s  |
| cislo_bankovniho_uctu | Číslo bankovního účtu | String | 5651586319/0800 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Lochenice  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 64 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50302 |
| www | Webové stránky | String | https://www.facebook.com/pomahejtesnami/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.2745 15.8224) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8224 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2745 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | VS1 |

## Nadregionální a regionální ÚSES vymezený v ZÚR

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Územní systém ekologické stability, Životní prostředí, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **526** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Nadregion%C3%A1ln%C3%AD_a_region%C3%A1ln%C3%AD_%C3%9ASES_vymezen%C3%BD_v_Z%C3%9AR_KHK/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Kraj | Název kraje | String | Královéhradecký kraj |
| Kraj_kod | Kód kraje dle RÚIAN | Integer | 86 |
| Plati_od | Datum nabytí účinnosti příslušné verze ZÚR | Date | 1618531200000 |
| RC_ZUR | Registrační číslo příslušné verze ZÚR v systému iKAS | Integer | 641 |
| CasH | Časový horizont prvku | Integer | 0 |
| Typ | Typ prvku ÚSES | String | NRBC |
| Vloz_BC | Vyznačení RBC vloženého do NRBK | String |   |
| Oznaceni | Označení prvku ÚSES včetně případného názvu | String | Prameny Úpy |
| Jev | Jev A021 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů  | String | A021 |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String |   |
| Zdroj | Název nebo popis zdroje informace | String | ÚZ ZÚR po vydání změn č. 1,2,3,4,5 |
| Dat_Aktual | Datum aktualizace údaje | Date | 1624233600000 |
| Poznamka | Poznámka | String | 85 |

## Záplavové území dvacetileté vody

> Datová sada s prostorovou lokalizací záplavového území dvacetileté vody. Zdrojem dat je Odbor životního prostředí a zemědělství Krajského úřadu Královéhradeckého kraje. Data stažena zVodohospodářského informačního portálu VODA https://voda.gov.cz/ Průběh polygonů záplavových území v této vrstvě je pouze ORIENTAČNÍ, jedná se o kompilaci dat jednotlivých dílčích záplavových území, v rámci které byly provedeny úpravy (vyhlazení) geometrie v místě návaznosti jednotlivých částí. Pro závazné informace ohledně průběhu polygonů kontaktujte správce daného vodního toku nebo místě příslušný vodoprávní úřad.

Klíčová slova: Záplava, Povodeň, Dvacetiletá voda, Q20, Životní prostředí, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **77** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1plavov%C3%A9_%C3%BAzem%C3%AD_dvacetilet%C3%A9_vody/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_tok | Název vodního toku | String | Chaloupská svodnice |
| id_tok | Označení vodního toku | Double | 104470000100 |
| nazev_tok_vyssiho_radu | Název vodního toku, do kterého se daný vodní tok vlévá | String | Labský náhon |
| id_tok_vyssiho_radu | Označení vodního toku, do kterého se daný vodní tok vlévá | Double | 104450000100 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| dp_id | Jedinečný identifikátor va katalogu otevřených dat Data KHK | String | QDVACET1 |

## Hackathon 2023 partneři



_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Vymezení specifických oblastí v ZÚR

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Specifické oblasti, Královéhradecký kraje

- Geometrie: esriGeometryPolygon | Záznamů: **5** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Vymezen%C3%AD_specifick%C3%BDch_oblast%C3%AD_v_Z%C3%9AR/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Ident | Identifikátor oblasti | String | NSO1 |
| Nazev | Název oblasti | String | Specifická oblast Broumovsko |
| Typ | Typ oblasti | String | P |
| Vyznam | Význam oblasti | String | R |
| Popis | Popis | String |   |
| Jev | Jev B006 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů  | String | B006 |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String |   |
| Zdroj | Název nebo popis zdroje informace | String | ÚZ ZUR po změně 1,2,3,4,5 |
| Dat_Aktual | Datum aktualizace údaje | Date | 1683590400000 |
| Poznamka | Poznámka | String |   |

## Zpráva a datové karty 2024

> Výroční zprávy o stavu kraje v přehledných infografikách.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Základní umělecké školy

> Datová sada s prostorovou lokalizací základních uměleckých škol na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: ZUŠ, Školy, Školství, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **32** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1kladn%C3%AD_um%C4%9Bleck%C3%A9_%C5%A1koly/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Biskupské gymnázium, církevní základní škola, mateřská škola |
| nazev_zrizovatele | Název zřizovatele | String | Biskupství královéhradecké |
| typ_zrizovatele | Typ zřizovatele | String | církev |
| ico | IČO | String | 71341072 |
| bezbarierovost | Bezbariérovost | String | částečně |
| pozn_bezbarierovost | Poznámka k bezbariérovosti | String | jedna budova není bezbariérová (zde i bezbariérové WC), druh |
| vyucovane_predmety | Vyučované předměty | String | hra na housle, hra na klavír, hra na kytaru, hra na violonce |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Orlické nábřeží |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 356 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String | 1 |
| psc | PSČ | String | 50003 |
| www | Webové stránky | String | www.czus.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.2062919 15.8337133) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8337133 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2062919 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ZUS1 |

## Zpráva a datové karty

> Výroční zprávy o stavu kraje v přehledných infografikách.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Letiště

> Datová sada s prostorovou lokalizací letišť na území Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Doprava, Letiště, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **9** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Leti%C5%A1t%C4%9B/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název  | String | Letiště Hradec Králové |
| kod | Kód  | String | LKHK |
| druh | Přístup pro veřejnost | String | vnitrostátní veřejné |
| draha | Povrch dráhy | String | zpevněná |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Letiště |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 98 |
| psc | PSČ | String | 50341 |
| www | Webové stránky | String | https://www.letistehradeckralove.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.2530 15.8453) |
| typ_cisla_domovniho | Typ čísla domovního | String | cp |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8453 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.253 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | LET4 |

## Architektonické památky

> Datová sada s prostorovou lokalizací architektonických památek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Architektonické památky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **74** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Architektonick%C3%A9_pam%C3%A1tky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Podorlický skanzen Krňovice |
| popis | Popis | String | Muzeum lidových staveb - skanzen doprovázený expozicemi hist |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Třebechovice pod Orebem |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 571041 |
| nazev_ulice | Název ulice | String | Krňovice  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 34 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50346 |
| www | Webové stránky | String | http://www.krnovice.cz |
| telefon | Telefon | String |   |
| wkt | Zápis vektorové geometrie | String | POINT(50.1903 15.9823) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.9823 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.1903 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ARCH24 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 100122 |

## Klastry Národní klastrové asociace

> Datová sada s prostorovou lokalizací klastrů v evidenci Národní klastrové asociace. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

Klíčová slova: Klastry, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **3** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Klastry_N%C3%A1rodn%C3%AD_klastrov%C3%A9_asociace/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Czech Stone Cluster, družstvo |
| ico | IČO | String | 27512410 |
| pravni_forma | Právní forma | String | Družstvo |
| vznik | Datum vzniku | String | 23.3.2007 |
| pocet_firem | Počet firem | Double | 16 |
| cz_nadace | Typ odvětví | String | Ostatní specializované stavební činnosti j. n. |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Lázně Bělohrad |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573094 |
| nazev_ulice | Název ulice | String | Horní Nová Ves  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 108 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50781 |
| www | Webové stránky | String | www.czechstonecluster.eu |
| wkt | Zápis vektorové geometrie | String | POINT(50.4523 15.5798) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.5798 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4523 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | KNCA1 |

## Hackathon 2022 partneři



_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Počet ubytovaných v domovech mládeže a internátech

> Datová sada zobrazující počet ubytovaných v domovech mládeže a internátech. Zdrojem dat je Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Domovy mládeže, Internáty, Školství, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **46** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Po%C4%8Det_ubytovan%C3%BDch_v_DDM_a_intern%C3%A1tech/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_organizace | Název organizace | String | Bezpečnostně právní akademie, s.r.o., střední škola |
| ico | IČO | String | 27121313 |
| identifikator_reditelstvi | Identifikátor ředitelství | String | 651014271 |
| zrizovatel | Zřizovatel | String | privátní sektor |
| typ | Typ zařízení | String | Domov mládeže |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Malé Svatoňovice |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579513 |
| nazev_ulice | Název ulice | String | 17. listopadu |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 177 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String |  542 34     |
| pocet_luzkove_kapacity | Počet lůžkové kapacity | Double | 155 |
| celkovy_pocet_pokoju | Celkový počet pokojů | Double | 39 |
| uhrnny_pocet_ubytovanych | Úhrnný počet ubytovaných studentů | Double | 120 |
| www | Webové stránky | String | http://www.bpa-svatonovice.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.5350761 16.0442047) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.0442047 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.5350761 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | DMI1 |

## Církevní památky

> Datová sada s prostorovou lokalizací církevních památek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Církevní památky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **162** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/C%C3%ADrkevn%C3%AD_pam%C3%A1tky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Kostel Nanebevzetí Panny Marie, Hradec Králové |
| popis | Popis | String | Barokní kostel navazující na jezuitskou kolej na Velkém námě |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Velké náměstí |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 32 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String | 40 |
| psc | PSČ | String | 50003 |
| www | Webové stránky | String | http://www.kostelpmhk.cz/ |
| telefon | Telefon | String | +420737446884 |
| wkt | Zápis vektorové geometrie | String | POINT(50.2092 15.8341) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8341 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2092 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | CIRKEV83 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 101974 |

## Schválené individuální dotace do kraje v letech 2021 - 2025

> Datová sada s prostorovou lokalizací schválených individuálních dotací Královéhradeckého kraje do území Královéhradeckého kraje v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dotace, Individuální dotace, Kraj, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **1** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_dotace_do_kraje/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| poc_dotaci | Počet dotací | Double | 2619 |
| finance | Množství financí v Kč | Double | 1864575428 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | IDOT1 |

## Zpráva a datové karty 2023

> Výroční zprávy o stavu kraje v přehledných infografikách.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Pivovary

> Datová sada s prostorovou lokalizací pivovarů na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

Klíčová slova: Pivovary, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **34** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pivovary/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Bakovský pivovar |
| region | Region | String | Studnice / Kladské pomezí |
| primarni_zatrideni | Primární zatřídění | String | Recepty a produkty / Pivovary |
| sekundarni_zatrideni | Sekundární zatřídění | String |  |
| tercialni_zatrideni | Terciální zatřídění | String |  |
| popis | Popis | String | Pivo ze zdejší produkce je možné ochutnat v sousední Hospodě |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Králohradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | 5209 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Náchod |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | CZ0523 |
| nazev_obce | Název obce | String | Studnice |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 574481 |
| nazev_ulice | Název ulice | String |  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 4 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54701 |
| www | Webové stránky | String | www.bakovskypivovar.cz |
| telefon | Telefon | String |  |
| mobil_1 | Mobil č. 1 | String | 420 603 421 490 |
| mobil_2 | Mobil č. 2 | String |  |
| fax | Fax | String |  |
| email_1 | Email č. 1 | String | info@bakovskypivovar.cz |
| email_2 | Email č. 2 | String |  |
| wkt | Zápis vektorové geometrie | String | POINT(50.43479530 16.08620640) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.0862064 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4347953 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PIV1 |

## Územní rezervy dle ZÚR

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Územní rezervy, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **9** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/%C3%9Azemn%C3%AD_rezervy_dle_Z%C3%9AR/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Kraj | Název kraje | String | Královéhradecký kraj |
| Kraj_kod | Kód kraje dle RÚIAN | Integer | 86 |
| Plati_od | Datum nabytí účinnosti příslušné verze ZÚR | Date | 1618531200000 |
| RC_ZUR | Registrační číslo příslušné verze ZÚR v systému iKAS | Integer | 641 |
| Ident | Identifikátor územní rezervy v daných ZÚR | String | DZ1r |
| Typ_skup | Základní skupina typu budoucího využití dle vyhlášky č. 501/2006 Sb. | String | D |
| Popis | Popis záměru | String | Modernizace,elektrizace trati Jaroměř-Náchod |
| Jev | Jev A118 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů  | String | A118 |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String |   |
| Zdroj | Název nebo popis zdroje informace | String | ÚZ ZÚR po vydání změn č. 1,2,3,4,5 |
| Dat_Aktual | Datum aktualizace údaje | Date | 1683590400000 |
| Poznamka | Poznámka | String |   |

## Praktičtí lékaři pro děti a dorost

> Datová sada s prostorovou lokalizací praktických lékařů pro děti a dorost. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Praktičtí lékaři, Děti a dorost, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **123** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Prakti%C4%8Dt%C3%AD_l%C3%A9ka%C5%99i_pro_d%C4%9Bti_a_dorost/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| provozovatel | Provozovatel | String | Ordinace lékaře pro děti a dorost |
| ico | IČO | String | neuvedeno |
| pravni_forma | Právní forma | String | společnost s ručením omezeným  |
| nazev_oboru | Název oboru | String | praktické lékařství pro děti a dorost |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Bratří Štefanů |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 895 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String | 1 |
| psc | PSČ | String | 50003 |
| www | Webové stránky | String |  |
| wkt | Zápis vektorové geometrie | String | POINT(50.2166475  15.8540636) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8540636 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2166475 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PLDD1 |

## Řemeslné výrobky

> Datová sada s prostorovou lokalizací sídel řemeslných výrobků na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

Klíčová slova: Řemeslné výrobky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **64** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/%C5%98emesln%C3%A9_v%C3%BDrobky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Amálčina lázeň s epsomskou solí |
| region | Region | String | Náchod / Kladské pomezí |
| primarni_zatrideni | Primární zatřídění | String | Recepty a produkty / Řemeslné výrobky |
| sekundarni_zatrideni | Sekundární zatřídění | String |  |
| tercialni_zatrideni | Terciální zatřídění | String |  |
| popis | Popis | String |  
  |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Náchod |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5209 |
| nazev_obce | Název obce | String | Náchod |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573868 |
| nazev_ulice | Název ulice | String | Kostelecká |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1829 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54701 |
| telefon_1 | Telefon č. 1 | String |  |
| telefon_2 | Telefon č. 2 | String |  |
| mobil_1 | Mobil č. 1 | String | 420 777 591 139 |
| mobil_2 | Mobil č. 2 | Double |  |
| mobil_3 | Mobil č. 3 | String |  |
| fax | Fax | String |  |
| email_1 | Email č. 1 | String | info@mydla-amalka.cz |
| email_2 | Email č. 2 | String |  |
| www | Webové stránky | String | www.mydla-amalka.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.42456139 16.16584417) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.16584417 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.42456139 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | RV1 |

## Železniční tratě VDKHK

> Datová sada s prostorovou lokalizací vlakových linek v rámci Veřejné dopravy Královéhradeckého kraje (VDKHK). Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Linky, Vlak, Železnice, VDKHK, Královéhradecký kraj

- Geometrie: esriGeometryPolyline | Záznamů: **19** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Vlakov%C3%A9_linky_IREDO/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| ozn_linka | Označení linky | String | 020 |
| trasa | Popis trasy | String | Velký Osek, žel. st. - Chlumec nad Cidlinou, žel. st. - Hrad |
| jizd_rad | Webový odkaz na jízdní řád Veřejné dopravy Královéhradeckého kraje | String | https://www.dopravakhk.cz/vyhledat-jizdni-rad.html?query=020 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj, Středočeský kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052, CZ020 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | VL1 |

## Autobusové zastávky VDKHK

> Datová sada s prostorovou lokalizací autobusových zastávek v rámci Veřejné dopravy Královéhradeckého kraje (VDKHK). Nejsou zde obsaženy zastávky MHD. Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Autobus, Zastávky, VDKHK, Doprava, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **4538** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Autobusov%C3%A9_zast%C3%A1vky_IREDO/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Adršpach,Dolní Adršpach,odb.Zdoňov |
| oznaceni | Označení | String | 39 |
| stanoviste | Stanoviště | Double | 1 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Broumov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5201 |
| nazev_obce | Název obce | String | Adršpach |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 547786 |
| wkt | Zápis vektorové geometrie | String | POINT(50.616431 16.132747) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.132747 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.616431 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | AZ1 |

## Zámky

> Datová sada s prostorovou lokalizací zámků na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Zámky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **33** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1mky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Zámek Dětenice |
| popis | Popis | String | Zámek Dětenice se nachází asi 70 km severovýchodně od Prahy, |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Dětenice |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 572829 |
| nazev_ulice | Název ulice | String |  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 1 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50724 |
| www | Webové stránky | String | http://www.detenice.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.368 15.1729) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.1729 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.368 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ZAMKY2 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 1248 |

## Počet obydlených domů dle připojení na kanalizaci v obcích

> Datová sada s prostorovou lokalizací počtu obydlených domů
dle připojení na kanalizaci v obcích na území Královéhradeckého kraje.
Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu
Královéhradeckého kraje.

Klíčová slova: Kanalizace, Domy, Počet, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **448** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Po%C4%8Det_obydlen%C3%BDch_dom%C5%AF_dle_p%C5%99ipojen%C3%AD_na_kanalizaci_v_obc%C3%ADch/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_obce | Název obce | String | Červená Hora |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 505099 |
| kanal_sit | Kanalizační síť | Double | 5 |
| cov | Vlastní ČOV | Double | 7 |
| zumpa | Žumpa | Double | 38 |
| bez_a_nezj | Bez odkanalizování a nezjištěno | Double | 4 |
| celkem | Celkem | Double | 54 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | KANPOC53 |

## Ostatní letní sporty

> Datová sada s prostorovou lokalizací areálů určených pro ostatní letní sporty na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

Klíčová slova: Ostatní letní sporty, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **61** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Ostatn%C3%AD_letn%C3%AD_sporty/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Aerobic - Sportovní areál Jičín  |
| region | Region | String | Jičín / Český ráj |
| primarni_zatrideni | Primární zatřídění | String | Sport a relaxace / Ostatní letní sporty |
| sekundarni_zatrideni | Sekundární zatřídění | String |  |
| tercialni_zatrideni | Terciální zatřídění | String |  |
| popis | Popis | String | Cvičí se zde:- aerobic- bodystyling- STEP aerobik- dopolední |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Jičín |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 572659 |
| nazev_ulice | Název ulice | String | Revoluční |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 863 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50601 |
| www | Webové stránky | String | www.sport-jicin.cz/vnitrni/aerobic.php
http://www.sport-jici |
| telefon_1 | Telefon č. 1 | String | 420 493 531 860 |
| telefon_2 | Telefon č. 2 | Double | 420493533462 |
| mobil_1 | Mobil č. 1 | String | 420 604 239 008 |
| mobil_2 | Mobil č. 2 | Double |  |
| mobil_3 | Mobil č. 3 | Double |  |
| fax | Fax | String |  |
| email_1 | Email č. 1 | String | posta@sport-jicin.cz |
| email_2 | Email č. 2 | String |  |
| email_3 | Email č. 3 | String |  |
| wkt | Zápis vektorové geometrie | String | POINT(50.44066694 15.36333028) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.36333028 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.44066694 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | OLS1 |

## Mapa stránek

> Mapa stránek datového portálu Královéhradeckého kraje Data KHK.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Zdroje dat na portálu

> Podívejte se, odkud odkud čerpáme data pro datový portál Data KHK.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Podíl obydlených domů dle připojení na kanalizaci

> Datová sada s prostorovou lokalizací podílu obydlených domů dle připojení na kanalizaci v obcích na území Královéhradeckého kraje. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Kanalizace, Domy, Podíl, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **448** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pod%C3%ADl_obydlen%C3%BDch_dom%C5%AF_dle_p%C5%99ipojen%C3%AD_na_kanalizaci/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_obce | Název obce | String | Červená Hora |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 505099 |
| kanal_sit | Kanalizační síť | Double | 0.0925925925926 |
| cov | Vlastní ČOV | Double | 0.12962962963 |
| zumpa | Žumpa | Double | 0.703703703704 |
| bez_a_nezj | Bez odkanalizování a nezjištěno | Double | 0.0740740740741 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | KANPR53 |

## Hackathon 2024 mentoři a porotci



_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Zubní lékaři

> Datová sada s prostorovou lokalizací zubních lékařů. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Zubní lékaři, Zdravotnictví, Primární péče, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **299** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Zubn%C3%AD_l%C3%A9ka%C5%99i/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| provozovatel | Provozovatel | String | Ordinace zubního lékaře |
| ico | IČO | String | neuvedeno |
| pravni_forma | Právní forma | String | Společnost s ručením omezeným |
| nazev_oboru | Název oboru | String | zubní lékařství |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Vrchlabí |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5215 |
| nazev_obce | Název obce | String | Lánov |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579432 |
| nazev_ulice | Název ulice | String | Prostřední Lánov  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 129 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 543 41 |
| www | Webové stránky | String | https://www.nzip.cz/poskytovatel/stomatologie/08481288000000 |
| wkt | Zápis vektorové geometrie | String | POINT(50.6196200  15.6570742) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.6570742 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.61962 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ZL1 |

## Dojížďka do středních škol

> Datová sada s prostorovou lokalizací středních škol v Královéhradeckém kraji, která obsahuje údaje týkající se dojížďky studentů do středních škol z jejich míst s trvalým bydlištěm a další údaje. Data poskytl Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dojíˇžka, Střední školy, Školství, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **12013** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Doj%C3%AD%C5%BE%C4%8Fka_do_st%C5%99edn%C3%ADch_%C5%A1kol/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_skoly | Název školy | String | ACADEMIA MERCURII soukromá střední škola,  s.r.o. |
| izo | Identifikační znak organizace | String | 108024148 |
| red_izo | Resortní identifikátor právnické osoby | String | 600012271 |
| zrizovatel | Zřizovatel | String | privátní sektor |
| typ_tridy | Typ třídy | String | běžná |
| obor | Obor | String | 6542M02 |
| obor_nazev | Název oboru | String | Stravovací a ubytovací služby |
| forma_vzdelavani | Forma vzdělávání | String | denní |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Náchod |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5209 |
| nazev_obce | Název obce | String | Náchod |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573868 |
| nazev_ulice | Název ulice | String | Smiřických |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 740 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50901 |
| www | Webové stránky | String | https://www.amnachod.cz/index.php/cs/ |
| nazev_okresu_bydliste_studenta | Název okresu bydliště studenta | String | Náchod |
| kod_okresu_bydliste_studenta | Kód okresu bydliště studenta dle číselníku ČSÚ | String | CZ0523 |
| nazev_obce_bydliste_studenta | Název obce bydliště studenta | String | Velké Poříčí |
| kod_obce_bydliste_studenta | Kód obce bydliště studenta dle číselníku ČSÚ | String | 547646 |
| pocet_zaku | Počet studentů s trvalým bydlištěm v dané obci dojíždějící do dané střední školy | Double | 1 |
| wkt | Zápis vektorové geometrie | String | POINT(50.4209792  16.1647297) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.1647297 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4209792 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | SPSS1 |

## Mapy z oblasti kultury

> Na stránce naleznete mapové výstupy z analytické části Strategie rozvoje kultury, kulturního dědictví a kulturních a kreativních odvětví pro Královéhradecký kraj 2022–⁠2030.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Počet žáků v oborech středních škol zřizovaných krajem

> Datová sada s prostorovou lokalizací středních škol zřizovaných Královéhradeckým krajem včetně přehledu počtu žáků v jednotlivých školách. Data jsou za školní rok 2025/2026. Zdrojem dat je Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: SŠ, Žáci, Školství, Obory, Počet žáků, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **299** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Po%C4%8Dty_%C5%BE%C3%A1k%C5%AF_v_oborech_st%C5%99edn%C3%ADch_%C5%A1kol_z%C5%99izovan%C3%BDch_krajem/FeatureServer/4

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | nazev | String | Česká lesnická akademie Trutnov - střední škola a vyšší odbo |
| redizo | redizo | String | 600012905 |
| izo | izo | String | 060153296 |
| izo_sidlo | izo_sidlo | String | 000195057 |
| zrizovatel | zrizovatel | String | kraj |
| druh_zarizeni | druh_zarizeni | String | Střední škola                            |
| nazev_vusc | nazev_vusc | String | Královéhradecký kraj |
| kod_vusc | kod_vusc | String | CZ052 |
| nazev_okresu | nazev_okresu | String | Trutnov |
| kod_okresu | kod_okresu | String | CZ0525 |
| nazev_orp | nazev_orp | String | Trutnov |
| kod_orp | kod_orp | String | 5214 |
| nazev_obce | nazev_obce | String | Svoboda nad Úpou |
| kod_obce | kod_obce | String | 579734 |
| nazev_ulice | nazev_ulice | String | Horská |
| cislo_domovni | cislo_domovni | String | 134 |
| typ_cisla_domovniho | typ_cisla_domovniho | String | cp |
| cislo_orientacni | cislo_orientacni | String |  |
| kategorie | kategorie | String | Obory s výučním listem - H |
| obor_kod | obor_kod | String | 4156H01 |
| obor_nazev | obor_nazev | String | Lesní mechanizátor |
| skolni_rok | skolni_rok | String | 2025/2026 |
| pocet_zaku_celkem | pocet_zaku_celkem | Double | 110 |
| pocet_absolventu | pocet_absolventu | Double | 37 |
| pocet_prijatych | pocet_prijatych | Double | 30 |
| wkt | wkt | String | POINT(50.5410736 15.7257289) |
| x | x | Double | 15.7257289 |
| y | y | Double | 50.5410736 |
| id | id | String | ZOSS1 |

## Kulturní domy a kongresová centra

> Datová sada s prostorovou lokalizací kulturních domů a kongresových center na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Kulturní domy, Kongresová centra, Kultura, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **56** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Kulturn%C3%AD_domy/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Adalbertinum |
| typ_od | Typ datové sady | String | Kulturní domy a kongresová centra |
| typ_zarizeni | Typ zařízení | String | Kongresové centrum |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Československé armády |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 300 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |   |
| psc | PSČ | String | 50003 |
| ico | IČO | String |   |
| ico_subjektu_zrizovatele_nebo_provozovatele | IČO subjektu zřizovatele nebo provozovatele | String | 27472809 |
| zrizovatel_provozovatel | Zřizovatel nebo provozovatel | String | Hradecká kulturní a vzdělávací společnost, s.r.o. |
| ico_zrizovatele_provozovatele | IČO zřizovatele nebo provozovatele | String | 27472809 |
| nazev_vusc_zrizovatele_provozovatele | Název vyššího územního samosprávného celku zřizovatele nebo provozovatele | String | Královéhradecký kraj |
| kod_vusc_zrizovatele_provozovatele | Kód vyššího územního samosprávného celku dle číselníku ČSÚ zřizovatele nebo provozovatele | String | CZ052 |
| nazev_okresu_zrizovatele_provozovatele | Název okresu zřizovatele nebo provozovatele | String | Hradec Králové |
| kod_okresu_zrizovatele_provozovatele | Kód okresu dle číselníku ČSÚ zřizovatele nebo provozovatele | String | CZ0521 |
| nazev_orp_zrizovatele_provozovatele | Název správního obvodu obce s rozšířenou působností zřizovatele nebo provozovatele | String | Hradec Králové |
| kod_orp_zrizovatele_provozovatele | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce__zrizovatele_provozovatele | Název obce zřizovatele nebo provozovatele | String | Hradec Králové |
| kod_obce_zrizovatele_provozovatele | Kód obce dle číselníku ČSÚ zřizovatele nebo provozovatele | String | 569810 |
| nazev_ulice_zrizovatele_provozovatele | Název ulice zřizovatele nebo provozovatele | String | Československé armády |
| cislo_domovni_zrizovatele_provozovatele | Číslo popisné nebo evidenční podle typu čísla domovního zřizovatele nebo provozovatele | String | 300 |
| typ_cisla_domovniho_zrizovatele_provozovatele | Typ čísla domovního zřizovatele nebo provozovatele | String |   |
| cislo_orientacni_zrizovatele_provozovatele | Číslo orientační zřizovatele nebo provozovatele | String | 22 |
| psc_zrizovatele_prozovatele | PSČ zřizovatele nebo provozovatele | String | 50003 |
| www | Webové stránky | String | https://www.adalbertinum.cz/ |
| bezbarierovost | Bezbariérovost | String | ANO |
| bezbarierovost_poznamka | Poznámka k bezbariérovosti | String | Chybí bezbariérové WC. Pří větší skupině je lepší zavolat př |
| vyucovane_predmety_realizovane_aktivity | Vyučované předměty nebo realizované aktivity | String | kulturní a společenské akce, kongresy, konference, školení,  |
| technicka_vybavenost | Technická vybavenost | String | moderní audiovizuální technika a osvětlení |
| kapacita | Kapacita | Double | 730 |
| kapacita_poznamka | Poznámka ke kapacitě | String | Velký sál: 440 (divadlo), 240 (konference)
Malý sál: 100 (di |
| wkt | Zápis vektorové geometrie | String | POINT(50.2112056 15.8311492) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8311492 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2112056 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | KDKC1 |

## Muzea a galerie

> Datová sada s prostorovou lokalizací muzeí a galerií na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Muzea, Galerie, Kultura, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **160** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Muzea_a_galerie/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Archeologické muzeum v přírodě Villa Nova Uhřínov |
| ico | IČO | String | 60884037 |
| zrizovatel | Zřizovatel | String | Villa Nova Uhřínov - středisko experimentální archeologie a  |
| bezbarierovost | Bezbariérovost | String | částečně |
| pozn_bezbarierovost | Poznámka k bezbariérovosti | String | bezbariérové WC ne |
| typ_muzea | Typ muzea | String | archeologické |
| zamereni_muzea | Zaměření muzea | String | expozice se snaží přiblížit středověkou kolonizační vesnici |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Rychnov nad Kněžnou |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0524 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Rychnov nad Kněžnou |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5213 |
| nazev_obce | Název obce | String | Liberk |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 576450 |
| nazev_ulice | Název ulice | String | Uhřínov |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 15 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 51601 |
| www | Webové stránky | String | https://www.villanova.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.2509742 16.3503822) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.3503822 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2509742 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | MG1 |
| is_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 4080 |

## Počet studentů v oborech vyšších odborných škol zřizovaných krajem

> Datová sada s prostorovou lokalizací vyšších odborných škol zřizovaných Královéhradeckým krajem včetně přehledu počtu studentů v jednotlivých školách. Data jsou za školní rok 2024/2025. Zdrojem dat je Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: VOŠ, Obory, Počet studentů, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **14** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Po%C4%8Dty_student%C5%AF_v_oborech_vy%C5%A1%C5%A1%C3%ADch_odborn%C3%BDch_%C5%A1kol_z%C5%99izovan%C3%BDch_krajem/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název školy | String | Česká lesnická akademie Trutnov - střední škola a vyšší odbo |
| ico | IČO | String | 60153296 |
| pravni_forma | Právní forma | String | příspěvková organizace |
| redizo | Resortní identifikátor právnické osoby | String | 600012905 |
| izo | Identifikační znak organizace | String |     110029283 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Trutnov |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579025 |
| nazev_ulice | Název ulice | String | Lesnická |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 9 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |   |
| psc | PSČ | String | 54101 |
| nazev_obce_pracoviste | Název obce pracoviště školy | String | Trutnov |
| kod_obce_pracoviste | Kód obce pracoviště školy dle číselníku ČSÚ | String | 579025 |
| nazev_ulice_pracoviste | Název ulice pracoviště školy | String | Na Struze |
| cislo_domovni_pracoviste | Číslo popisné nebo evidenční podle typu čísla domovního pracoviště školy | String | 8 |
| typ_cisla_domovniho_pracoviste | Typ čísla domovního pracoviště školy | String | č.p. |
| cislo_orientacni_pracoviste | Číslo orientační pracoviště školy | String |   |
| psc_pracoviste | PSČ pracoviště školy | String | 54101 |
| www | Webové stránky | String | http://www.clatrutnov.cz/ |
| vzdelavaci_program_nazev | Název vzdělávacího programu | String | Lesnictví |
| vzdelavaci_program_kod | Kód vzdělávacího programu | String | 4132N01 |
| vzdelani_delka_roky | Délka vzdělávání v letech | String | 3 |
| vzdelani_forma | Forma vzdělávání | String | kombinovaná forma vzdělávání |
| pocet_studentu_1_rocnik | Počet studentů v 1. ročníku ve školním roce 2024/2025 | Double | 23 |
| pocet_studentu_2_rocnik | Počet studentů v 2. ročníku ve školním roce 2024/2025 | Double | 10 |
| pocet_studentu_3_rocnik | Počet studentů v 3. ročníku ve školním roce 2024/2025 | Double | 7 |
| pocet_studentu_4_rocnik | Počet studentů v 4. ročníku ve školním roce 2024/2025 | Double | 0 |
| pocet_studentu_celkem | Celkový počet studentů ve školním roce 2024/2025 | Double | 40 |
| pocet_absolventu_2023_2024 | Počet absolventů ve školním roce 2023/2024 | Double | 3 |
| pocet_prijatych_studentu | Počet přijatých studentů pro školním ročník 2024/2025 | Double | 21 |
| wkt | Zápis vektorové geometrie | String | POINT(50.5585669 15.9119539) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.9119539 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.5585669 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | SKVOS1 |

## Přijímací řízení na střední školy

> Datová sada s prostorovou lokalizací středních škol v Královéhradeckém kraji, která obsahuje údaje týkající se přijímacího řízení a další údaje. Data poskytl Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Přijímací řízení, Střední školy, Školství, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **352** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99ij%C3%ADmac%C3%AD_%C5%99%C3%ADzen%C3%AD_na_st%C5%99edn%C3%AD_%C5%A1koly/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Academia Mercurii soukromá střední škola, s.r.o. |
| ico | IČO | String | 62028561 |
| zrizovatel | Zřizovatel | String | privátní sektor |
| pravni_forma | Právní forma | String | společnost s ručením omezeným |
| redizo | Resortní identifikátor právnické osoby | String | 600012271 |
| izo | Identifikační znak organizace | String | 108024148 |
| obor_nazev | Název studijního oboru | String | Cestovní ruch |
| obor_kod | Kód studijního oboru | String | 65-42-M/02 |
| nazev_svp__zamereni_oboru | Název zaměření oboru | String |  |
| vzdelavani_delka_roky | Délka vzdělávání v letech | String | 4 |
| zpusob_ukonceni_vzdelavani | Způsob ukončení vzdělávání | String | maturitní zkouška |
| pocet_prihl_1_kolo_prij_riz | Počet přihlášených do 1. kola přijímacího řízení pro školní rok 2023/2024 | Double | 128 |
| pocet_prijatych_po_1_kole_prij | Počet přijatých po 1. kole přijímacího řízení pro školní rok 2023/2024 | Double | 30 |
| pocet_nabizenych_mist | Počet nabízených míst pro školní rok 2024/2025 | Double | 30 |
| rocni_skolne | Roční školné | Double | 19200 |
| prijimaci_zkouska | Přijímací zkouška | String | ano |
| forma_vzdelavani | Forma vzdělávání | String | Denní forma |
| druh_vzdelavani | Druh vzdělávání | String | střední vzdělávání s maturitní zkouškou |
| obor_urcen_pro_uchaz_s_uk_vzd | Obor určen pro uchazeče s ukončeným vzděláním | String | NE |
| obor_vhodny_pro_ozp | Obor vhodný pro osoby se zdravotním postižením | String | s ukončenou povinnou škol. docházkou |
| povinna_lekarska_prohlidka | Povinná lékařská prohlídka | String | NE |
| poznamka | Poznámka | String |  |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Náchod |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5209 |
| nazev_obce | Název obce | String | Náchod |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573868 |
| nazev_ulice | Název ulice | String | Smiřických |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 740 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54701 |
| www | Webové stránky | String | www.amnachod.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.420981 16.164733) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.164733 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.420981 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PRSS1 |

## Památkové rezervace

> Datová sada s prostorovou lokalizací památkových rezervací na území Královéhradeckého kraje. Jsou zde obsaženy městské a vesnické památkové zóny. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

Klíčová slova: Památkové rezervace, Památky, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **7** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pam%C3%A1tkov%C3%A9_rezervace/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Vesnická památková rezervace Vesec u Sobotky |
| vyhlasovatel | Vyhlašovatel | String | Vláda České republiky |
| dokument | Dokument | String | Nařízení Vlády České republiky č. 127/1995 Sb. |
| uskp | Rejstříkové číslo v Ústředním seznamu kulturních památek | String | 1099 |
| prohlaseni | Datum prohlášení za památkovou rezervaci | String | 24. května 1995 |
| ucinnost | Datum účinnosti prohlášení za památkovou rezervaci | String | 25. července 1995 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Libošovice |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573116 |
| psc | PSČ | String | 50601 |
| www | Webové stránky | String | https://pamatkovykatalog.cz/vesec-7663870 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PR7 |

## Národní kulturní památky - bodové objekty

> Datová sada s prostorovou lokalizací bodových objektů (sochy apod.), které jsou součástí národních kulturních památek na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

Klíčová slova: Národní kulturní památky, Bodové objekty, Památky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **24** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/N%C3%A1rodn%C3%AD_kulturn%C3%AD_pam%C3%A1tky_bodov%C3%A9_objekty/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Babiččino údolí |
| objekt | Předmět ochrany památkové péče | String | socha P. Marie |
| ico | IČO | String | 28779274 |
| pravni_forma | Právní forma | String | Společnost s ručením omezeným |
| uskp | Rejstříkové číslo v Ústředním seznamu kulturních památek | String | 11782/6-1584 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Náchod |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5209 |
| nazev_obce | Název obce | String | Česká Skalice |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573990 |
| nazev_ulice | Název ulice | String | Ratibořice |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 2 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 552 03 |
| www | Webové stránky | String | https://pamatkovykatalog.cz/zamek-ratiborice-s-arealem-babic |
| wkt | Zápis vektorové geometrie | String | POINT(50.4216 16.0558) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.0558 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4216 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | NKPB1 |

## Rozhledny a vyhlídky

> Datová sada s prostorovou lokalizací rozhleden a vyhlídek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Rozhledny a vyhlídky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **49** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Rozhledny_a_vyhl%C3%ADdky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Hora Sněžka |
| popis | Popis | String | Nejvyšší hora v České republice, která má tvar trojbokého je |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Pec pod Sněžkou |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579581 |
| nazev_ulice | Název ulice | String |  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String |  |
| typ_cisla_domovniho | Typ čísla domovního | String |  |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54221 |
| www | Webové stránky | String | http://www.horasnezka.cz |
| telefon | Telefon | String | +420499456111 |
| wkt | Zápis vektorové geometrie | String | POINT(50.736 15.7397) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.7397 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.736 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ROZHL1 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 569 |

## Hackathon 2026 mentoři a porotci



_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Památkové zóny

> Datová sada s prostorovou lokalizací památkových zón na území Královéhradeckého kraje. Jsou zde obsaženy městské, vesnické i krajinné památkové zóny. 
Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje a Památkový katalog Národního památkového ústavu.

Klíčová slova: Památkové zóny, Památky, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **35** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pam%C3%A1tkov%C3%A9_z%C3%B3ny/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Městská památková zóna Sobotka |
| vyhlasovatel | Vyhlašovatel | String | Východočeský KNV Hradec Králové |
| dokument | Dokument | String | Vyhláška Vč KNV Hradec Králové ze dne 17.10.1990 |
| uskp | Rejstříkové číslo v Ústředním seznamu kulturních památek | String | 2011 |
| prohlaseni | Datum prohlášení za památkovou zónu | String | 17. října 1990 |
| ucinnost | Datum účinnosti prohlášení za památkovou zónu | String | 1. listopadu 1990 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Sobotka |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573493 |
| psc | PSČ | String | 50743 |
| www | Webové stránky | String | https://pamatkovykatalog.cz/sobotka-7664826 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PZ16 |

## Domy dětí a mládeže a střediska volného času

> Datová sada s prostorovou lokalizací domů dětí a mládeže a středisek volného času na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: DDM, Volný čas, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **27** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Domovy_d%C4%9Bt%C3%AD_a_ml%C3%A1de%C5%BEe_a_st%C5%99ediska_voln%C3%A9ho_%C4%8Dasu/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Dům dětí a mládeže Dobruška, Domašínská 363 |
| ico | IČO | String | 64224635 |
| kratky_nazev | Krátký název | String |  |
| popis | Popis pořádaných akcí | String | Kroužky pro děti a mládež, tábory, akce pro veřejnost. |
| zrizovatel | Zřizovatel | String | Město Dobruška |
| typ_zrizovatele | Typ zřizovatele | String | Město Dobruška |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Rychnov nad Kněžnou |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0524 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Dobruška |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5202 |
| nazev_obce | Název obce | String | Dobruška |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 576271 |
| nazev_ulice | Název ulice | String | Domašínská |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 363 |
| typ_cisla_domovniho | Typ čísla domovního | String | cp |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 51801 |
| www | Webové stránky | String | ddm.mestodobruska.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.2928358 16.1635708) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.1635708 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2928358 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | DDMSVC1 |

## Investiční pobídky

> Datová sada s prostorovou lokalizací investičních pobídek na území Královéhradeckého kraje. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest.

Klíčová slova: Investiční pobídky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **84** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Investi%C4%8Dn%C3%AD_pob%C3%ADdky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_spolecnosti | Název společnosti | String | ABB s.r.o. |
| cislo_pobidky | Číslo pobídky | String | 737 |
| ico | IČO | String | 49682563 |
| pravni_forma | Právní forma | String | společnost s ručením omezeným |
| cz_nace | Klasifikace ekonomických činností | String | 271100 |
| sektor | Sektor | String | elektronický a elektrotechnický |
| druh_investicni_akce | Druh investiční akce | String | technologické centrum |
| země_puvodu_skupiny | Země původu skupiny | String | neuvedeno |
| země_puvodu_zadatele | Země původu žadatele | String | Česká republika |
| investice_mil__EUR | Hodnota investice v milionech eur | Double | 0.39643211100099107 |
| investice_mil__USD | Hodnota investice v milionech amerických dolarů | Double | 0.5256794406770752 |
| investice_mil__CZK | Hodnota investice v milionech korun českých | Double | 10 |
| nova_pm | Nová pobídka | String | 40 |
| dane | Daně | String | ano |
| rekvalifikace | Investiční pobídka ve formě hmotné podpory na školení a rekvalifikaci zaměstnanců | String | neuvedeno |
| mira_verejne_podpory | Míra veřejné podpory | Double | 0.4 |
| strop_verejne_podpory_mil__CZK | Maximální možná výše podpory pro podpořený projekt v milionech korun českých | Double | 14.77 |
| rok_podani_zameru | Rok podání záměru | String | 2013 |
| rok_rozhodnuti | Rok vydání rozhodnutí Ministerstva průmyslu a obchodu o udělení investiční pobídky | String | 2013 |
| mpsv | Ministerstvo práce a sociálních věcí | String | ne |
| rozhodnuti_zruseni_odstoupeni | Rozhodnutí o zrušení nebo odstoupení investiční pobídky | String | ano |
| investor_investuje_bez_pobidek | Investor investuje bez pobídek | String | neuvedeno |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Chelčického  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 201 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50002 |
| www | Webové stránky | String | https://new.abb.com/cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.2145661 15.8165589) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8165589 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2145661 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | IP1 |

## Jezdectví

> Datová sada s prostorovou lokalizací areálů určených pro jezdectví na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

Klíčová slova: Jezdectví, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **89** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Jezdectv%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Agrofarma Bolkovský dvůr v Krkonoších |
| region | Region | String | Rudník / Krkonoše |
| primarni_zatrideni | Primární zatřídění | String | Ubytování a stravování / Ranče, agroturistika |
| sekundarni_zatrideni | Sekundární zatřídění | String | Sport a relaxace / Jezdectví |
| tercialni_zatrideni | Terciální zatřídění | String |  |
| popis | Popis | String | Hlavní činností farmy je především chov ovcí, které se přes  |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Vrchlabí |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5215 |
| nazev_obce | Název obce | String | Rudník |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579645 |
| nazev_ulice | Název ulice | String |  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 204 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54372 |
| www | Webové stránky | String | http://bolkovsky-dvur.konvi.cz/ |
| mobil_1 | Mobil č. 1 | String | 420 603 751 540 |
| mobil_2 | Mobil č. 2 | String |  |
| fax | Fax | String |  |
| email_1 | Email | String | bolkovskydvur@seznam.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.61234500 15.74073111) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.74073111 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.612345 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | JEZ1 |

## Jak otevíráme data

> Podívejte se, jaké otevřené datové sady v současnosti Královéhradecký kraj publikuje.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Vymezení krajin, ke kterým jsou stanoveny cílové kvality v ZÚR

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Krajiny, Královéhradecký kraje

- Geometrie: esriGeometryPolygon | Záznamů: **25** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Vymezen%C3%AD_krajin/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Kraj | Název kraje | String | Královéhradecký kraj |
| Kraj_kod | Kód kraje dle RÚIAN | Integer | 86 |
| Plati_od | Datum nabytí účinnosti příslušné verze ZÚR | Date | 1683590400000 |
| RC_ZUR | Registrační číslo příslušné verze ZÚR v systému iKAS | Integer | 641 |
| Ident | Identifikátor území | String | 1 |
| CilKvality | Stručný popis cílových kvalit krajiny v daném území | String | 01/1; 01/2; 01/3; 01/4; 01/5; 01/6 |
| Odkazy | WWW odkazy na materiál (ZÚR) s podrobnějším popisem | String |   |
| Jev | Jev A017b z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů | String | A017b |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String |   |
| Zdroj | Název nebo popis zdroje informace | String | ÚZ ZÚR po vydání změn č. 1,2,3,4,5 |
| Dat_Aktual | Datum aktualizace údaje | Date | 1685404800000 |
| Poznamka | Poznámka | String | Krkonoše |

## Hranice obcí v Královéhradeckém kraji

> Datová sada s prostorovým vymezením obcí v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Obce, Hranice, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **448** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hranice_obc%C3%AD_v_Kr%C3%A1lov%C3%A9hradeck%C3%A9m_kraji/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_obce | Název obce | String | Červená Hora |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 505099 |
| pozn | Poznámka | String | Plošné vymezení obce |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | OBEC53 |

## Hackathon 2022 mentoři a porotci



_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Významné oborové záměry ekonomického rozvoje v ZÚR

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Významné oborové záměry ekonomického rozvoje, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **2** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/V%C3%BDznamn%C3%A9_oborov%C3%A9_z%C3%A1m%C4%9Bry_ekonomick%C3%A9ho_rozvoje_v_Z%C3%9AR/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Ident | Identifikátor záměru dle zdroje informace | String | PZ2 |
| Typ | Základní typ záměru | String | P |
| Popis | Stručný popis záměru | String | plocha průmyslové zóny Vrchlabí |
| Jev | Jev A118 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů  | String | A118 |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String |   |
| Zdroj | Název nebo popis zdroje informace | String | ZÚR KHK |
| Dat_Aktual | Datum aktualizace údaje | Date | -2209161600000 |
| Poznamka | Poznámka | String |   |

## Ostatní historické památky

> Datová sada s prostorovou lokalizací ostatních historických památek na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o. prostřednictvím Turistického portálu Královéhradeckého kraje.

Klíčová slova: Ostatní historické památky, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **51** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Ostatn%C3%AD_historick%C3%A9_pam%C3%A1tky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Barokní kašna v Pecce |
| popis | Popis | String | Barokní kašna se nachází na náměstí v Pecce, pochází z roku  |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Nová Paka |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5210 |
| nazev_obce | Název obce | String | Pecka |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573299 |
| nazev_ulice | Název ulice | String |  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 2 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50782 |
| www | Webové stránky | String | http://www.mestys-pecka.cz |
| telefon | Telefon | String | +420493799525 |
| wkt | Zápis vektorové geometrie | String | POINT(50.4803 15.6084) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.6084 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4803 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | OSTHP1 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 808 |

## Místní akční skupiny

> Datová sada s prostorovou lokalizací místních akčních skupin na území Královéhradeckého kraje. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: MAS, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **17** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/M%C3%ADstn%C3%AD_ak%C4%8Dn%C3%AD_skupiny/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Místní akční skupina Stolové hory, z. s. |
| por_cislo | Pořadové číslo | String | 159 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | MAS12 |

## Hranice okresů v Královéhradeckém kraji

> Datová sada s prostorovým vymezením okresů v Královéhradeckém kraji. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Hranice, Okresy, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **5** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hranice_okres%C5%AF_v_Kr%C3%A1lov%C3%A9hradeck%C3%A9m_kraji/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| pozn | Poznámka | String | Plošné vymezení okresu Hradec Králové |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | OKR1 |

## O webu Data KHK

> Informace o webu Data KHK a přehled jeho návštěvnosti.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Infografiky, analýzy a publikace

> Rozcestník zajímavých infografik, analýz a publikací z Královéhradeckého kraje.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Inovativní firmy

> Datová sada s prostorovou lokalizací inovativních firem na území Královéhradeckého kraje. Zdrojem dat je Agentura pro podporu podnikání a investic CzechInvest. Vysvětlivky k datové sadě jsou k dispozici zde

Klíčová slova: Inovativní firmy, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **211** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Inovativn%C3%AD_firmy/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | AB Pharma s.r.o. |
| ico | IČO | String | 0 |
| pravni_forma | Právní forma | String | N/A |
| hlavni_nace_popis | Popis hlavní klasifikace ekonomických činností | String | N/A |
| hlavni_nace_kod | Kód hlavní klasifikace ekonomických činností | String | N/A |
| kategorie_poctu_zamestancu | Kategorie počtu zaměstnanců | String | N/A |
| kategorie_obratu | Kategorie finančního obratu | String | N/A |
| zdroj | Zdroj | String | WiW |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String |  |
| cislo_popisne | Číslo popisné nebo evidenční podle typu čísla domovního | String |  |
| typ_cisla_domovniho | Typ čísla domovního | String |  |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | N/A |
| www | Webové stránky | String | N/A |
| wkt | Zápis vektorové geometrie | String | POINT(50.21158 15.83124) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.83124 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.21158 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | IF1 |

## Opravy silniční infrastruktury od roku 2016 - bodové

> Datová sada s prostorovou lokalizací oprav silniční infrastruktury v letech 2016 až 2022 vyjádřených bodově. Jedná se o silnice II. a III. třídy, tedy těch, které jsou ve vlastnictví Královéhradeckého kraje. Zdrojem dat je ÚDRŽBA SILNIC Královéhradeckého kraje a.s.

Klíčová slova: Silnice II. a III. třídy, Opravy, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **38** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Opravy_silni%C4%8Dn%C3%AD_infrastruktury_od_roku_2016_bodov%C3%A9/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Most ev.č. 501 – 006 Lázně Bělohrad |
| st_id | Identifikátor stavby | String | B0011 |
| oznaceni_silnice | Označení silnice | String | ev.č. 501-006 |
| rok_real | Rok realizace | String | 2017 |
| staniceni | Staničení | String | 9,972 |
| zdroj_financi | Zdroj financí | String | IROP |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jičín |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5207 |
| nazev_obce | Název obce | String | Lázně Bělohrad |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573094 |
| wkt | Zápis vektorové geometrie | String | POINT(50.4316 15.5727) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.5727 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4316 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | OSIB7 |

## Železniční stanice a zastávky VDKHK

> Datová sada s prostorovou lokalizací železničních stanic a zastávek využívaných pro Veřejnou dopravu Královéhradeckého kraje (VDKHK). Nejedná se o zobrazeních všech železničních stanic a zastávek, které se v Královéhradeckém kraji nacházejí. Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Stanice, Zastávky, Železnice, VDKHK, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **185** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/%C5%BDelezni%C4%8Dn%C3%AD_stanice_a_zast%C3%A1vky_VDKHK/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Doudleby nad Orlicí |
| oznaceni | Označení | String | 532606 |
| typ | Typ | String | Stanice Veřejné dopravy Královéhradeckého kraje |
| oteviraci_doba | Otevírací doba informačního centra Veřejné dopravy Královéhradeckého kraje | String | - |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Rychnov nad Kněžnou |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0524 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Kostelec nad Orlicí |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číslesníku ČSÚ | String | 5208 |
| nazev_obce | Název obce | String | Doudleby nad Orlicí |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 576301 |
| www | Webové stránky Veřejné dopravy Královéhradeckého kraje | String | https://www.dopravakhk.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.1101 16.26) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.26 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.1101 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ZSZ35 |

## Turistické regiony

> Datová sada s prostorovým vymezením turistických regionů v Královéhradeckém kraji vymezených dle agentury CzechTourism. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Turismus, Regiony, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **7** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Turistick%C3%A9_regiony/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Broumovsko |
| poznamka | Poznámka | String | Plošné vymezení turistického regiony Broumovsko |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | TR1 |

## Schválené individuální a programové dotace do ORP v letech 2021 - 2025

> Datová sada s prostorovou lokalizací schválených individuálních a programových dotací Královéhradeckého kraje do území správních obvodů obcí s rozšířenou působností v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dotace, Individuální dotace, Programové dotace, ORP, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **15** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_individu%C3%A1ln%C3%AD_a_programov%C3%A9_dotace_do_ORP/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| poc_dotaci | Počet dotací | Double | 2348 |
| finance | Množství financí v Kč | Double | 736819762 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | IPDOTORP1 |

## Autobusové linky VDKHK

> Datová sada s prostorovou lokalizací autobusových linek využívaných pro Veřejnou dopravu Královéhradeckého kraje (VDKHK). Nejsou zde obsaženy linky MHD a ty, které nejsou součástí dopravy v kompetenci kraje. Zdrojem dat je Odbor dopravy a silničního hospodářství Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Autobusy, Linky, VDKHK, Královéhradecký kraj

- Geometrie: esriGeometryPolyline | Záznamů: **197** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Autobusov%C3%A9_linky_VDKHK/FeatureServer/7

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| trasa | trasa | String | Turistický autobus s přepravou kol (baťoh): Jilemnice-Beneck |
| linka | linka | String | IDOL 949 |
| jizd_rad | jizd_rad | String | https://www.dopravakhk.cz/vyhledat-jizdni-rad.html?query=949 |
| nazev_vusc | nazev_vusc | String | Královéhradecký kraj, Liberecký kraj |
| kod_vusc | kod_vusc | String | CZ052, CZ051 |
| dp_id | dp_id | String | ALI194 |

## Organizace Královéhradeckého kraje

> Datová sada s prostorovou lokalizací organizací založených nebo zřízených Královéhradeckým krajem. Zdrojem dat je Odbor analýz a podpory řízení Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Organizace, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **126** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Organizace_Kr%C3%A1lov%C3%A9hradeck%C3%A9ho_kraje/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Archeopark Všestary o.p.s. |
| ico | IČO | String | 28852800 |
| pravni_forma | Právní forma | String | obecně prospěšná společnost |
| odbor | Odbor Krajského úřadu Královéhradeckého kraje | String | kultury a památkové péče |
| typ | Typ | String | spoluzakladatel Královéhradecký kraj |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Všestary |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 571091 |
| nazev_ulice | Název ulice | String | Všestary |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 238 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50312 |
| www_1 | Webové stránky č. 1 | String | http://archeoparkvsestary.cz/ |
| www_2 | Webové stránky č. 2 | String |  |
| fotografie_zdroj | Zdroj fotografie | String | Jaroslav Kocourek |
| fotografie_1 | Fotografie č. 1 | String | https://drive.google.com/file/d/10jzf20KcR6G8iJ27h7gGukou0Gb |
| fotografie_2 | Fotografie č. 2 | String | https://drive.google.com/file/d/1exUivA4xg3YVnFVR8vnkQb_Kd21 |
| wkt | Zápis vektorové geometrie | String | POINT(50.2588 15.7612) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.7612 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2588 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | SOKHK1 |

## Mapy MAS a DSO

> Na stránce naleznete mapové výstupy z analytické části Strategie rozvoje kultury, kulturního dědictví a kulturních a kreativních odvětví pro Královéhradecký kraj 2022–⁠2030.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Hudební kluby a festival parky

> Datová sada s prostorovou lokalizací hudebních klubů a festival parků na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Hudební kluby, Festival parky, Kultura, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **36** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hudebn%C3%AD_kluby_a_festival_parky/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | AC klub |
| ico | IČO | String | 25954881 |
| provozovatel | Provozovatel | String | Ivan Škaloud |
| popis | Popis | String | Výhradně nekuřácký klub je multikulturním prostorem, který s |
| typ_provozu | Typ provozu | String | hudební klub |
| kapacita | Kapacita | Double | 250 |
| kapacita_pozn | Doplňující informace ke kapacitě | String | 100 na sezení a 150 na stání |
| bezbariérovost | Bezbariérovost | String | ne |
| bezbariérovost_pozn | Doplňující informace k bezbariérovosti | String |  |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Zámostí  |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 684 |
| typ_cisla_domovniho | Typ čísla domovního | String | cp |
| cislo_orientacni | Číslo orientační | String | 1 |
| psc | PSČ | String | 50009 |
| www |  Webové stránky | String | http://www.acklubhk.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.2069731 15.8502192) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8502192 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2069731 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | HKFP1 |

## Zábavní centra

> Datová sada s prostorovou lokalizací zábavních center na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Zábavní centra, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **27** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Z%C3%A1bavn%C3%AD_centra/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Městské kulturní středisko Nová Paka |
| popis | Popis | String | Městské kulturní středisko v Nové Pace se anchází nedaleko M |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Nová Paka |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5210 |
| nazev_obce | Název obce | String | Nová Paka |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573248 |
| nazev_ulice | Název ulice | String | F. F. Procházky |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 101 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50901 |
| www | Webové stránky | String | http://www.mks-novapaka.cz |
| telefon | Telefon | String | +420493721928 |
| wkt | Zápis vektorové geometrie | String | POINT(50.4952 15.5138) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.5138 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.4952 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | ZABCEN1 |
| ds_id | Jedinečný identifikátor v datovém skladu Turistického portálu KHK | String | 2730 |

## Vymezení rozvojových oblastí v ZÚR

> Vrstva ze Zásad územního rozvoje Královéhradeckého kraje (ZÚR) z datového modelu DMG ÚAP. Zpracovatelem datového modelu je Hydrosoft Veleslavín s.r.o. Zdrojem dat je Odbor územního plánování a stavebního řádu Krajského úřadu Královéhradeckého kraje. Datová sada byla vytvořena v souřadnicovém systému S-JTSK. Dokumentace k datové sadě je ke stažení zde.

Klíčová slova: Zásady územního rozvoje Královéhradeckého kraje, Rozvojové oblasti, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **3** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Vymezen%C3%AD_rozvojov%C3%BDch_oblast%C3%AD_v_Z%C3%9AR/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| Ident | Identifikátor oblasti | String | OB4 |
| Nazev | Název oblasti | String | Rozvojová oblast Hradec Králové/Pardubice |
| Vyznam | Význam oblasti | String | S |
| Jev | Jev B006 z územně analytických podkladů dle přílohy č. 1 k vyhlášce č. 500/2006 Sb., ve znění pozdějších předpisů  | String | B006 |
| Pasport_Id | Identifikátor pasportu surových dat, která jsou podkladem pro daný údaj | String |   |
| Zdroj | Název nebo popis zdroje informace | String | ÚZ ZUR po změně 1,2,3,4,5 |
| Dat_Aktual | Datum aktualizace údaje | Date | 1683590400000 |
| Poznamka | Poznámka | String | zpřesnění vymezení z PÚR ČR |

## Lázně

> Datová sada s prostorovou lokalizací lázní na území Královéhradeckého kraje. Zdrojem dat je Odbor zdravotnictví Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Lázně, Zdravotnictví, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **7** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/L%C3%A1zn%C4%9B/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| druh_zarizeni | Druh zařízení | String | Lázeňská léčebna |
| poskytovatel | Poskytovatel | String | Státní léčebné lázně Janské Lázně, státní podnik |
| ico | IČO | String | 00024007 |
| zrizovatel | Zřizovatel | String | Ministersvo zdravotnictví |
| pozn | Poznámka | String | Dětská léčebna Vesna |
| forma | Forma zdravotní péče | String | následná lůžková péče - lázeňská léčebně rehabilitační péče |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Trutnov |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0525 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Trutnov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5214 |
| nazev_obce | Název obce | String | Janské Lázně |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 579351 |
| nazev_ulice | Název ulice | String | Horní promenáda |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 268 |
| typ_cisla_domovniho | Typ čísla domovního | String | cp |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54225 |
| www | Webové stránky | String | http://www.janskelazne.com/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.6279231 15.7803608) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.7803608 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.6279231 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | LAZ1 |

## Právní subjekty vykonávající činnost škol a školských zařízení

> Datová sada s prostorovou lokalizací právních subjektů vykonávajících činnost škol a školských zařízení v Královéhradeckém kraji. Zdrojem dat je Odbor školství a sportu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Školy, Školská zařízení, Školství, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **566** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Pr%C3%A1vn%C3%AD_subjekty_vykon%C3%A1vaj%C3%ADc%C3%AD_%C4%8Dinnost_%C5%A1kol_a_%C5%A1kolsk%C3%BDch_za%C5%99%C3%ADzen%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název školy nebo školského zařízení | String | 1. mateřská škola, Nová Paka, Husitská 217 |
| ico | IČO | String | 75017547 |
| pravni_forma | Právní forma | String | Příspěvková organizace zřízená územním samosprávným celkem |
| zrizovatel | Zřizovatel | String | obec či dobrovolný svazek obcí |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Jičín |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0522 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Nová Paka |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5210 |
| nazev_obce | Název obce | String | Nová Paka |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 573248 |
| nazev_ulice | Název ulice | String | Husitská |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 217 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50901 |
| www | Webové stránky | String | mshusitska.webnode.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.4868046376215 15.5170184854157) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.517018485415656 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.48680463762148 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PSSKOL1 |

## Schválené programové dotace do ORP v letech 2021 - 2025

> Datová sada s prostorovou lokalizací schválených programových dotací Královéhradeckého kraje do území správních obvodů obcí s rozšířenou působností v letech 2021 - 2025. Zdrojem dat je Odbor regionálního rozvoje, grantů a dotací Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Dotace, Programové dotace, ORP, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **15** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Schv%C3%A1len%C3%A9_programov%C3%A9_dotace_do_ORP/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| poc_dotaci | Počet dotací | Double | 1596 |
| finance | Množství financí v Kč | Double | 197682773 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | PDOTORP1 |

## Válečné hroby

> Datová sada s prostorovou lokalizací všech válečných hrobů na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Památky, Hroby, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **1423** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/V%C3%A1le%C4%8Dn%C3%A9_hroby/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| pocet_hrobu | Počet hrobů | Double | 2 |
| počet_pohrbenych | Počet pohřbených | Double | 87 |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Jaroměř |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5206 |
| nazev_obce | Název obce | String | Jaroměř |
| kod_obce | Kód obce | String | 574121 |
| katastr | Katastrální území | String | Jaroměř |
| cislo_parcely | Číslo parcely | String | 939 |
| wkt | Zápis vektorové geometrie | String | POINT(50.353117 15.918239) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.918239 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.353117 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | VH1 |
| id | Jedinečný identifikátor dle odboru kultury | String | 0 |

## Letní koupání

> Datová sada s prostorovou lokalizací areálů určených pro letní koupání na území Královéhradeckého kraje. Zdrojem dat je Královéhradecká krajská centrála cestovního ruchu, p. o.

Klíčová slova: Letní koupání, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **54** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Letn%C3%AD_koup%C3%A1n%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Aqa LAND – Areál zdraví a sportu Meziměstí |
| region | Region | String | Meziměstí / Broumovsko |
| primarni_zatrideni | Primární zatřídění | String | Sport a relaxace / Letní koupání |
| sekundarni_zatrideni | Sekundární zatřídění | String |  |
| tercialni_zatrideni | Terciální zatřídění | String |  |
| popis | Popis | String |  
 
Plavecký bazén: rozměr 25 m se čtyřmi drahami a hloubkou |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Náchod |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0523 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Broumov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5201 |
| nazev_obce | Název obce | String | Meziměstí |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 574252 |
| nazev_ulice | Název ulice | String | Školní |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 197 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 54983 |
| www | Webové stránky | String | www.koupaliste.mezimesti.cz |
| telefon_1 | Telefon č. 1 | String |  |
| telefon_2 | Telefon č. 2 | String |  |
| telefon_3 | Telefon č. 3 | String |  |
| mobil_1 | Mobil č. 1 | String | 420 778 772 653 |
| mobil_2 | Mobil č. 2 | String |  |
| fax | Fax | String |  |
| email_1 | Email č. 1 | String | aqaland@mezimesti.cz |
| email_2 | Email č. 2 | String |  |
| email_3 | Email č. 3 | String |  |
| wkt | Zápis vektorové geometrie | String | POINT(50.62904000 16.24167111) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 16.24167111 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.62904 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | LK1 |

## Informace o stránce

> Podmínky používání portálu Data KHK, prohlášení o přístupnosti, zpracování a ochrana osobních údajů, cookies, kontakt na správce webu a atribuce.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Hranice Královéhradeckého kraje

> Datová sada s prostorovým vymezením Královéhradeckého kraje. Zdrojem dat je Odbor analýz, podpory řízení a kontroly Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Hranice, Kraj, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **1** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Hranice_Kr%C3%A1lov%C3%A9hradeck%C3%A9ho_kraje/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královéhradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| pozn | Poznámka | String | Plošné vymezení Královéhradeckého kraje |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | KHK1 |

## Přírodní památky včetně ochranných pásem

> Datová sada s prostorovou lokalizací přírodních památek včetně ochranných pásem v Královéhradeckém kraji. Zdrojem dat je Odbor životního prostřední a zemědělství Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Přírodní památky, Ochranná pásma, Královéhradecký kraj

- Geometrie: esriGeometryPolygon | Záznamů: **170** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/P%C5%99%C3%ADrodn%C3%AD_pam%C3%A1tky_v%C4%8Detn%C4%9B_ochrann%C3%BDch_p%C3%A1sem/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| NAZEV | Název | String | Herlíkovické štoly |
| KOD | Evidenční číslo | Integer | 677 |
| KAT | Kategorie | String | PP |
| ROZL | Rozloha v hektarech | Double | 0.0081 |
| OP_TYP | Typ ochranného pásma | String | ZAK |
| IUCN | Kategorie IUCN | String | IV |
| PODKLAD | Podkladová mapa | String | KM |
| ZPRACOVAL | Zpracovatel | String |   |
| VZNIK | Vznik | Integer | 20001130 |
| ZMENA_G | Změna grafická | Integer | 20001130 |
| ZMENA_T | Změna textová | Integer | 20170901 |
| POZNAMKA | Poznámka | String |   |
| KAT_OP | Kategorie, ochranné pásmo | String | PP_PP |

## Feedback

> Formulář pro zanechání zpětné vazby k portálu Data KHK.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Školy a školská zařízení

> Datová sada s prostorovou lokalizací všech škol a školských zařízení v Královéhradeckém kraji podle zřizovatele. Zdrojem dat je Školský informační portál Královéhradeckého kraje.

Klíčová slova: Školy, Školská zařízení, Školství, Královéhradecký kraj, Dětský domov, Domov mládeže, Dům dětí a mládeže, Internát, Mateřská škola, Základní škola, Družina, Jídelna, Vyšší odborná škola, Základní umělecká škola

- Geometrie: esriGeometryPoint | Záznamů: **1626** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Seznam_%C5%A1kol_a_%C5%A1kolsk%C3%BDch_za%C5%99%C3%ADzen%C3%AD/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název školy nebo zařízení | String | Střední škola technická a řemeslná, Nový Bydžov, Dr. M. Tyrš |
| ico | IČO | String | 87751 |
| zrizovatel | Zřizovatel | String | kraj |
| redizo | Resortní identifikátor právnické osoby | String | 600011763 |
| zarizeni_druh | Druh zařízení | String | Střední škola |
| izo | Identifikační znak organizace | String | 877510000 |
| pravni_forma | Právní forma | String | příspěvková organizace |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Nový Bydžov |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5212 |
| nazev_obce | Název obce | String | Nový Bydžov |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 570508 |
| nazev_ulice | Název ulice | String | Dr. M. Tyrše |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 112 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50401 |
| www | Webové stránky | String | http://www.sstrnb.cz/ |
| wkt | Zápis vektorové geometrie | String | POINT(50.2399 15.486) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.486 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2399 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | SSSZ1 |

## Dashboardy

> Podívejte se na seznam dashboardů vytvořených speciálně pro datový portál Data KHK.

_bez ArcGIS GeoService (pravděpodobně webová stránka)_

## Divadla a filharmonie

> Datová sada s prostorovou lokalizací divadel a filharmonií na území Královéhradeckého kraje. Zdrojem dat je Odbor kultury, památkové péče a cestovního ruchu Krajského úřadu Královéhradeckého kraje.

Klíčová slova: Divadla, Filharmonie, Kultura, Královéhradecký kraj

- Geometrie: esriGeometryPoint | Záznamů: **33** | Služba: https://services6.arcgis.com/ogJAiK65nXL1mXAW/arcgis/rest/services/Divadla_a_filharmonie/FeatureServer/0

| Pole | Alias | Typ | Ukázka |
|---|---|---|---|
| nazev | Název | String | Adalbertinum |
| ico | IČO | String | 27472809 |
| popis | Popis | String | Budova Adalbertina byla postavena v letech 1895-97 v histori |
| typ_zarizeni | Typ zařízení | String | multifunkční prostor |
| zrizovatel | Zřizovatel | String | Hradecká kulturní a vzdělávací společnost |
| kapacita | Kapacita | Double | 440 |
| bezbarierovost | Bezbariérovost | String | ANO |
| typ_bezbarierovosti | Typ bezbariérovosti | String | tělesně postižené |
| nazev_vusc | Název vyššího územního samosprávného celku | String | Královehradecký kraj |
| kod_vusc | Kód vyššího územního samosprávného celku dle číselníku ČSÚ | String | CZ052 |
| nazev_okresu | Název okresu | String | Hradec Králové |
| kod_okresu | Kód okresu dle číselníku ČSÚ | String | CZ0521 |
| nazev_orp | Název správního obvodu obce s rozšířenou působností | String | Hradec Králové |
| kod_orp | Kód správního obvodu obce s rozšířenou působností dle číselníku ČSÚ | String | 5205 |
| nazev_obce | Název obce | String | Hradec Králové |
| kod_obce | Kód obce dle číselníku ČSÚ | String | 569810 |
| nazev_ulice | Název ulice | String | Československé armády |
| cislo_domovni | Číslo popisné nebo evidenční podle typu čísla domovního | String | 300 |
| typ_cisla_domovniho | Typ čísla domovního | String | č.p. |
| cislo_orientacni | Číslo orientační | String |  |
| psc | PSČ | String | 50003 |
| www | Webové stránky | String | http://www.adalbertinum.cz |
| telefon | Telefon | String | +420495514722 |
| email | Email | String | info@adalbertinum.cz |
| wkt | Zápis vektorové geometrie | String | POINT(50.2112056 15.8311492) |
| x | Zeměpisná délka v souřadnicovém systému WGS84 | Double | 15.8311492 |
| y | Zeměpisná šířka v souřadnicovém systému WGS84 | Double | 50.2112056 |
| dp_id | Jedinečný identifikátor v katalogu otevřených dat Data KHK | String | DIVFILH1 |
