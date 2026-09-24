"""Cluster pěti emulovaných IQRF gatewayí v pěti okresech Královéhradeckého kraje (jeden proces, pět MQTT spojení).

Stejné rozložení jako testovací síť v aplikaci (VirtualGateways.cs): budovy, uzly, veličiny. Gatewaye musí být
v aplikaci připojené (Senzory → Spustit testovací síť kraje), jinak je broker nepustí. Heslo každé gatewaye se odvodí
z MQTT_DEVICE_SECRET stejně jako v aplikaci. Když cluster běží, virtuální gatewaye v aplikaci samy ustoupí.

    pip install -r tools/iqrf-emulator/requirements.txt
    python tools/iqrf-emulator/multi_emulator.py --device-secret <MQTT_DEVICE_SECRET>

    # nebo v Dockeru
    docker compose --profile emulators up -d
"""

import argparse
import json
import math
import os
import ssl
import time
import zlib
from datetime import datetime
from zoneinfo import ZoneInfo

import paho.mqtt.client as mqtt

from emulator import envelope, gateway_password

LOCAL = ZoneInfo("Europe/Prague")

TEMPERATURE = (1, "TEMPERATURE", "Temperature", "°C")
CO2 = (2, "CARBON_DIOXIDE", "Carbon dioxide", "ppm")
VOC = (3, "VOLATILE_ORGANIC_COMPOUND", "Volatile organic compounds", "ppm")
ILLUMINANCE = (11, "ILLUMINANCE", "Illuminance", "lx")
NOISE = (19, "SOUND_PRESSURE_LEVEL", "Sound pressure level", "dB")
HUMIDITY = (128, "RELATIVE_HUMIDITY", "Relative humidity", "%")
MOTION = (129, "BINARYDATA7", "Binary data7", "")

# gateway → (profil provozu, {uzel: [senzory]})
NETWORK = {
    "gw-hk-tyl": ("school", {1: [TEMPERATURE, CO2, HUMIDITY], 2: [TEMPERATURE, ILLUMINANCE, MOTION]}),
    "gw-jc-moa": ("school", {1: [CO2, TEMPERATURE], 2: [NOISE, MOTION]}),
    "gw-tu-gym": ("school", {1: [VOC, CO2], 2: [TEMPERATURE, HUMIDITY]}),
    "gw-na-jir": ("school", {1: [HUMIDITY, TEMPERATURE, ILLUMINANCE], 2: [CO2, TEMPERATURE]}),
    "gw-rk-sps": ("workshop", {1: [TEMPERATURE, NOISE], 2: [TEMPERATURE, CO2, HUMIDITY]}),
}


def value(gateway, profile, node, sensor_type, now):
    """Průběhy podle provozu: vyučovací hodiny a přestávky (CO₂), směny v dílně (hluk), noční útlum (teplota)."""
    local = datetime.fromtimestamp(now, LOCAL)
    phase = zlib.crc32(f"{gateway}/{node}".encode()) % 1000 / 1000 * 2 * math.pi
    minutes = local.hour * 60 + local.minute + local.second / 60
    workday = local.weekday() < 5
    occupied = workday and 7 <= local.hour < 16
    noise = math.sin(now / 37 + phase) * 0.5 + math.sin(now / 11 + phase * 2) * 0.3
    lesson_minute = (minutes - 8 * 60) % 55
    in_lesson = occupied and lesson_minute < 45
    shift = profile == "workshop" and workday and (6 <= local.hour < 11 or 12 <= local.hour < 15)

    if sensor_type == 1:
        return round((21.2 if occupied else 18.4) + 0.6 * math.sin(minutes / 180 + phase) + 0.15 * noise, 2)
    if sensor_type == 2:
        return round((620 + lesson_minute * 24 if in_lesson else 560) + 40 * noise if occupied else 430 + 20 * noise)
    if sensor_type == 3:
        return round((0.45 if occupied else 0.12) + 0.08 * noise, 2)
    if sensor_type == 11:
        return round(max(0, (380 if occupied else 20) + 60 * math.sin(minutes / 90 + phase) + 10 * noise))
    if sensor_type == 19:
        return round((78 if shift else 55 if occupied else 32) + 4 * noise, 1)
    if sensor_type == 128:
        return round(43 + 4 * math.sin(minutes / 240 + phase) + noise, 1)
    if sensor_type == 129:
        return 1 if occupied and noise > -0.2 else 0
    return 0


