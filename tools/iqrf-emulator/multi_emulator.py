"""Cluster pěti emulovaných IQRF gatewayí v pěti okresech Královéhradeckého kraje (jeden proces, pět MQTT spojení).

Stejné rozložení jako testovací síť v aplikaci (VirtualGateways.cs): budovy, uzly, veličiny. Gatewaye musí být
v aplikaci připojené (Senzory → Spustit testovací síť kraje), jinak je broker nepustí. Heslo každé gatewaye se odvodí
z MQTT_DEVICE_SECRET stejně jako v aplikaci. Když cluster běží, virtuální gatewaye v aplikaci samy ustoupí.

Navíc zařízení, která jsou zapnutá a online, ale nejsou v žádné síti (katalog PairableDevices.cs v aplikaci):
- dvě nové gatewaye z výroby — v aplikaci se objeví mezi „Nová zařízení online“ a připojí se heslem z nálepky,
- čidla v dosahu gatewayí gw-rk-sps a gw-hk-tyl — do sítě gatewaye se přidají kódem z nálepky (IQRF SmartConnect).
Nálepky (serial, heslo, kódy) vypíše konzole po startu. Párování potřebuje MQTT_PROVISIONING_PASSWORD.

    pip install -r tools/iqrf-emulator/requirements.txt
    python tools/iqrf-emulator/multi_emulator.py --device-secret <MQTT_DEVICE_SECRET> --provisioning-password <MQTT_PROVISIONING_PASSWORD>

    # nebo v Dockeru
    docker compose --profile emulators up -d
"""

import argparse
import json
import math
import os
import ssl
import threading
import time
import zlib
from datetime import datetime
from zoneinfo import ZoneInfo

import paho.mqtt.client as mqtt

import pairing
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

# Nové gatewaye z výroby: serial (MID) → (model, heslo z nálepky, {uzel: [senzory]})
PAIRABLE = {
    "81A4F2C0": ("IQD-GW04", "7Q4M-2PXA", {1: [TEMPERATURE, CO2, HUMIDITY], 2: [TEMPERATURE, NOISE]}),
    "81A4F3D9": ("IQD-GW04", "H3TR-9KLC", {1: [TEMPERATURE, HUMIDITY, ILLUMINANCE]}),
}

# Čidla v dosahu gatewaye mimo její síť: gateway → [(MID, kód z nálepky, adresa po přidání, [senzory])]
NEARBY = {
    "gw-rk-sps": [("5C0E91A7", "RK2B-X8QD", 3, [CO2, TEMPERATURE])],
    "gw-hk-tyl": [("5C0E92F3", "HK7N-4WZE", 3, [NOISE])],
}

# Čidla přidaná do sítě kódem: gateway → {adresy}; ukládá se do souboru, aby po restartu zůstala v síti
BONDED = {}
BONDED_LOCK = threading.Lock()
STATE_FILE = None

# Kolikrát po sobě broker gatewayi odmítne, než usoudíme, že ji v aplikaci odebrali (výpadek brokeru se nepočítá)
REJECTIONS_BEFORE_RESET = 3


def stable_hash(value):
    """FNV-1a jako StableHash v aplikaci — stejná „síla signálu“ ukáže aplikace i emulátor."""
    h = 2166136261
    for b in value.encode("utf-8"):
        h = ((h ^ b) * 16777619) & 0xFFFFFFFF
    return h - (1 << 32) if h >= 1 << 31 else h


def rssi_of(value, base, steps, step):
    return base - abs(int(math.fmod(stable_hash(value), steps))) * step


def nodes_of(gateway):
    """Uzly v síti gatewaye: základní podle katalogu + čidla z dosahu, která uživatel přidal kódem."""
    if gateway in NETWORK:
        nodes = dict(NETWORK[gateway][1])
    else:
        nodes = dict(next(p[2] for serial, p in PAIRABLE.items() if pairing.gateway_id_of(serial) == gateway))
    with BONDED_LOCK:
        bonded = set(BONDED.get(gateway, ()))
    for _mid, _code, address, sensors in NEARBY.get(gateway, []):
        if address in bonded:
            nodes[address] = sensors
    return nodes


def profile_of(gateway):
    return NETWORK[gateway][0] if gateway in NETWORK else "school"


def bond(gateway, address):
    with BONDED_LOCK:
        BONDED.setdefault(gateway, set()).add(address)
        snapshot = {g: sorted(a) for g, a in BONDED.items()}
    if STATE_FILE:
        with open(STATE_FILE, "w", encoding="utf-8") as f:
            json.dump(snapshot, f)


