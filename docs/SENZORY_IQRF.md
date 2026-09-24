# Kompletní průvodce: Senzory IQRF v aplikaci SmartAudit KHK

Tento dokument slouží jako ucelená technická, implementační i obchodní dokumentace integrace bezdrátových senzorů **IQRF** (vyvinutých a standardizovaných společnostmi **MICRORISC s.r.o.** a **IQRF Tech s.r.o.** v Jičíně) do krajského energetického systému **SmartAudit KHK**.

---

## 1. Kontext a klíčové osoby projektu

V rámci Hackathonu Královéhradeckého kraje 2026 je společnost **MICRORISC s.r.o.** (IČO: 25921681, se sídlem v Jičíně) oficiálním partnerem soutěže (uvedeno v oficiálních pravidlech v `_private/pravidla.txt`).

V odborné porotě zasedá:
* **Ing. Jan Vašta** – obchodní ředitel (Sales Director) společností **MICRORISC s.r.o.** a **IQRF Tech s.r.o.**, člen představenstva IQRF Alliance.
* **Co ho zajímá a co ocení:** Reálné využití české technologie IQRF v budovách zřizovaných Královéhradeckým krajem (školy, domovy sociálních služeb, nemocnice, úřady). Zásadní je **obchodní případ pro kraj**, snadnost instalace bez stavebních zásahů a živá ukázka, kde aplikace automaticky objeví senzory a plynule přejde ze simulace na skutečná telemetrická data.

---

## 2. Jak technologie IQRF funguje

Technologie IQRF je bezdrátová mesh platforma pracující v bezlicenčním pásmu **868 MHz** (v Evropě) s velmi nízkou spotřebou (LPWAN / Sub-GHz).

```
[ Bezdrátové uzly v učebnách ]
  (Uzel 1: CO2, T, RH)  <--\
  (Uzel 2: Teplota, PIR) <--+- IQMESH (868 MHz, směrování přes uzly)
  (Uzel 3: Osvětlení)   <--/
            │
            ▼
[ IQRF Gateway v budově ] (např. IQUBE / Raspberry Pi + IQRF klobouk)
  Obsahuje IQRF Coordinator transceiver + běží zde IQRF Gateway Daemon
            │
            ▼ MQTT / MQTTS (TCP port 1883 / 8883) přes školní LAN nebo LTE
[ Krajský server SmartAudit KHK ]
  - Mosquitto MQTT Broker
  - Backend API: IqrfGatewayService (přijímá data a dotazuje se uzlů)
  - SensorIngestService & SimulationHandover (zápis do PostGIS, přepnutí zóny do ŽIVĚ)
  - Web UI: /senzory (správa bran, live grafy, přiřazení do 3D řezu budovy)
```

### Hlavní technické přednosti pro školní budovy:
1. **Frekvence 868 MHz vs. Wi-Fi (2.4 / 5 GHz):** Sub-GHz vlny prochází masivními zdmi a stropy starých historických školních budov nesrovnatelně lépe než Wi-Fi.
2. **Protokol IQMESH (samoorganizující se mesh):** Každý napájený uzel může fungovat jako retranslátor (router). Síť nevyžaduje přímou viditelnost na bránu; signál „přeskakuje“ z učebny do učebny (až 240 skoků a uzlů v jedné síti).
3. **Nenarušuje školní síť:** Do stovek čidel ve třídách se nezadávají hesla na školní Wi-Fi ani se do nich nekupují SIM karty. Jediným prvkem napojeným na internet je centrální brána (Gateway).
4. **Standardizace IQRF Standard:** Jednotné rozhraní pro periférie (např. senzorická periférie `0x5E`), definující kódování hodnot, jednotek a přesnosti.

---

## 3. Jak se senzory instalují do škol

Instalace v typické škole (např. gymnázium nebo střední průmyslová škola s 15–30 třídami) probíhá bez jakéhokoliv sekání do zdí:

### A. Umístění senzorů ve třídách (učebnách)
* **Kde instalovat:** Na vnitřní stěnu učebny do výšky cca **1,3 až 1,7 m** nad podlahu (zóna dýchání sedících studentů a učitele).
* **Čemu se vyhnout:** 
  * Neumísťovat bezprostředně vedle oken nebo větracích klapek (docházelo by ke zkreslení čerstvým vzduchem).
  * Neumísťovat přímo nad topná tělesa / radiátory (zkreslení teploty).
  * Neumísťovat na místa vystavená přímému celodennímu slunečnímu svitu.
* **Napájení:** 
  * Čidla CO₂ (např. Protronix) vyžadují kvůli optickému infračervenému NDIR senzoru více energie – instalují se na stěnu s napájením 230 V (přes instalační krabici KU68 nebo miniaturní adaptér do zásuvky / USB-C).
  * Čistě teplotní a vlhkostní senzory či pohybová čidla (PIR) mohou běžet na lithiovou baterii s životností 5–7 let.