def sensor_json(gateway, profile, node, sensor, now):
    stype, sid, name, unit = sensor
    return {"id": sid, "type": stype, "name": name, "shortName": sid[:3], "value": value(gateway, profile, node, stype, now), "unit": unit, "decimalPlaces": 2}


def respond(gateway, request):
    profile, nodes = NETWORK[gateway]
    m_type = request.get("mType")
    data = request.get("data", {})
    msg_id = data.get("msgId", "")
    req = data.get("req", {})
    n_adr = req.get("nAdr", 0)
    now = time.time()

    if m_type == "iqrfEmbedCoordinator_BondedDevices":
        return envelope(m_type, msg_id, 0, {"bondedDevices": sorted(nodes)})
    if m_type == "iqrfSensor_ReadSensorsWithTypes":
        if n_adr not in nodes:
            return envelope(m_type, msg_id, n_adr, None, status=1002, status_str="Timeout — uzel neodpověděl")
        return envelope(m_type, msg_id, n_adr, {"sensors": [sensor_json(gateway, profile, n_adr, s, now) for s in nodes[n_adr]]})
    if m_type == "iqrfSensor_Frc":
        param = req.get("param", {})
        sensor_type, index = param.get("sensorType"), param.get("sensorIndex", 0)
        sensors = [None] * (max(nodes) + 1)
        for node, node_sensors in nodes.items():
            of_type = [s for s in node_sensors if s[0] == sensor_type]
            if index < len(of_type):
                sensors[node] = sensor_json(gateway, profile, node, of_type[index], now)
        return envelope(m_type, msg_id, 0, {"sensors": sensors})
    return envelope(m_type, msg_id, n_adr, None, status=-1, status_str="Emulátor tento typ zprávy nepodporuje")


def start_gateway(gateway, args):
    request_topic = f"gateway/{gateway}/iqrf/requests"
    response_topic = f"gateway/{gateway}/iqrf/responses"

    def on_connect(client, _userdata, _flags, reason_code, _properties):
        if reason_code.is_failure:
            print(f"[{gateway}] broker odmítl připojení ({reason_code}) — je gateway v aplikaci připojená? Zkusím znovu.")
            return
        print(f"[{gateway}] připojeno, poslouchám {request_topic}")
        client.subscribe(request_topic)

    def on_message(client, _userdata, message):
        try:
            request = json.loads(message.payload.decode("utf-8"))
        except json.JSONDecodeError:
            return
        reply = respond(gateway, request)
        reply["data"]["insId"] = "iqrfgd2-multi-emulator"
        client.publish(response_topic, json.dumps(reply, ensure_ascii=False), qos=1)

    client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=f"multi-emulator-{gateway}")
    client.username_pw_set(gateway, gateway_password(args.device_secret, gateway))
    if args.tls:
        client.tls_set(ca_certs=args.cafile, tls_version=ssl.PROTOCOL_TLS_CLIENT)
    client.on_connect = on_connect
    client.on_message = on_message
    # Broker pustí gateway až po jejím připojení v aplikaci — do té doby se připojení opakuje
    client.reconnect_delay_set(min_delay=5, max_delay=30)
    client.connect_async(args.host, args.port)
    client.loop_start()
    return client


def main():
    parser = argparse.ArgumentParser(description="Cluster pěti emulovaných IQRF gatewayí (testovací síť kraje)")
    parser.add_argument("--host", default="localhost")
    parser.add_argument("--port", type=int, default=1883)
    parser.add_argument("--device-secret", default=os.environ.get("MQTT_DEVICE_SECRET"), help="MQTT_DEVICE_SECRET z .env")
    parser.add_argument("--tls", action="store_true")
    parser.add_argument("--cafile")
    args = parser.parse_args()
    if not args.device_secret:
        parser.error("zadejte --device-secret nebo nastavte MQTT_DEVICE_SECRET")

    clients = [start_gateway(gateway, args) for gateway in NETWORK]
    print(f"Běží {len(clients)} emulovaných gatewayí: {', '.join(NETWORK)}")
    try:
        while True:
            time.sleep(3600)
    except KeyboardInterrupt:
        for client in clients:
            client.loop_stop()
            client.disconnect()


if __name__ == "__main__":
    main()
