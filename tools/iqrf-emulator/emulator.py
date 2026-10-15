"""Emulátor IQRF Gateway Daemonu pro demo SmartAuditu bez hardwaru.

Chová se jako gateway s koordinátorem a dvěma uzly se senzory (IQRF Standard Sensor):
odebírá gateway/{id}/iqrf/requests a na dotazy aplikace odpovídá do gateway/{id}/iqrf/responses
stejným JSON formátem jako skutečný daemon (IQRF JSON API).

    pip install -r tools/iqrf-emulator/requirements.txt
    python tools/iqrf-emulator/emulator.py --gateway demo-gw

Uzel 1 — učebna: teplota, CO₂, vlhkost. Uzel 2 — chodba: teplota, osvětlení, pohyb (PIR).
CO₂ v učebně roste a po „vyvětrání“ spadne, aby graf v aplikaci žil.
"""

import argparse
import json
import math
import random
import time

import paho.mqtt.client as mqtt

# (typ IQRF Standard Sensor, id, název, jednotka) — typy odpovídají IqrfSensorTypes v aplikaci
NODES = {
    1: [(1, "TEMPERATURE", "Temperature", "°C"), (2, "CARBON_DIOXIDE", "Carbon dioxide", "ppm"), (128, "RELATIVE_HUMIDITY", "Relative humidity", "%")],
    2: [(1, "TEMPERATURE", "Temperature", "°C"), (11, "ILLUMINANCE", "Illuminance", "lx"), (129, "BINARYDATA7", "Binary data7", "")],
}


class Room:
    """Jednoduchý model místnosti — pomalu se měnící hodnoty s šumem."""

    def __init__(self):
        self.started = time.time()
        self.co2 = 650.0

    def value(self, node, sensor_type):
        t = time.time() - self.started
        if sensor_type == 1:
            base = 21.4 if node == 1 else 19.2
            return round(base + 0.6 * math.sin(t / 300) + random.uniform(-0.1, 0.1), 2)
        if sensor_type == 2:
            # Každých ~5 minut se vyvětrá, mezitím CO₂ roste s dětmi ve třídě
            self.co2 = 620.0 if random.random() < 0.02 else min(2400.0, self.co2 + random.uniform(8, 22))
            return round(self.co2)
        if sensor_type == 128:
            return round(44 + 3 * math.sin(t / 420) + random.uniform(-0.5, 0.5), 1)
        if sensor_type == 11:
            return round(max(0, 320 + 80 * math.sin(t / 240) + random.uniform(-15, 15)))
        if sensor_type == 129:
            return 1 if random.random() < 0.3 else 0
        return 0


def response(request, room):
    m_type = request.get("mType")
    data = request.get("data", {})
    msg_id = data.get("msgId", "")
    req = data.get("req", {})
    n_adr = req.get("nAdr", 0)

    if m_type == "iqrfEmbedCoordinator_BondedDevices":
        result = {"bondedDevices": sorted(NODES)}
    elif m_type == "iqrfSensor_ReadSensorsWithTypes":
        if n_adr not in NODES:
            return envelope(m_type, msg_id, n_adr, None, status=1002, status_str="Timeout — uzel neodpověděl")
        result = {
            "sensors": [
                {"id": sid, "type": stype, "name": name, "shortName": sid[:3], "value": room.value(n_adr, stype), "unit": unit, "decimalPlaces": 2}
                for stype, sid, name, unit in NODES[n_adr]
            ]
        }
    else:
        return envelope(m_type, msg_id, n_adr, None, status=-1, status_str="Emulátor tento typ zprávy nepodporuje")

    return envelope(m_type, msg_id, n_adr, result)


def envelope(m_type, msg_id, n_adr, result, status=0, status_str="ok"):
    rsp = {"nAdr": n_adr, "hwpId": 0, "rCode": 0 if status == 0 else 1, "dpaVal": 64}
    if result is not None:
        rsp["result"] = result
    return {"mType": m_type, "data": {"msgId": msg_id, "rsp": rsp, "insId": "iqrfgd2-emulator", "status": status, "statusStr": status_str}}


def main():
    parser = argparse.ArgumentParser(description="Emulátor IQRF Gateway Daemonu (MQTT)")
    parser.add_argument("--host", default="localhost", help="MQTT broker (výchozí localhost)")
    parser.add_argument("--port", type=int, default=1883)
    parser.add_argument("--gateway", default="demo-gw", help="Id gatewaye v topicu gateway/{id}/iqrf/...")
    args = parser.parse_args()

    request_topic = f"gateway/{args.gateway}/iqrf/requests"
    response_topic = f"gateway/{args.gateway}/iqrf/responses"
    room = Room()

    def on_connect(client, _userdata, _flags, reason_code, _properties):
        print(f"Připojeno k {args.host}:{args.port} ({reason_code}), poslouchám {request_topic}")
        client.subscribe(request_topic)

    def on_message(client, _userdata, message):
        try:
            request = json.loads(message.payload.decode("utf-8"))
        except json.JSONDecodeError:
            return
        reply = response(request, room)
        client.publish(response_topic, json.dumps(reply, ensure_ascii=False))
        print(f"← {request.get('mType')} uzel {request.get('data', {}).get('req', {}).get('nAdr')}  → odpověď {reply['data']['statusStr']}")

    client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=f"iqrf-emulator-{args.gateway}")
    client.on_connect = on_connect
    client.on_message = on_message
    client.connect(args.host, args.port)
    client.loop_forever()


if __name__ == "__main__":
    main()
