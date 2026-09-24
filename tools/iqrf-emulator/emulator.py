"""Emulátor IQRF Gateway Daemonu pro demo SmartAuditu bez hardwaru.

Chová se jako gateway s koordinátorem a dvěma uzly se senzory (IQRF Standard Sensor):
odebírá gateway/{id}/iqrf/requests a na dotazy aplikace odpovídá do gateway/{id}/iqrf/responses
stejným JSON formátem jako skutečný daemon (IQRF JSON API).

    pip install -r tools/iqrf-emulator/requirements.txt
    python tools/iqrf-emulator/emulator.py --gateway ucebna-1 --password <heslo gatewaye>

Broker nepouští anonymy: jméno = id gatewaye, heslo ukáže aplikace u gatewaye (Senzory → detail).
Místo hesla lze zadat --device-secret (MQTT_DEVICE_SECRET z .env) — emulátor si heslo odvodí stejně jako aplikace.
Přes síť šifrovaně: --port 8883 --tls --cafile smartaudit-ca.crt (certifikát CA ke stažení v aplikaci).

Uzel 1 — učebna: teplota, CO₂, vlhkost. Uzel 2 — chodba: teplota, osvětlení, pohyb (PIR).
CO₂ v učebně roste a po „vyvětrání“ spadne, aby graf v aplikaci žil.
"""

import argparse
import base64
import hashlib
import hmac
import json
import math
import os
import random
import ssl
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
    elif m_type == "iqrfSensor_Frc":
        # Jedna veličina ze všech uzlů: pole indexované adresou uzlu (0 = koordinátor), uzel bez senzoru = null
        param = req.get("param", {})
        sensor_type, sensor_index = param.get("sensorType"), param.get("sensorIndex", 0)
        sensors = [None] * (max(NODES) + 1)
        for node, node_sensors in NODES.items():
            of_type = [s for s in node_sensors if s[0] == sensor_type]
            if sensor_index < len(of_type):
                stype, sid, name, unit = of_type[sensor_index]
                sensors[node] = {"id": sid, "type": stype, "name": name, "shortName": sid[:3], "value": room.value(node, stype), "unit": unit, "decimalPlaces": 2}
        result = {"sensors": sensors}
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


def gateway_password(device_secret, gateway_id):
    """Stejné odvození jako GatewayCredentials v aplikaci: HMAC-SHA256, base64url, 24 znaků."""
    digest = hmac.new(device_secret.encode("utf-8"), f"mqtt:{gateway_id}".encode("utf-8"), hashlib.sha256).digest()
    return base64.urlsafe_b64encode(digest).decode("ascii").rstrip("=")[:24]


def main():
    parser = argparse.ArgumentParser(description="Emulátor IQRF Gateway Daemonu (MQTT)")
    parser.add_argument("--host", default="localhost", help="MQTT broker (výchozí localhost)")
    parser.add_argument("--port", type=int, default=1883, help="1883 = na tomto počítači, 8883 = TLS přes síť")
    parser.add_argument("--gateway", default="ucebna-1", help="Id gatewaye v topicu gateway/{id}/iqrf/... (musí být připojená v aplikaci)")
    parser.add_argument("--password", help="Heslo gatewaye z aplikace (jméno = id gatewaye)")
    parser.add_argument("--device-secret", default=os.environ.get("MQTT_DEVICE_SECRET"), help="Místo hesla: MQTT_DEVICE_SECRET, heslo se odvodí")
    parser.add_argument("--tls", action="store_true", help="Šifrované spojení (port 8883)")
    parser.add_argument("--cafile", help="Certifikát CA brokeru pro ověření serveru (smartaudit-ca.crt)")
    args = parser.parse_args()

    password = args.password or (gateway_password(args.device_secret, args.gateway) if args.device_secret else None)
    if not password:
        parser.error("zadejte --password (z aplikace) nebo --device-secret")

    request_topic = f"gateway/{args.gateway}/iqrf/requests"
    response_topic = f"gateway/{args.gateway}/iqrf/responses"
    room = Room()

    def on_connect(client, _userdata, _flags, reason_code, _properties):
        if reason_code.is_failure:
            print(f"Broker odmítl připojení: {reason_code} — je gateway „{args.gateway}“ připojená v aplikaci a sedí heslo?")
            return
        print(f"Připojeno k {args.host}:{args.port} ({reason_code}), poslouchám {request_topic}")
        client.subscribe(request_topic)

    def on_message(client, _userdata, message):
        try:
            request = json.loads(message.payload.decode("utf-8"))
        except json.JSONDecodeError:
            return
        reply = response(request, room)
        client.publish(response_topic, json.dumps(reply, ensure_ascii=False))
        req = request.get("data", {}).get("req", {})
        target = f"typ {req.get('param', {}).get('sensorType')} ze všech uzlů" if request.get("mType") == "iqrfSensor_Frc" else f"uzel {req.get('nAdr')}"
        print(f"← {request.get('mType')} {target}  → odpověď {reply['data']['statusStr']}")

    client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=f"iqrf-emulator-{args.gateway}")
    client.username_pw_set(args.gateway, password)
    if args.tls:
        client.tls_set(ca_certs=args.cafile, tls_version=ssl.PROTOCOL_TLS_CLIENT)
    client.on_connect = on_connect
    client.on_message = on_message
    client.connect(args.host, args.port)
    client.loop_forever()


if __name__ == "__main__":
    main()