def load_state(path):
    if not path or not os.path.exists(path):
        return
    try:
        with open(path, encoding="utf-8") as f:
            for gateway, addresses in json.load(f).items():
                BONDED[gateway] = set(addresses)
    except (OSError, ValueError):
        print(f"Stav {path} nejde načíst — začínám z výroby")


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
    profile, nodes = profile_of(gateway), nodes_of(gateway)
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
    if m_type == "iqmeshNetwork_SmartConnect":
        # Kód z nálepky čidla v dosahu → čidlo dostane adresu v síti (už přidané odpoví stejnou adresou)
        code = pairing.normalize(req.get("smartConnectCode") or "")
        match = next((n for n in NEARBY.get(gateway, []) if pairing.normalize(n[1]) == code), None)
        if match is None:
            return envelope(m_type, msg_id, 0, None, status=1003, status_str="Bonding se nezdařil — čidlo s tímto kódem neodpovědělo")
        mid, _code, address, _sensors = match
        bond(gateway, address)
        print(f"[{gateway}] čidlo MID {mid} přidáno do sítě jako uzel {address}")
        reply = envelope(m_type, msg_id, 0, None)
        reply["data"]["rsp"].update({"assignedAddr": address, "nodesNr": len(nodes_of(gateway))})
        return reply
    if m_type == "smartaudit_NearbyNodes":
        nearby = [
            {"mid": mid, "sensors": [s[0] for s in sensors], "rssi": rssi_of(mid, -60, 3, 7)}
            for mid, _code, address, sensors in NEARBY.get(gateway, [])
            if address not in nodes
        ]
        return envelope(m_type, msg_id, 0, {"nodes": nearby})
    return envelope(m_type, msg_id, n_adr, None, status=-1, status_str="Emulátor tento typ zprávy nepodporuje")


def configure(client, args):
    if args.tls:
        client.tls_set(ca_certs=args.cafile, tls_version=ssl.PROTOCOL_TLS_CLIENT)
    client.reconnect_delay_set(min_delay=5, max_delay=30)


def rejected(reason_code):
    """Broker nepustil kvůli jménu nebo heslu (účet v aplikaci neexistuje) — ne kvůli výpadku."""
    return reason_code.value in (134, 135)


def start_gateway(gateway, args, username=None, password=None, on_connected=None, on_rejected=None):
    request_topic = f"gateway/{gateway}/iqrf/requests"
    response_topic = f"gateway/{gateway}/iqrf/responses"

    def on_connect(client, _userdata, _flags, reason_code, _properties):
        if reason_code.is_failure:
            if on_rejected and rejected(reason_code):
                on_rejected()
            else:
                print(f"[{gateway}] broker odmítl připojení ({reason_code}) — je gateway v aplikaci připojená? Zkusím znovu.")
            return
        print(f"[{gateway}] připojeno, poslouchám {request_topic}")
        client.subscribe(request_topic)
        if on_connected:
            on_connected()

    def on_message(client, _userdata, message):
        try:
            request = json.loads(message.payload.decode("utf-8"))
        except json.JSONDecodeError:
            return
        reply = respond(gateway, request)
        reply["data"]["insId"] = "iqrfgd2-multi-emulator"
        client.publish(response_topic, json.dumps(reply, ensure_ascii=False), qos=1)

    client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=f"multi-emulator-{gateway}")
    client.username_pw_set(username or gateway, password or gateway_password(args.device_secret, gateway))
    configure(client, args)
    client.on_connect = on_connect
    client.on_message = on_message
    # Broker pustí gateway až po jejím připojení v aplikaci — do té doby se připojení opakuje
    client.connect_async(args.host, args.port)
    client.loop_start()
    return client


