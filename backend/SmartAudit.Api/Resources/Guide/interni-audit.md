## Jak se počítá roční ztráta v Kč
Ztráta vychází z naměřené vnitřní teploty, ne ze simulačního profilu. Budova drží teplotu T místo útlumové teploty (školy a kultura 18 °C, sociální služby a nemocnice 20 °C), takže navíc uteče H × (T − T_útlum) wattů, kde H = podlahová plocha × měrná ztráta podle stáří budovy (ČÚZK). Sčítají se jen hodiny, kde audit našel zjištění vytápění (přetápění ve volných dnech, chybějící noční útlum). Týden se přepočte na otopnou sezónu (33 týdnů) a vynásobí cenou tepla — paušálem 2,2 Kč/kWh, nebo cenou ze smlouvy o dodávce tepla z Registru smluv.

## Katalog opatření a orientační ceny
Seřízení ekvitermní regulace a časových programů MaR: servisní zásah 0–15 000 Kč, úspora typicky 5–15 % tepla. Víkendový a noční útlum v MaR: cca 0 Kč (nastavení). Posunutí ranního náběhu: cca 0 Kč. Termostatické hlavice: cca 15 000 Kč na podlaží. CO₂ semafor: 3 000–5 000 Kč za místnost. Decentrální rekuperační jednotka do učebny: cca 400 000 Kč za místnost (zásah do fasády). Výměna oken: statisíce Kč. Zateplení obálky: miliony Kč. Opatření se řadí od nejlevnějšího s nejrychlejší návratností.

## AI analýza objektu
V inspekčním panelu na záložce Audit může kraj nebo správce spustit AI analýzu. Model dostane jen data z aplikace a pevný katalog opatření, navrhne konkrétní opatření s návratností a krátký dopis správci budovy. Výsledek se propisuje do PDF auditu. Když se stav budovy nezměnil, vrátí se uložená analýza bez nového volání modelu.

## Smlouvy z Registru smluv
Záložka „Smlouvy“ v inspekčním panelu ukáže smlouvy organizace podle IČO — dodávky tepla a energií, servis kotelen a MaR, zateplení. Cena tepla ze smlouvy nahradí v auditu paušál. Když má organizace servisní smlouvu na kotelnu nebo MaR, AI doporučí seřízení uplatnit v rámci ní místo nové zakázky.

## PDF reporty
Na stránce Reporty jde stáhnout audit jednoho objektu (s grafy a lokalizační mapkou), souhrnný přehled portfolia s mapou okresů a souhrn pro Radu kraje na jednu stranu A4: ztráta kraje, pět priorit, pilot IQRF s návratností a návrh usnesení.
→ [Reporty](/reporty)

## Portfolio a Zjištění
Portfolio je tabulka všech objektů s řazením podle každého sloupce. Stránka Zjištění (Anomálie) řadí problémy podle odhadované roční ztráty v Kč — nahoře jsou budovy, kde oprava ušetří nejvíc. Správce vidí jen svoje budovy.
→ [Zjištění](/anomalie)
→ [Portfolio](/portfolio)

## Inspekční panel
Po kliknutí na budovu v mapě se otevře inspekční panel se záložkami: Přehled (organizace, obsazenost, kontext území), Telemetrie (grafy a „Místnosti teď“), Audit (zjištění, ztráta, AI analýza), Budova (karta ČÚZK, úpravy plochy a podlaží) a Smlouvy. Mapa budovu přiblíží a ukáže ve 3D v barvě stavu.

## Priorita investic a ohrožené oblasti
Kontext území v panelu ukazuje, jestli budova leží v hospodářsky a sociálně ohrožené oblasti (HSOO) a kolik dotací šlo do ORP v letech 2021–2025. Pomáhá to kraji při rozhodování, kam investovat dřív.

## Přidání vlastní budovy
Správce může na /budovy/nova přidat budovu své organizace: adresu najde našeptávač RÚIAN (nebo klik do mapy), obec, okres, památkovou ochranu a kartu ČÚZK aplikace doplní sama a audit budovu vyhodnotí do minuty. V mapě jde pak doplnit přilehlou budovu nebo upravit obrys.
→ [Přidat budovu](/budovy/nova)
