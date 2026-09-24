# Skutečná čidla IQRF

SmartAudit umí vedle simulované telemetrie přijímat hodnoty ze skutečných senzorů
**IQRF** (MICRORISC). Gateway s IQRF Gateway Daemonem mluví přes MQTT. Aplikace sama najde
spárované uzly a jejich senzory, uživatel jen přiřadí kanál do místnosti.

```
IQRF uzly ──IQMESH──► IQRF gateway (Daemon) ──MQTT──► broker (mqtt:1883) ──► API: IqrfGatewayService
                                                                          ◄── dotazy na hodnoty
```

## Připojení za 5 minut

1. **Spusťte aplikaci.** Příkaz `docker compose up -d` spustí i broker Mosquitto na portu 1883.
2. **Nasměrujte gateway na broker.** V IQRF Gateway Webapp otevřete *Konfigurace → Messaging → MQTT* a založte novou instanci:

   | Pole | Hodnota |
   |---|---|
   | Broker | `tcp://<IP tohoto počítače>:1883` |
   | Topic pro dotazy | `gateway/<id>/iqrf/requests` |
   | Topic pro odpovědi | `gateway/<id>/iqrf/responses` |

   `<id>` je libovolné id gatewaye bez mezer a znaků `/ + #`, například `hackathon-gw`.

3. **Připojte gateway v aplikaci.** Na obrazovce **Senzory → Připojit IQRF gateway** vyberte objekt a zadejte stejné id.
   Když gateway na brokeru už odpovídá, nabídne ji aplikace sama v sekci „Ozývají se, ale nejsou připojené“.
4. **Počkejte na objevení kanálů.** Aplikace se zeptá koordinátoru na spárované uzly (`iqrfEmbedCoordinator_BondedDevices`).
   Pak se v nastaveném intervalu ptá každého uzlu na senzory (`iqrfSensor_ReadSensorsWithTypes`).
   Na detailu gatewaye se objeví kanály s živou hodnotou a průběhem.
5. **Přiřaďte kanál do místnosti.** Od té chvíle platí:
   - hodnoty se ukládají do databáze;
   - simulované čidlo té místnosti a veličiny přestane generovat data a grafy pokračují skutečným měřením;
   - audit objektu se přepočítá;
   - v inspekčním panelu je místnost označená odznakem **živě**.

Veličinu určí typ senzoru podle IQRF Standard Sensor:

| Typ | Veličina |
|---|---|
| 1 | teplota |
| 2 | CO₂ |
| 3 | VOC |
| 11 | osvětlení |
| 16 | tlak |
| 18 | PM2,5 |
| 19 | hluk |
| 128 | vlhkost |
| 129 | binární (pohyb) |

U neznámého typu vybere veličinu uživatel.

## Bez hardwaru

Emulátor se chová jako IQRF Gateway Daemon se dvěma uzly:

- uzel 1 (učebna): teplota, CO₂ a vlhkost;
- uzel 2 (chodba): teplota, osvětlení a pohyb.

```bash
pip install -r tools/iqrf-emulator/requirements.txt
python tools/iqrf-emulator/emulator.py --gateway demo-gw
```

## Záložní vstup přes HTTP

Když MQTT nejde použít, jde gatewayi poslat odpověď IQRF JSON API přes HTTP. Je to stejný JSON, jaký gateway
publikuje do topicu odpovědí, například přeposlaný z Node-RED na gatewayi. Volání vyžaduje token role Kraj nebo Správce:

```
POST /api/sensors/iqrf/{id-gatewaye}/messages
Authorization: Bearer <token>
Content-Type: application/json

{ "mType": "iqrfSensor_ReadSensorsWithTypes", "data": { "rsp": { "nAdr": 1, "result": { "sensors": [ … ] } }, "status": 0 } }
```

## Bezpečnost

Broker v `mqtt/mosquitto.conf` přijímá anonymní připojení bez TLS. Hodí se jen do uzavřené sítě
(hackathon, demo). Pro provoz doplňte `password_file`, TLS a v API `IqrfSettings__Username` / `__Password`.

## Kde to je v kódu

| Část | Soubor |
|---|---|
| Parser odpovědí IQRF JSON API, mapování typů | `backend/SmartAudit.Core/Domain/Helpers/IqrfResponseParser.cs`, `IqrfSensorTypes.cs` |
| MQTT klient, dotazování uzlů | `backend/SmartAudit.Api/Services/Sensors/IqrfGatewayService.cs` |
| Zápis měření, objevování kanálů | `backend/SmartAudit.Api/Services/Sensors/SensorIngestService.cs` |
| Předávka místnosti mezi simulací a čidlem | `backend/SmartAudit.Api/Services/Sensors/SimulationHandover.cs` |
| API | `backend/SmartAudit.Api/Controllers/SensorsController.cs` + `Features/Sensors/*` |
| Obrazovka Senzory | `frontend/app/(app)/senzory/` |