class PairableGateway:
    """Gateway z výroby. Po startu zkusí svůj účet (už spárovaná z dřívějška); když ho broker nezná, ohlašuje se
    a čeká na nabídku. Po odebrání v aplikaci (broker ji opakovaně odmítne) se vrátí do párování."""

    def __init__(self, serial, args):
        self.serial = serial
        self.model, self.password, self.nodes = PAIRABLE[serial]
        self.gateway_id = pairing.gateway_id_of(serial)
        self.args = args
        self.client = None
        self.mode = None
        self.rejections = 0
        self.lock = threading.Lock()
        self.rssi = rssi_of(serial, -52, 4, 6)

    def start(self):
        self.to_gateway(self.gateway_id, gateway_password(self.args.device_secret, self.gateway_id), probing=True)

    def stop(self):
        with self.lock:
            self.mode = "stopped"
            self._close()

    def _close(self):
        if self.client:
            self.client.disconnect()
            self.client.loop_stop()
            self.client = None

    def to_gateway(self, username, password, probing=False):
        with self.lock:
            if self.mode == "stopped":
                return
            self._close()
            self.mode = "gateway"
            self.rejections = 0
            # Při startu stačí jedno odmítnutí (gateway ještě nikdo nespároval); po spárování chvíli trvá, než aplikace založí účet
            limit = 1 if probing else REJECTIONS_BEFORE_RESET

            def on_connected():
                self.rejections = 0

            def on_rejected():
                self.rejections += 1
                if self.rejections >= limit:
                    if not probing:
                        print(f"[{self.serial}] gateway byla v aplikaci odebrána — zpět do režimu párování")
                    threading.Thread(target=self.to_pairing, daemon=True).start()

            self.client = start_gateway(self.gateway_id, self.args, username, password, on_connected, on_rejected)

    def to_pairing(self):
        with self.lock:
            if self.mode == "stopped":
                return
            self._close()
            self.mode = "pairing"

            def on_connect(client, _userdata, _flags, reason_code, _properties):
                if reason_code.is_failure:
                    print(f"[{self.serial}] párovací účet odmítnut ({reason_code}) — sedí MQTT_PROVISIONING_PASSWORD?")
                    return
                client.subscribe(pairing.offer_topic(self.serial))
                print(f"[{self.serial}] online, čekám na spárování (heslo z nálepky {self.password})")

            def on_message(client, _userdata, message):
                threading.Thread(target=self.answer_offer, args=(client, message.payload.decode("utf-8")), daemon=True).start()

            client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=f"pairing-{self.serial}")
            client.username_pw_set(pairing.ACCOUNT, self.args.provisioning_password)
            configure(client, self.args)
            client.on_connect = on_connect
            client.on_message = on_message
            client.connect_async(self.args.host, self.args.port)
            client.loop_start()
            self.client = client

        threading.Thread(target=self.announce_loop, args=(client,), daemon=True).start()

    def announce_loop(self, client):
        types = {address: [s[0] for s in sensors] for address, sensors in self.nodes.items()}
        while self.client is client and self.mode == "pairing":
            if client.is_connected():
                client.publish(pairing.announce_topic(self.serial), pairing.announcement(self.serial, self.model, types, self.rssi))
            time.sleep(10)

    def answer_offer(self, client, offer):
        credentials = pairing.open_offer(self.password, self.serial, offer)
        if credentials is None:
            print(f"[{self.serial}] nabídka odmítnuta — špatné heslo")
            client.publish(pairing.result_topic(self.serial), pairing.result(False)).wait_for_publish(timeout=5)
            return

        print(f"[{self.serial}] heslo sedí — připojuji se jako {credentials[0]}")
        client.publish(pairing.announce_topic(self.serial), "").wait_for_publish(timeout=5)
        client.publish(pairing.result_topic(self.serial), pairing.result(True)).wait_for_publish(timeout=5)
        self.to_gateway(*credentials)


def print_labels():
    print("\nNálepky zařízení k párování:")
    for serial, (model, password, nodes) in PAIRABLE.items():
        print(f"  gateway {model}  MID {serial}  heslo {password}  ({len(nodes)} uzly)")
    for gateway, nodes in NEARBY.items():
        for mid, code, _address, sensors in nodes:
            print(f"  čidlo u {gateway}  MID {mid}  kód {code}  ({', '.join(s[1].lower() for s in sensors)})")
    print()


def main():
    global STATE_FILE

    parser = argparse.ArgumentParser(description="Cluster pěti emulovaných IQRF gatewayí (testovací síť kraje) a zařízení k párování")
    parser.add_argument("--host", default="localhost")
    parser.add_argument("--port", type=int, default=1883)
    parser.add_argument("--device-secret", default=os.environ.get("MQTT_DEVICE_SECRET"), help="MQTT_DEVICE_SECRET z .env")
    parser.add_argument("--provisioning-password", default=os.environ.get("MQTT_PROVISIONING_PASSWORD"), help="MQTT_PROVISIONING_PASSWORD z .env (párování)")
    parser.add_argument("--no-pairing", action="store_true", help="Bez nespárovaných gatewayí z výroby")
    parser.add_argument("--state", default="emulator-state.json", help="Soubor s čidly přidanými do sítě")
    parser.add_argument("--factory-reset", action="store_true", help="Zapomenout přidaná čidla (gatewaye z výroby vrátí do párování až odebrání v aplikaci)")
    parser.add_argument("--tls", action="store_true")
    parser.add_argument("--cafile")
    args = parser.parse_args()
    if not args.device_secret:
        parser.error("zadejte --device-secret nebo nastavte MQTT_DEVICE_SECRET")

    STATE_FILE = args.state
    if args.factory_reset and os.path.exists(args.state):
        os.remove(args.state)
    load_state(args.state)

    clients = [start_gateway(gateway, args) for gateway in NETWORK]
    print(f"Běží {len(clients)} emulovaných gatewayí: {', '.join(NETWORK)}")

    pairable = []
    if args.no_pairing:
        pass
    elif not args.provisioning_password:
        print("Bez MQTT_PROVISIONING_PASSWORD — gatewaye z výroby k párování nespouštím")
    else:
        pairable = [PairableGateway(serial, args) for serial in PAIRABLE]
        for gateway in pairable:
            gateway.start()
        print_labels()

    try:
        while True:
            time.sleep(3600)
    except KeyboardInterrupt:
        for gateway in pairable:
            gateway.stop()
        for client in clients:
            client.loop_stop()
            client.disconnect()


if __name__ == "__main__":
    main()
