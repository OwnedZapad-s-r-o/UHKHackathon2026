# Další datové zdroje mimo katalog kraje — co jsem ověřil 8. 9. 2026

Pravidla, čl. 6.1 b) počítají i s **otevřenými daty z Národního katalogu v územním rozsahu
Královéhradeckého kraje** — takže data města Hradec Králové se do kritéria b počítají.

---

## 1. Otevřená data města Hradec Králové — ✅ POUŽITELNÉ, 51 sad, CC0

Portál: <https://ckan.mmhk.cz> (též opendata.mmhk.cz), CKAN API funguje:

```
https://ckan.mmhk.cz/api/3/action/package_list
https://ckan.mmhk.cz/api/3/action/package_show?id=<nazev>
```

**Licence Creative Commons CCZero** u všech ověřených sad. Formáty: GeoJSON (WGS84 i S-JTSK),
shapefile, XLSX.

### Sady, které přímo posilují naše nápady

| Sada | Co v ní je | Posiluje |
|---|---|---|
| **bezbarierovy-hradec** | Chodníky pro nevidomé · **indukční smyčky** · objekty pro nevidomé · **objekty pro vozíčky** · přechody pro nevidomé | nápad „Kraj bez bariér" |
| **skolstvi-obvody-zs** | Spádové obvody ZŠ, GeoJSON WGS84 | nápad „Kam mizí naši deváťáci" — úřední spádovost vs. reálné chování |
| **pocty-obyvatel-k-jednotlivym-adresnim-bodum-2017-2023** | Počty obyvatel po adresních bodech, **časová řada 2017–2023**, XLSX | demografie pod dojížďkou |
| skolstvi-skoly · socialni-sluzby-a-zdravotnictvi | body škol, sociálních a zdravotnických zařízení | obojí |
| doprava-pasport-* (chodníky, cyklostezky, komunikace, parkoviště) | pasporty komunikací | dopravní nápady |

⚠️ **Většina sad má `metadata_modified` v roce 2020.** Nejsou to živá data. U bezbariérovosti
to řekněte nahlas — pětiletá data o přístupnosti jsou lepší než žádná, ale nejsou aktuální.

**Celkem s krajem: 165 + 51 = 216 datových sad** dostupných pro kritérium b.

---

## 2. Živé polohy MHD Hradec — ⚠️ FUNGUJE, ALE PRÁVNĚ NEJASNÉ

Nenašel jsem oficiální API DPMHK ani GTFS-Realtime feed, **ani v katalogu města, ani v NKOD**.
Existuje ale funkční endpoint komunitní mapy <https://mapa.fcen.cz>:

```
GET https://mapa.fcen.cz/api/vehicle_data       → {age, vehicles:[…]}   ~31 kB
GET https://mapa.fcen.cz/api/platform_list      → {platforms:[…]}       428 zastávek
GET https://mapa.fcen.cz/api/platform_departures?id=<id>
```

**Ověřeno 8. 9. 2026 v 19:00 — 49 vozidel v reálném čase.** Vzorek jednoho vozu:

```json
{"vehicle_id":"069", "latitude":50.19298, "longitude":15.84804, "azimuth":162,
 "line":"2", "line_next":"2", "planned_departure_time":1788887589,
 "delay":239, "speed":44, "number_of_passengers":12,
 "start_name":"TERMINÁL HD", "direction_name":"NOVÝ HRADEC KRÁLOVÉ"}
```

Pozoruhodné je `delay` (zpoždění v sekundách — tady 4 minuty) a hlavně
**`number_of_passengers`** — obsazenost vozu. To je věc, kterou skoro nikdo nemá.

`platform_list` má souřadnice jako celá čísla (`lat: 5021381` = 50,21381 — dělit 100 000).

### Proč to přesto není hotová věc

1. **Žádné CORS hlavičky** → z prohlížeče to nezavoláte. Potřebujete vlastní proxy
   (20 řádků v Node nebo Pythonu), což vám bere „žádný backend" a přidává krok nasazení.
2. **mapa.fcen.cz je komunitní projekt, ne oficiální zdroj DPMHK.** Není to publikováno
   jako otevřená data a podmínky užití nikde nejsou. Čl. 5.2 b) přitom chce data,
   *„jejichž podmínky užití umožňují soutěžícímu legálně publikovat alespoň část
   používaných dat jako otevřená data"*.
3. Pokrývá **jen MHD Hradce** (49 vozů), ne kraj.

➜ **Co s tím:** hned ráno 15. 10. se zeptejte mentorů nebo organizátorů, jestli je
napojení v pořádku, případně jestli existuje oficiální feed od DPMHK. Mentoři jsou
na místě celý den. Nestavte na tom jádro aplikace, dokud to nemáte potvrzené.

---

## 3. Kamery Hradec Králové — ⚠️ ŽIVÉ, ALE BEZ CORS A MIMO KATALOG

Zdroj: <https://doprava.hradeckralove.cz/kamery/> — tři lokality: **soutok, tesla, mileta**
(+ stránka „academic").

```
https://doprava.hradeckralove.cz/uploads/camera/soutok/nahled.jpg
https://doprava.hradeckralove.cz/uploads/camera/tesla/nahled.jpg
https://doprava.hradeckralove.cz/uploads/camera/mileta/<IP>_01_YYYYMMDDHHMMSS_TIMING.jpg
```

Ověřeno: všechny tři vrací obrázek, `Last-Modified` odpovídá **aktualizaci jednou za minutu**.
Kamera *mileta* má v názvu souboru časové razítko, takže archiv je pravděpodobně
enumerovatelný.

⚠️ **Také bez CORS.** Zobrazit je v `<img>` jde bez problému, ale **číst pixely nebo je
poslat do AI na analýzu nejde** — na to potřebujete proxy. A nejsou publikované jako
otevřená data, podmínky užití nikde nejsou uvedené.

---

## Shrnutí pro rozhodování

| Zdroj | Otevřená data? | Živé? | Z prohlížeče? | Verdikt |
|---|---|---|---|---|
| Katalog kraje (165 sad) | ✅ ano | ne | ✅ CORS `*` | páteř aplikace |
| Hradec CKAN (51 sad, CC0) | ✅ ano | ne | ✅ | **zdarma +51 sad ke kritériu b** |
| MHD live (mapa.fcen.cz) | ❌ ne | ✅ ano | ❌ nutná proxy | jen po potvrzení mentory |
| Kamery HK | ❌ ne | ✅ ano | ❌ nutná proxy | nanejvýš ozdoba |
