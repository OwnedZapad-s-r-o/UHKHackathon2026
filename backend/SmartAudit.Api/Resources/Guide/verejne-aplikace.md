## Co je SmartAudit
SmartAudit je aplikace Královéhradeckého kraje pro energetický audit a kvalitu vnitřního prostředí v budovách, které kraj zřizuje — střední školy, domovy mládeže, domovy pro seniory a další sociální služby, nemocnice a kulturní organizace (muzea, galerie, knihovny). Spojuje otevřená data kraje, katastr nemovitostí ČÚZK, počasí a měření z čidel IQRF.

## Jak najít svou školu nebo domov
Na stránce „Najdi svou školu“ (/objekty) napište název, obec nebo kód objektu. Výsledky jdou filtrovat podle okresu a druhu zařízení a seřadit podle kvality vzduchu. Na mapě se objekty ukazují barevně podle toho, jak se v nich teď dýchá. Kliknutím otevřete veřejnou kartu objektu.
→ [Najdi svou školu](/objekty)

## Veřejná karta objektu
Veřejná karta ukazuje, jak se v budově dýchá: CO₂ v nejhorší místnosti, průměrnou teplotu, počet hodin nad limitem za týden, graf CO₂ za posledních 24 hodin, hodnoty po místnostech, radu kdy větrat a co kraj v budově řeší. Částky v Kč veřejná karta záměrně neobsahuje.

## QR kód na dveře
Ke každé budově jde vytisknout QR kód (tlačítko „QR kód na dveře“ na veřejné kartě). Rodiče, žáci nebo návštěvníci si ho načtou telefonem a uvidí aktuální kvalitu vzduchu bez přihlášení a bez instalace aplikace.

## Okresy Královéhradeckého kraje
Kraj má pět okresů: Hradec Králové, Jičín, Náchod, Rychnov nad Kněžnou a Trutnov. Mapa aplikace je umí obarvit podle podílu budov se zjištěním a po kliknutí na okres ukáže jen jeho objekty.

## Otevřené API pro vývojáře
Data SmartAuditu jsou k dispozici jako otevřená data: JSON, GeoJSON a CSV na adrese /api/open/v1 bez registrace a bez klíče, pod licencí CC BY 4.0. Dokumentace je na /api/open/docs/. Katalog je ve formátu DCAT-AP-CZ, takže jde zapsat do Národního katalogu otevřených dat. Průvodce sám kód nepíše — na integraci slouží dokumentace API.
→ [Dokumentace otevřeného API](/api/open/docs/)

## Kdo může co vidět
Veřejnost (bez přihlášení) vidí kvalitu vnitřního vzduchu, teplotu a co kraj v budově řeší, ale ne částky v Kč ani návrhy dopisů. Krajský úřad vidí celé portfolio včetně odhadu ztrát v Kč. Správce budovy (ředitel, provozář) vidí jen budovy své organizace.

## Co umí průvodce
Průvodce SmartAuditu odpovídá jen na otázky o budovách kraje, jejich energii, vnitřním prostředí, čidlech a datech, ze kterých aplikace vychází. Umí najít objekty a ukázat je v mapě, porovnat okresy, vysvětlit zjištění auditu a pojmy jako CO₂ nebo noční útlum. Nepíše kód, recepty, slohové práce ani nic mimo tento účel a jeho pravidla nejdou změnit.

## Odkud jsou data
Seznam budov pochází z otevřených dat Královéhradeckého kraje (datakhk.cz): školy, sociální služby, nemocnice, organizace kraje. Půdorys, rok výstavby a plochu budovy dodává katastr ČÚZK (INSPIRE Budovy), počasí a prašnost Open-Meteo, školní prázdniny MŠMT, adresy RÚIAN a smlouvy Registr smluv.