### B. Umístění centrální brány (IQRF Gateway)
* Typicky se umisťuje do **serverovny, kabinetu správce IT nebo do ředitelny**.
* **Konektivita k internetu:**
  * **Varianta 1 (doporučená):** Zapojení do školní LAN sítě přes běžný ethernetový kabel (RJ45). Brána dostane IP přes DHCP a vyžaduje pouze odchozí spojení na MQTT port krajského serveru.
  * **Varianta 2 (izolovaná):** Pokud správce IT školy nechce do školní sítě pouštět cizí zařízení, brána se osadí LTE/4G USB modemem nebo SIM kartou a komunikuje naprosto nezávisle na školní infrastruktuře.

---

## 4. Cenová kalkulace a ekonomický model

Ekonomika nasazení je jedním z hlavních argumentů pro vedení kraje (Ing. Jaromír Dědeček) i pro obchodní ředitele (Ing. Jan Vašta):

### Ceník jednotlivých komponent (orientační velkoobchodní ceny bez DPH):
* **Kombinované prostorové čidlo kvality vzduchu (CO₂ + Teplota + Vlhkost) s IQRF:**
  * Např. Protronix NLII-CO2-IQRF nebo alianční partneři: **cca 5 900 – 7 490 Kč** (při odběru pro desítky škol množstevní slevy až pod 5 500 Kč).
* **Kombinované čidlo Teplota + Vlhkost (chodby, tělocvičny, sklady):**
  * **cca 1 200 – 2 200 Kč**.
* **Centrální IQRF brána (Gateway):**
  * IQUBE (Orange Pi Zero + IQRF transceiver TR-76D): **cca 3 895 Kč**.
  * Průmyslová brána na DIN lištu do rozvaděče (např. Unipi s IQRF modulem): **cca 6 000 – 9 000 Kč**.
* **Samostatné transceivery pro integraci (TR-76D / TR-76DA):** **cca 250 – 330 Kč/ks**.

### Příkladový rozpočet pro běžnou krajskou střední školu (15 učeben + 3 společné prostory):

| Položka | Počet | Jedn. cena bez DPH | Celkem bez DPH |
|---|---|---|---|
| IQRF Gateway IQUBE (LAN/Wi-Fi) | 1 ks | 3 895 Kč | 3 895 Kč |
| Prostorové čidlo CO₂ / T / RH do tříd | 15 ks | 6 200 Kč | 93 000 Kč |
| Čidlo teploty a pohybu pro chodby a tělocvičnu | 3 ks | 1 500 Kč | 4 500 Kč |
| Montáž, spárování sítě a zprovoznění | 1 den | 8 000 Kč | 8 000 Kč |
| **Celková jednorázová investice** | | | **109 395 Kč** |

### Návratnost a přínos pro školu a kraj:
1. **Přímá úspora za vytápění (10–25 %):** SmartAudit KHK na základě teplotních profilů okamžitě odhalí přetápění o víkendech, noční nevytápění nebo větrání okny při spuštěném topení. Při ročních nákladech školy na vytápění 400 000 – 800 000 Kč je investice do čidel zpět během **1 až 2 topných sezón**.
2. **Zdraví a soustředění žáků:** Hladina CO₂ nad 1 500 ppm způsobuje prokazatelný pokles pozornosti a únavu. Čidlo s LED indikací nebo notifikace v aplikaci upozorní učitele na nutnost krátkého intenzivního vyvětrání.
3. **Žádné paušální poplatky (Zero Vendor Lock-in):** Aplikace SmartAudit KHK běží na krajském serveru jako open-source bez měsíčních licencí komerčních cloudů.

---

## 5. Rozhraní, protokoly a technická dokumentace

