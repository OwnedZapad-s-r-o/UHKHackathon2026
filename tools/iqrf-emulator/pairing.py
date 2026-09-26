"""Párování nové gatewaye heslem z nálepky — stejný protokol jako PairingProtocol v aplikaci.

1) Nespárovaná gateway se přihlásí sdíleným účtem „provisioning“ a každých 10 s se ohlásí na provisioning/announce/{serial}.
2) Aplikace heslem z nálepky zašifruje nabídku s přihlašovacími údaji k brokeru (AES-GCM, klíč PBKDF2-SHA256 z hesla
   a serialu, serial jako přidaná data) a pošle ji na provisioning/{serial}/offer.
3) Gateway nabídku rozšifruje svým heslem a odpoví na provisioning/{serial}/result: {"ok": true} nebo špatné heslo.
"""

import base64
import hashlib
import json
import os
import time

from cryptography.exceptions import InvalidTag
from cryptography.hazmat.primitives.ciphers.aead import AESGCM

ACCOUNT = "provisioning"
ITERATIONS = 100_000


def normalize(code):
    """Heslo z nálepky jde opsat s mezerami, bez pomlčky i malými písmeny."""
    return "".join(ch for ch in code if ch.isalnum()).upper()


def gateway_id_of(serial):
    """Id gatewaye po spárování — stejné v aplikaci (PairableDevices.GatewayIdOf)."""
    return f"iqrf-{serial.lower()}"


def announce_topic(serial):
    return f"provisioning/announce/{serial}"


def offer_topic(serial):
    return f"provisioning/{serial}/offer"


def result_topic(serial):
    return f"provisioning/{serial}/result"


def _key(password, serial):
    salt = f"smartaudit-pairing:{serial.upper()}".encode("utf-8")
    return hashlib.pbkdf2_hmac("sha256", normalize(password).encode("utf-8"), salt, ITERATIONS, 32)


def seal_offer(password, serial, username, mqtt_password, nonce=None):
    """Nabídka jako od aplikace — pro testy protokolu (pevný nonce dává stálý výsledek)."""
    plaintext = json.dumps({"gatewayId": username, "username": username, "password": mqtt_password}, separators=(",", ":")).encode("utf-8")
    nonce = nonce or os.urandom(12)
    data = AESGCM(_key(password, serial)).encrypt(nonce, plaintext, serial.upper().encode("utf-8"))
    return json.dumps({"v": 1, "nonce": base64.b64encode(nonce).decode("ascii"), "data": base64.b64encode(data).decode("ascii")})


def open_offer(password, serial, offer_json):
    """Přihlašovací údaje z nabídky, nebo None, když heslo nesedí (nebo je nabídka poškozená)."""
    try:
        offer = json.loads(offer_json)
        nonce = base64.b64decode(offer["nonce"])
        data = base64.b64decode(offer["data"])
        plaintext = AESGCM(_key(password, serial)).decrypt(nonce, data, serial.upper().encode("utf-8"))
        credentials = json.loads(plaintext)
        return credentials["username"], credentials["password"]
    except (InvalidTag, ValueError, KeyError, TypeError):
        return None


def announcement(serial, model, nodes, rssi):
    """Kdo jsem a co měřím — nikdy žádné tajemství. nodes: {adresa: [typy senzorů]}."""
    return json.dumps({
        "serial": serial,
        "kind": "gateway",
        "model": model,
        "nodes": [{"address": address, "sensors": types} for address, types in sorted(nodes.items())],
        "rssi": rssi,
        "emulated": True,
        "ts": int(time.time()),
    })


def result(ok):
    return json.dumps({"ok": True, "reason": None} if ok else {"ok": False, "reason": "wrong-password"})