### Oficiální zdroje a dokumentace:
* **IQRF dokumentace a standardy:** [https://docs.iqrf.org](https://docs.iqrf.org)
* **IQRF Gateway Daemon:** [https://docs.iqrf.org/iqrf-gateway-daemon/](https://docs.iqrf.org/iqrf-gateway-daemon/)
* **IQRF Standard Sensor specifikace:** [https://www.iqrf.org/standards/sensor](https://www.iqrf.org/standards/sensor)
* **E-shop a hardware MICRORISC:** [https://eshop.microrisc.com](https://eshop.microrisc.com)

### Protokol komunikace přes MQTT

Brána (IQRF Gateway Daemon) komunikuje s brokerem pomocí standardního formátu **IQRF JSON API**:

* **Topic pro dotazy (aplikace -> gateway):** `gateway/{gatewayId}/iqrf/requests`
* **Topic pro odpovědi (gateway -> aplikace):** `gateway/{gatewayId}/iqrf/responses`

#### A. Zjištění spárovaných uzlů (`iqrfEmbedCoordinator_BondedDevices`)
Aplikace se ptá koordinátoru (adresa 0), jaká čidla jsou v síti fyzicky spárovaná:
```json
{
  "mType": "iqrfEmbedCoordinator_BondedDevices",
  "data": {
    "msgId": "sa-init-001",
    "req": { "nAdr": 0, "param": {} },
    "returnVerbose": true
  }
}
```
Odpověď brány:
```json
{
  "mType": "iqrfEmbedCoordinator_BondedDevices",
  "data": {
    "msgId": "sa-init-001",
    "rsp": {
      "nAdr": 0,
      "result": {
        "bondedDevices": [1, 2, 5]
      }
    },
    "status": 0,
    "statusStr": "ok"
  }
}
```

#### B. Vyčtení hodnot senzorů uzlu (`iqrfSensor_ReadSensorsWithTypes`)
Aplikace periodicky (např. každých 15–60 s) dotazuje uzel `nAdr: 1`:
```json
{
  "mType": "iqrfSensor_ReadSensorsWithTypes",
  "data": {
    "msgId": "sa-req-1234",
    "req": {
      "nAdr": 1,
      "param": { "sensorIndexes": -1 }
    },
    "returnVerbose": true
  }
}
```
Odpověď brány s naměřenými hodnotami (standardní formát IQRF):
```json
{
  "mType": "iqrfSensor_ReadSensorsWithTypes",
  "data": {
    "msgId": "sa-req-1234",
    "rsp": {
      "nAdr": 1,
      "result": {
        "sensors": [
          {
            "id": "TEMPERATURE",
            "type": 1,
            "name": "Temperature",
            "shortName": "TEM",
            "value": 21.8,
            "unit": "°C",
            "decimalPlaces": 2
          },
          {
            "id": "CARBON_DIOXIDE",
            "type": 2,
            "name": "Carbon dioxide",
            "shortName": "CO2",
            "value": 850.0,
            "unit": "ppm",
            "decimalPlaces": 0
          },
          {
            "id": "RELATIVE_HUMIDITY",
            "type": 128,
            "name": "Relative humidity",
            "shortName": "RH",
            "value": 45.2,
            "unit": "%",
            "decimalPlaces": 1
          }
        ]
      }
    },
    "status": 0,
    "statusStr": "ok"
  }
}
```

### Mapování typů senzorů v kódu SmartAudit (`IqrfSensorTypes.cs`)

Podle číselného typu `type` z IQRF Standard Sensor backend automaticky rozpozná měřenou veličinu:

| IQRF Typ | Standardní název | Namapovaná veličina ve SmartAudit | Obvyklá jednotka |
|---|---|---|---|
| **1** | `TEMPERATURE` | `SensorMetric.Temperature` | °C |
| **2** | `CARBON_DIOXIDE` | `SensorMetric.Co2` | ppm |
| **3** | `VOC` | `SensorMetric.Voc` | ppm / ppb |
| **11** | `ILLUMINANCE` | `SensorMetric.Illuminance` | lx |
| **14** | `CO` | `SensorMetric.Other` (Havarijní) | ppm |
| **16** | `PRESSURE` | `SensorMetric.Pressure` | hPa |
| **18** | `PM2_5` | `SensorMetric.Pm25` | µg/m³ |
| **19** | `SOUND_LEVEL` | `SensorMetric.Noise` | dBA |
| **26** | `PM10` | `SensorMetric.Other` | µg/m³ |
| **28** | `TVOC` | `SensorMetric.Voc` | ppb / mg/m³ |
| **128** | `RELATIVE_HUMIDITY` | `SensorMetric.Humidity` | % |
| **129** | `BINARY_DATA7` | `SensorMetric.Motion` (PIR) | stav 0 / 1 |

### Detailní technická specifikace a katalog jednotlivých senzorů

V ekosystému IQRF a pro nasazení do krajských škol a budov jsou standardizovány tyto konkrétní senzorické jednotky:

#### 1. Senzor CO₂ (Oxid uhličitý) — `Type 0x02` (STD_SENSOR_TYPE_CO2)
* **Měřicí princip:** Optický nedisperzní infračervený detektor (**NDIR**) s pozlacenou optickou komorou a duální vlnovou délkou.
* **Měřicí rozsah:** Standardně 400 až 5 000 ppm (rozšířený až do 10 000 ppm).
* **Rozlišení a přesnost:** Rozlišení 1 ppm (2 bajty, `uint16`); přesnost typicky **±(30 ppm + 3 % z naměřené hodnoty)**.
* **Kalibrační mechanismus:** Integrovaná samočinná kalibrace **ABC (Automatic Baseline Correction)**, která sleduje nejnižší naměřenou hodnotu v cyklu 8 dní a automaticky ji srovnává s úrovní čerstvého venkovního vzduchu (~415 ppm). Pro prostory s nepřetržitým provozem lze ABC vypnout a provést manuální kalibraci.
* **Časové konstanty:** Zahřátí senzoru < 60 s, stabilní provoz po 15 min; doba odezvy $t_{90} < 90\text{ s}$.
* **Hygienické normy a vliv na výuku (vyhláška č. 410/2005 Sb. a ČSN EN 16798-1):**
  * `< 1 000 ppm`: Optimální vnitřní ovzduší, maximální soustředěnost a pracovní výkon.
  * `1 000 – 1 500 ppm`: Hranice únavy, pokles koncentrace žáků, doporučení intenzivně vyvětrat.
  * `> 1 500 ppm`: Výrazná ospalost, bolest hlavy, nárůst chybovosti až o 50 %. V aplikaci spouští varovnou notifikaci a vizuální alarm.
* **Napájení:** Vzhledem k vyhřívání NDIR komory vyžaduje trvalé napájení 12–24 V DC / 230 V AC (cca 0,5–1 W).

#### 2. Senzor Teploty (Temperature) — `Type 0x01` (STD_SENSOR_TYPE_TEMPERATURE)
* **Měřicí princip:** Křemíkový polovodičový pásmový senzor s integrovaným 16-bitovým A/D převodníkem.
* **Měřicí rozsah:** -40 °C až +85 °C (kalibrovaný pro vnitřní prostory v rozsahu 0 °C až +50 °C).
* **Rozlišení a přesnost:** Rozlišení **0,0625 °C**; typická přesnost **±0,2 °C až ±0,4 °C** v pásmu pokojových teplot.
* **Přínos pro energetický audit SmartAudit KHK:**
  * Výpočet reálných **denostupňů** (HDD — Heating Degree Days).
  * Každý $1\text{ °C}$ přetápění nad projektovanou teplotu ($20\text{ °C}$ pro třídy) představuje **cca 6 % zbytečně propálené energie**.
  * Detekce nočních a víkendových anomálií (např. teplota $23\text{ °C}$ v sobotu v noci = jasná zbytečná ztráta).

#### 3. Senzor Relativní vlhkosti (Humidity) — `Type 0x80` (STD_SENSOR_TYPE_HUMIDITY)
* **Měřicí princip:** Kapacitní polymerní dielektrikum reagující změnou kapacity na molekuly vody ve vzduchu.
* **Měřicí rozsah:** 0 až 100 % RH (nekondenzující).
* **Rozlišení a datový formát:** 1 bajt (přenáší hodnotu `RH * 2`), rozlišení **0,5 % RH**; přesnost **±2 až ±3 % RH**.
* **Doba odezvy:** Rychlá odezva $t_{63} < 8\text{ s}$.
* **Kritéria pro školy:**
  * Optimální vlhkost: **40 % až 60 % RH**.
  * Pod 30 %: Vysychání sliznic dětí, pálení očí, vyšší aerosolový přenos virových infekcí.
  * Nad 65 %: Kondenzace na chladných tepelných mostech (okenní ostění, rohy) a riziko vzniku zdraví nebezpečných plísní.

#### 4. Senzor Těkavých organických látek (VOC / TVOC) — `Type 0x03` a `Type 0x1C`
* **Měřicí princip:** Polovodičový senzor s vyhřívaným kovovým oxidem (**MOX**).
* **Detekované spektrum plynů:** Alkoholy, aldehydy, alifatické a aromatické uhlovodíky, pachy, výpary z dezinfekcí, čisticích prostředků, nového nábytku, podlahových krytin a barev.
* **Rozsah a interpretace:** Vyhodnocuje index kvality ovzduší (IAQ index 0–500) nebo ppm ekvivalent etanolu/isobutylenu.
* **Využití:** Upozornění na nevhodné použití chemických čisticích prostředků ve třídách a tělocvičnách nebo špatnou výměnu vzduchu po malování učeben.

#### 5. Senzor Osvětlení (Illuminance) — `Type 0x0B` (STD_SENSOR_TYPE_ILLUMINANCE)
* **Měřicí princip:** Křemíková fotodioda s přesným optickým filtrem kopírujícím spektrální citlivost lidského oka ($V(\lambda)$ křivka podle CIE).
* **Měřicí rozsah:** 0 až 65 535 luxů (dynamický rozsah přes 16 bitů).
* **Splnění norem (ČSN EN 12464-1 — Osvětlení vnitřních pracovních prostor):**
  * Třídy a posluchárny: požadavek na minimálně **300–500 lx** na lavicích žáků.
  * Tabule a prezentační plochy: minimálně **500 lx**.
* **Energetická kontrola ve SmartAudit:** Identifikace svícení v prázdných místnostech nebo zbytečného umělého svícení za jasného slunečného dne.

#### 6. Senzor Polétavého prachu (PM2.5 / PM10) — `Type 0x12` a `Type 0x1B`
* **Měřicí princip:** Laserový optický rozptyl světla (**Laser Scattering Particle Counter**). Miniaturní ventilátor vhání vzduch před laserový paprsek a fotodioda z odražených záblesků počítá velikost a hustotu částic.
* **Měřicí rozsah:** 0 až 1 000 µg/m³ pro frakce **PM2.5** (jemné částice pronikající do plicních sklípků) a **PM10**.
* **Přesnost:** ±10 µg/m³ v pásmu 0–100 µg/m³.
* **Význam pro Královéhradecký kraj:** Školní budovy ležící u frekventovaných silnic I. a II. třídy (např. Hradec Králové, Náchod, Trutnov, Jičín). Aplikace doporučí, kdy větrat s ohledem na venkovní smog a inverzní situace.

#### 7. Senzor Hladiny hluku (Sound Pressure Level) — `Type 0x13`
* **Měřicí princip:** Akustický MEMS mikrofon s hardwarovým filtrem váhové křivky A (**dBA**).
* **Měřicí rozsah:** 35 až 105 dBA.
* **Využití:** Monitorování akustické pohody ve třídách, speciálních učebnách a školních jídelnách; detekce překročení hygienických limitů hluku.

#### 8. Senzor Atmosférického tlaku (Atmospheric Pressure) — `Type 0x10`
* **Měřicí princip:** Piezorezistivní křemíkový barometrický senzor.
* **Měřicí rozsah:** 300 až 1 100 hPa s přesností ±1 hPa.
* **Význam pro přesnost měření:** NDIR senzory CO₂ měří parciální tlak molekul oxidu uhličitého. Pro absolutně přesný přepočet na jednotky ppm je nezbytná barometrická korekce podle nadmořské výšky školy (např. školy v Krkonoších vs. Polabí).

#### 9. Pohybový senzor přítomnosti osob (PIR) — `Type 0x81` (Binary Data 7)
* **Měřicí princip:** Pyroelektrický infračervený detektor s vícesegmentovou Fresnelovou čočkou.
* **Detekční zóna:** Úhel záběru 100° až 120°, detekční dosah 6 až 10 metrů.
* **Klíčový vstup pro AI audit ve SmartAudit:** Křížová validace s vytápěním. Pokud teplota dosahuje 22 °C, ale PIR čidlo neeviduje žádný pohyb po dobu 3 hodin v pracovní den, aplikace okamžitě generuje hlášení o plýtvání teplem v neobsazené učebně.

#### 10. Havarijní senzor oxidu uhelnatého (CO) — `Type 0x0E`
* **Měřicí princip:** Elektrochemický článek s životností 5–7 let.
* **Měřicí rozsah:** 0 až 500 ppm CO.
* **Využití:** Kotelny, technické místnosti a strojovny škol vytápěných plynem či peletami (bezpečnostní havarijní monitorování).

---

## 6. Návod krok za krokem: Jak škola připojí senzory do centrální HK aplikace

Když celá aplikace SmartAudit KHK běží na centrálním serveru kraje (např. doména `smartaudit.datakhk.cz`), proces připojení nové školy je přímočarý:

```
+-----------------------------------------------------------------------------------+
| KROK 1: V budově školy                                                            |
| Fyzická montáž čidel na zeď a jejich spárování (Bonding) ke školní IQRF Gateway   |
+-----------------------------------------------------------------------------------+
                                         │
                                         ▼
+-----------------------------------------------------------------------------------+
| KROK 2: V administraci školní Gateway (IQRF Gateway Webapp)                       |
| Nastavení MQTT klienta směrem na krajský server: ssl://smartaudit.datakhk.cz:8883 |
+-----------------------------------------------------------------------------------+
                                         │
                                         ▼
+-----------------------------------------------------------------------------------+
| KROK 3: V krajské aplikaci SmartAudit KHK (/senzory)                              |
| Správce školy/kraje klikne "Připojit gateway" a vybere svou školu ze seznamu      |
+-----------------------------------------------------------------------------------+
                                         │
                                         ▼
+-----------------------------------------------------------------------------------+
| KROK 4: Automatické objevení kanálů                                               |
| SmartAudit vyčte uzly i veličiny a zobrazí je v seznamu nalezených senzorů        |
+-----------------------------------------------------------------------------------+
                                         │
                                         ▼
+-----------------------------------------------------------------------------------+
| KROK 5: Přiřazení do místnosti                                                    |
| Správce zvolí místnost (např. Učebna 2.A) -> simulace se vypne, naskočí odznak ŽIVĚ|
+-----------------------------------------------------------------------------------+
```

### Krok 1: Fyzické spárování (Bonding) senzoru k bráně
1. Brána a senzory se vybalí z krabic.
2. V prostředí brány (např. webové rozhraní IQRF Gateway Webapp na lokální IP adrese brány `http://192.168.1.xxx` nebo přes nástroj IQRF IDE):
   * Otevře se záložka **IQRF Network → Coordinator → Bond node**.
   * Na senzoru se stiskne párovací tlačítko (nebo se přiloží NFC telefon u Smart Connect technologie).
   * Koordinátor přiřadí senzoru síťovou adresu uzlu (např. Node `1`, Node `2`).

### Krok 2: Konfigurace MQTT spojení z brány na krajský server
V bráně školy v souboru `/etc/iqrfgd2/iqrf__MqttMessaging.json` (nebo ve webovém rozhraní v menu **Konfigurace → Messaging → MQTT**) se nastaví:

```json
{
  "component": "iqrf::MqttMessaging",
  "instance": "SmartAuditKHK",
  "BrokerAddr": "ssl://smartaudit.datakhk.cz:8883",
  "ClientId": "skola-gymnazium-bozeny-nemcove-gw01",
  "User": "skola-gymnazium-bozeny-nemcove-gw01",
  "Password": "<heslo z detailu gatewaye v aplikaci>",
  "TopicRequest": "gateway/skola-gymnazium-bozeny-nemcove-gw01/iqrf/requests",
  "TopicResponse": "gateway/skola-gymnazium-bozeny-nemcove-gw01/iqrf/responses",
  "Qos": 1,
  "Persistence": 1,
  "EnabledSSL": true,
  "TrustStore": "/etc/iqrf-gateway-daemon/certs/smartaudit-ca.crt",
  "EnableServerCertAuth": true,
  "acceptAsyncMsg": true
}
```
*Poznámka:* `ClientId` a část topicu `gateway/{id}/iqrf` tvoří unikátní identifikátor brány (např. `skola-gymnazium-bozeny-nemcove-gw01`).
Jméno a heslo gatewaye i certifikát CA brokeru (`smartaudit-ca.crt`) ukáže aplikace na detailu gatewaye — broker pustí jen gateway,
kterou správce předtím v aplikaci připojil (krok 3 proto v praxi proběhne před krokem 2).

### Krok 3: Registrace brány ve webové aplikaci SmartAudit KHK
1. Pověřený správce školy nebo energetik kraje se přihlásí do aplikace na krajském serveru a otevře sekci **Senzory** (`/senzory`).
2. Správce vyplní formulář **Zadejte gateway do aplikace** (krok 1 průvodce):
   * **Objekt:** vybere ze seznamu budov svou školu (např. `Gymnázium Boženy Němcové, Hradec Králové`).
   * **Název:** např. `Brána hlavní budova - 2. patro`.
   * **Id gatewaye:** předvyplněno `skola-gymnazium-bozeny-nemcove-gw01`.
   * **Interval dotazování:** zvolí např. `každých 60 s` (pro běžný provoz) nebo `15 s`.
   * **Adresy uzlů:** nechá prázdné – aplikace si je automaticky vyčte sama!

### Krok 4: Automatické nalezení kanálů a přiřazení do místností
1. SmartAudit pošle dotaz koordinátoru a během několika sekund se na obrazovce detailu brány objeví nalezené uzly a jejich senzory:
   * `Uzel 1 / Teplota (21.4 °C)`
   * `Uzel 1 / CO₂ (720 ppm)`
   * `Uzel 1 / Vlhkost (46 %)`
2. U každého senzoru správce klikne na **Přiřadit k místnosti** a vybere příslušnou místnost z digitálního pasportu budovy (např. `Učebna 2.A`).

### Krok 5: Co se stane v systému po přiřazení (`SimulationHandover`)
Jakmile je kanál přiřazen:
* Třída `SimulationHandover.TakeOverAsync` automaticky **deaktivuje matematickou simulaci** pro danou místnost a veličinu.
* Do databáze `SensorMeasurements` se začnou ukládat reálná data z fyzického čidla.
* V energetickém auditu budovy a v 3D řezu získá místnost zelený odznak **ŽIVĚ**.
* Pokud by čidlo někdy přestalo odpovídat (výpadek proudu nebo odpojení), mechanismus `SimulationHandover.ReleaseAsync` opět bezpečně naváže se simulací, aby grafy a výpočty nezůstaly prázdné.

---

## 7. K čemu je Python v repozitáři a v Dockeru?

V projektu SmartAudit KHK je architektura rozdělena na produkční technologie a vývojové/testovací nástroje. Častý dotaz bývá: *„K čemu tam je ten Python a proč je v Dockeru, když backend je v C# .NETu?“*

Zde je přesné a jasné vysvětlení:

### 1. Hardwarový emulátor IQRF brány (`tools/iqrf-emulator/emulator.py`)
* **Problém:** Skutečná IQRF brána a čidla jsou fyzický hardware za tisíce korun. Vývojář doma, tester v CI pipeline ani hodnotící porotce na hackathonu nemají u počítače vždy zapojenou skutečnou anténu a krabičku s čidlem CO₂.
* **Řešení:** Vytvořili jsme lehký emulátor v Pythonu. Tento skript:
  1. Připojí se k MQTT brokeru (Mosquitto).
  2. Poslouchá na topicu `gateway/{id}/iqrf/requests`.
  3. Simuluje chování koordinátoru i 2 uzlů (Uzel 1: učebna – CO₂, teplota, vlhkost; Uzel 2: chodba – teplota, světlo, pohyb).
  4. Simuluje reálnou fyziku třídy: jak děti dýchají, hladina CO₂ plynule stoupá z 600 na 2 000 ppm a po vyvětrání prudce klesne.
  5. Odpovídá ve 100% shodném formátu IQRF JSON API jako skutečný hardwarový daemon.

### 2. Proč spustit Python emulátor v Dockeru?
* Aby uživatel nemusel na svém počítači instalovat Python, pip a knihovnu `paho-mqtt`.
* Jednoduchým příkazem v Dockeru lze spustit celý ekosystém včetně simulovaného hardwaru:
  ```bash
  # Spuštění lokálního emulátoru v Pythonu (heslo gatewaye je na jejím detailu v aplikaci):
  python tools/iqrf-emulator/emulator.py --gateway ucebna-1 --password <heslo gatewaye>
  # Šifrovaně přes síť, s ověřením certifikátu brokeru:
  python tools/iqrf-emulator/emulator.py --host <IP> --port 8883 --tls --cafile smartaudit-ca.crt --gateway ucebna-1 --password <heslo>
  ```
  Nebo v Docker kontejneru (gateway `ucebna-1` musí být v aplikaci připojená):
  ```bash
  docker compose --profile emulator up emulator
  ```

### 3. Skripty pro analýzu a přípravu otevřených dat (`research/`)
V repozitáři jsou další Python skripty ve složkách `research/analyza/` a `research/kontext/`:
* `crawl.py`, `pull.py`: Jednorázové stažení otevřených datových sad z krajského portálu Data KHK.
* `krovak.py`, `souradnice.py`: Matematický přepočet geodetických souřadnic z Křovákova zobrazení (S-JTSK používané českými úřady) do globálního GPS WGS-84 pro uložení do PostgreSQL / PostGIS.
* `gravity.py`, `doprava_shluky.py`: Analýzy shluků a energeticko-dopravní modely pro analytickou část aplikace.

### Shrnutí role technologií:
* **Produkční server HK (provoz):** Běží výhradně v Dockeru na technologiích **.NET 9 C# (API)**, **PostgreSQL 17 / PostGIS (DB)**, **Eclipse Mosquitto (MQTT)** a **Next.js 15 (Web UI)**. Žádný Python pro běh produkce není vyžadován!
* **Python role:** Slouží výhradně jako **emulátor hardwaru pro vývojáře a demo ukázky** a jako **datová příprava (ETL / GIS)**.

---

## 8. Bezpečnost MQTT

Broker (`mqtt/`) nepouští nikoho bez jména a hesla a každá gateway smí jen na své topicy:
1. **Účet pro každou gateway (Dynamic Security):** jakmile správce gateway připojí v aplikaci, backend jí na brokeru založí účet
   a roli s právy jen na `gateway/<její-id>/iqrf/requests` (odběr) a `gateway/<její-id>/iqrf/responses` (zápis).
   Cizí gateway ani nikdo v síti školy tak nepřečte telemetrii jiné budovy a nepodvrhne měření. Odpojená gateway o účet přijde.
2. **Heslo gatewaye se neukládá:** je to HMAC-SHA256 tajemství `MQTT_DEVICE_SECRET` a id gatewaye — broker ho zná,
   aplikace ho kdykoli znovu ukáže jen kraji a správci té budovy.
3. **TLS na portu 8883:** broker si při prvním startu vytvoří vlastní certifikační autoritu a certifikát serveru
   (adresy z `IQRF_BROKER_PUBLIC_HOST`). Gateway ověřuje server certifikátem `smartaudit-ca.crt`, který jde stáhnout v aplikaci.
   Pro ostrý provoz lze certifikát nahradit krajským (svazek `mqtt_certs`) a zapnout vzájemné ověření certifikátem (`require_certificate`).
4. **Nešifrovaný port 1883** je jen pro backend uvnitř sítě Dockeru a nástroje na serveru (`127.0.0.1`), ne pro síť školy.
5. **HTTP Webhook Fallback:** Pro zabezpečené sítě, kde je blokován odchozí MQTT port, aplikace nabízí zabezpečený REST endpoint chráněný JWT tokenem role Kraj/Správce:
   ```http
   POST /api/sensors/iqrf/{gatewayId}/messages
   Authorization: Bearer <jwt-token>
   Content-Type: application/json
   ```

---

## 9. Rychlé shrnutí pro prezentaci před porotou (Ing. Jan Vašta)

Při prezentaci projektu se doporučuje ukázat tento 3krokový scénář:
1. **Zobrazení stavu:** Otevřít obrazovku **Senzory**, ukázat stav připojeného MQTT brokeru.
2. **Připojení živého čidla:** Spustit emulátor (nebo připojit skutečnou IQRF krabičku zapojenou v sále). Aplikace detekuje novou gateway, vyčte uzly přes IQRF Standard Sensor a zobrazí živou teplotu a CO₂.
3. **Přiřazení do budovy:** Přiřadit senzor do učebny gymnázia. Ukázat, že se simulace okamžitě přepnula na skutečná data, budova získala odznak **ŽIVĚ** a v detailu objektu se přepočítal energetický audit i vyčíslená úspora v Kč.

---

## 10. Srovnávací analýza: Jsou IQRF senzory cenově a funkčně nejvýhodnější?

Při výběru technologie pro školy a veřejné budovy se často objevuje otázka: *„Proč kupovat čidla za 6 000 Kč s IQRF, když na internetu lze koupit Wi-Fi čidlo za 1 000 Kč nebo čidlo se SIM kartou?“*

Pohled na **čistou nákupní cenu samotné plastové krabičky** je v prostředí veřejných budov zavádějící. Rozhodující je **TCO (Total Cost of Ownership — celkové náklady za 5 až 10 let provozu)**, legislativní shoda, provozní spolehlivost a požadavky školních správců IT.

### Srovnávací matice IoT technologií pro školní budovy

| Kritérium | **IQRF (MICRORISC / Protronix)** | **Levná Wi-Fi / Tuya (Čína)** | **LoRaWAN** | **NB-IoT / LTE-M (SIM)** | **Drátové (Modbus / KNX)** |
|---|---|---|---|---|---|
| **Pořizovací cena čidla** | 5 500 – 7 000 Kč | 800 – 1 500 Kč | 5 000 – 7 500 Kč | 4 500 – 6 500 Kč | 3 500 – 5 000 Kč |
| **Měsíční paušály operátorům** | **0 Kč** (vlastní síť) | 0 Kč | 0 Kč (vlastní GW) | **30 – 50 Kč / SIM / měsíc** | **0 Kč** |
| **Náklady na instalaci a kabeláž** | **Nulové** (montáž za 15 min) | Nulové | Nulové | Nulové | **20 000 – 50 000 Kč / třída** (sekání, protipožární ucpávky) |
| **Typ měření CO₂** | **Skutečný NDIR článek** | Většinou nekvalitní eCO₂ | Skutečný NDIR | Skutečný NDIR | Skutečný NDIR |
| **Prostupnost masivními zdmi** | **Vynikající** (868 MHz Mesh) | **Špatná** (2.4 GHz útlum) | Velmi dobrá (868 MHz) | Velmi dobrá (LTE) | 100% (kabel) |
| **Zatížení školní Wi-Fi sítě** | **Žádné** (izolované pásmo) | **Kritické** (zahltí AP) | Žádné | Žádné | Žádné |
| **Závislost na cloudovém vendorovi** | **Žádná** (otevřené MQTT) | Vysoká (čínské cloudy) | Žádná (otevřené LNS) | Střední | Žádná |
| **Legislativa a revize pro školy** | **Plná certifikace CE/ČSN** | Často chybí revize | Plná certifikace | Plná certifikace | Plná certifikace |
| **TCO na 1 školu (20 tříd, 10 let)** | **cca 130 000 Kč** | *Nefunkční / neschváleno* | cca 140 000 Kč | **cca 230 000 Kč** (+100 tis. SIM!) | **500 000 – 1 000 000 Kč** |

---

### Proč jsou IQRF senzory funkčně a ekonomicky nejlepší volbou?

#### 1. Proč NE levná Wi-Fi / Tuya čidla za 1 000 Kč?
* **Falešné měření CO₂ (eCO₂ vs. NDIR):** Levná čidla nemají drahý optický NDIR článek, ale jen laciný polovodič na vodík a těkavé látky. Jakmile se ve třídě použije lihová fixa na tabuli, dezinfekce na ruce nebo parfém, levné čidlo vylétne na 3 000 ppm, ačkoliv reálné CO₂ je v normě. Ve SmartAuditu by taková čidla generovala falešné poplachy.
* **Noční můra správce IT:** Školní Wi-Fi je dimenzovaná na mobily studentů a notebooky učitelů. Správci škol striktně odmítají mít ve své Wi-Fi desítky neznámých IoT krabiček s neaktualizovaným firmwarem. Změna hesla na Wi-Fi navíc znamená fyzicky obejít a znovu napárovat každé čidlo.
* **Revize elektroinstalace:** Do veřejných škol nelze na 230 V namontovat krabičku z čínského e-shopu bez protokolu o shodě a revizní zprávy pro veřejné prostory.

#### 2. Proč NE SIM karty (NB-IoT)?
* Senzory se SIM kartou jsou skvělé na osamělou meteorologickou stanici v lese.
* V budově školy s 20 třídami je však 20 SIM karet ekonomický nesmysl:
  * 20 SIM × 40 Kč měsíčně = 800 Kč/měsíc = **9 600 Kč ročně na jednu školu**.
  * Pro 50 krajských škol to znamená **480 000 Kč ročně vyhozených operátorům za SIM karty**.
  * Za 10 let provozu kraj zaplatí **téměř 5 milionů Kč jen na poplatcích operátorům**!
  * IQRF má **0 Kč měsíčních poplatků**, protože data z celé školy jdou přes jedinou lokální bránu.

#### 3. Proč NE drátové řešení (Modbus / KNX / BACnet)?
* Drátová čidla jsou spolehlivá, ale instalace kabelových tras v historických školních budovách (kamenné a cihlové zdi tloušťky 60–90 cm, památková ochrana, požární ucpávky mezi patry) stojí **stovky tisíc až miliony korun za stavební práce**.
* IQRF se namontuje dvěma šroubky do zdi během 15 minut na třídu bez jediného záseku do omítky.

#### 4. Proč IQRF vítězí nad LoRaWANem a Zigbee ve vnitřních prostorách?
* **Zigbee běží na 2.4 GHz:** Ve škole plné stovek mobilních telefonů s aktivní Wi-Fi a Bluetooth dochází v pásmu 2.4 GHz k vysokému zarušení a ztrátovosti paketů. Signál 2.4 GHz navíc špatně prochází tlustými nosnými zdmi.
* **LoRaWAN nemá uvnitř budovy mesh:** Standardní LoRaWAN funguje v topologii „hvězda“ – každé čidlo musí dosáhnout přímo na bránu. V členité školní budově se suterénem, tělocvičnou a zadními křídly signál z jedné vnitřní brány často neprojde.
* **IQRF spojuje výhody obou:** Běží na čistém a vysoce prostupném pásmu **868 MHz** a zároveň využívá **IQMESH**, takže čidla v patrech fungují jako retranslátory a spolehlivě propojí celou budovu bez hluchých míst.

### Závěrečný verdikt:
**Ano, pro krajské školy je IQRF ve spojení s kvalitními českými senzory (např. Protronix) funkčně i nákladově nejvýhodnějším řešením.** Má nejnižší celkové náklady TCO, nulové měsíční poplatky, plnou legislativní shodu a je vyvíjena přímo v Královéhradeckém kraji (MICRORISC v Jičíně), což garantuje přímou dostupnost servisu a podporu lokální ekonomiky.
