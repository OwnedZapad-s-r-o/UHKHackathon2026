#!/usr/bin/env python3
"""Varianta B — statický mirror otevřených dat a veřejných karet SmartAuditu pro Raspberry Pi.

Vystaví export z export-static-opendata.py na stejných adresách jako živé API:
  /api/open/v1/catalog                  katalog DCAT-AP-CZ (JSON-LD, IRI přepsané na veřejnou doménu)
  /api/open/v1/catalog/datasets/{id}    záznam datové sady
  /api/open/v1/facilities[.geojson|.csv] objekty kraje (filtry q, category, district, status)
  /api/open/v1/facilities/{id}          detail objektu
  /api/open/v1/preaudit[.csv]           pre-audit z otevřených dat
  /api/open/v1/datasets                 použité zdroje dat
  /objekty, /objekty/{kód}              veřejné karty (cíl QR kódů na dveřích) — když notebook spí
  /health                               stav mirroru

Jen standardní knihovna Pythonu 3.9+. Data se znovu načtou sama, když nahrajete nový export.

    python3 opendata-mirror.py --data /opt/smartaudit-mirror/data --port 8090 --base-url https://smartaudit.nasedomena.cz
"""

from __future__ import annotations

import argparse
import csv
import html
import io
import json
import re
import sys
import threading
from http import HTTPStatus
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import parse_qs, unquote, urlparse

# Výpis i v konzoli Windows (cp1250) bez pádu na znacích ▶ ✓
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

API = "/api/open/v1"
UUID = re.compile(r"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", re.I)

STATUS_LABEL = {"Nominal": "Optimální chod", "Warning": "Varování", "Critical": "Kritická anomálie"}
STATUS_COLOR = {"Nominal": "#065f46", "Warning": "#92400e", "Critical": "#991b1b"}
STATUS_BG = {"Nominal": "#ecfdf5", "Warning": "#fffbeb", "Critical": "#fef2f2"}
MODEL_TEXT = (
    "Hodnoty bez čidla jsou deterministicky počítány z reálné podlahové plochy a stáří budovy (ČÚZK INSPIRE), "
    "kapacity osob a investic (Data KHK), cen energií (Registr smluv) a počasí (Open-Meteo)."
)


class MirrorData:
    """Export načtený do paměti; při změně manifest.json se načte znovu (nové nahrání z notebooku)."""

    def __init__(self, folder: Path, base_url: str | None):
        self.folder = folder
        self.base_url = base_url.rstrip("/") if base_url else None
        self._lock = threading.Lock()
        self._stamp: float | None = None
        self.manifest: dict = {}
        self.facilities: list[dict] = []
        self.by_code: dict[str, dict] = {}

    def refresh(self) -> None:
        manifest = self.folder / "manifest.json"
        stamp = manifest.stat().st_mtime if manifest.exists() else None
        with self._lock:
            if stamp == self._stamp:
                return
            self._stamp = stamp
            self.manifest = json.loads(manifest.read_text(encoding="utf-8")) if stamp else {}
            path = self.folder / "facilities.json"
            self.facilities = json.loads(path.read_text(encoding="utf-8")) if path.exists() else []
            self.by_code = {f["code"].upper(): f for f in self.facilities}

    def read(self, name: str) -> bytes | None:
        path = (self.folder / name).resolve()
        if self.folder.resolve() not in path.parents or not path.is_file():
            return None
        return path.read_bytes()

    def rewrite_iris(self, body: bytes, request_base: str) -> bytes:
        """IRI v katalogu ukazují na doménu, odkud se exportovalo — přepíše je na adresu mirroru."""
        source = self.manifest.get("sourceBaseUrl")
        target = self.base_url or request_base
        if not source or source == target:
            return body
        return body.decode("utf-8").replace(source, target).encode("utf-8")


def matches(facility: dict, query: dict[str, list[str]]) -> bool:
    def value(key: str) -> str | None:
        return query.get(key, [None])[0]

    if (q := value("q")) and q.lower() not in f"{facility.get('name', '')} {facility.get('code', '')} {facility.get('municipalityName', '')}".lower():
        return False
    for key, field in (("category", "category"), ("district", "districtCode"), ("status", "status")):
        if (wanted := value(key)) and str(facility.get(field)) != wanted:
            return False
    return True


class Handler(BaseHTTPRequestHandler):
    server_version = "SmartAuditMirror/1.0"
    data: MirrorData

    def log_message(self, format: str, *args) -> None:  # noqa: A002 — podpis z BaseHTTPRequestHandler
        print(f"{self.address_string()} {format % args}")

    # ---------- odpovědi
    def send(self, status: int, body: bytes, content_type: str, cache: str = "public, max-age=300") -> None:
        self.send_response(status)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Cache-Control", cache)
        self.send_header("X-SmartAudit-Mirror", f"static export {self.data.manifest.get('exportedAt', '?')}")
        self.end_headers()
        if self.command != "HEAD":
            self.wfile.write(body)

    def json(self, value, status: int = 200, content_type: str = "application/json; charset=utf-8") -> None:
        self.send(status, json.dumps(value, ensure_ascii=False).encode("utf-8"), content_type)

    def not_found(self, message: str = "Nenalezeno") -> None:
        self.json({"message": message}, HTTPStatus.NOT_FOUND)

    def request_base(self) -> str:
        host = self.headers.get("X-Forwarded-Host") or self.headers.get("Host") or "localhost"
        proto = self.headers.get("X-Forwarded-Proto") or "http"
        return f"{proto}://{host}"

    # ---------- směrování
    def do_HEAD(self) -> None:  # noqa: N802
        self.do_GET()

    def do_GET(self) -> None:  # noqa: N802
        self.data.refresh()
        url = urlparse(self.path)
        path = unquote(url.path).rstrip("/") or "/"
        query = parse_qs(url.query)

        if path == "/health":
            return self.json({"status": "ok", "mode": "mirror", **self.data.manifest}, content_type="application/json; charset=utf-8")
        if path == "/":
            return self.page_index()
        if path == "/objekty":
            return self.page_list(query)
        if path.startswith("/objekty/"):
            return self.page_card(path.split("/")[2])
        if path.startswith(API):
            return self.api(path[len(API):] or "/", query)

        self.not_found("Tato stránka je dostupná jen v živé aplikaci — mirror vystavuje otevřená data a veřejné karty objektů.")

    def api(self, path: str, query: dict[str, list[str]]) -> None:
        d = self.data
        if path == "/catalog":
            body = d.read("catalog.jsonld")
            return self.send(200, d.rewrite_iris(body, self.request_base()), "application/ld+json; charset=utf-8") if body else self.not_found()
        if path.startswith("/catalog/datasets/"):
            dataset = path.rsplit("/", 1)[-1]
            body = d.read(f"datasets/{dataset}.jsonld") if re.fullmatch(r"[a-z0-9-]+", dataset) else None
            return self.send(200, d.rewrite_iris(body, self.request_base()), "application/ld+json; charset=utf-8") if body else self.not_found("Datová sada s tímto identifikátorem v katalogu není")
        if path == "/facilities":
            return self.json([f for f in d.facilities if matches(f, query)])
        if path == "/facilities.geojson":
            features = [
                {"type": "Feature", "geometry": {"type": "Point", "coordinates": [f["longitude"], f["latitude"]]}, "properties": f}
                for f in d.facilities
                if matches(f, query)
            ]
            return self.json({"type": "FeatureCollection", "features": features}, content_type="application/geo+json; charset=utf-8")
        if path in ("/facilities.csv", "/preaudit.csv"):
            name = path.lstrip("/")
            body = d.read(name)
            if body is None:
                return self.not_found()
            if name == "facilities.csv" and query:
                body = self.filter_csv(body, query)
            return self.send(200, body, "text/csv; charset=utf-8")
        if path == "/preaudit":
            body = d.read("preaudit.json")
            return self.send(200, body, "application/json; charset=utf-8") if body else self.not_found()
        if path == "/datasets":
            body = d.read("datasets.json")
            return self.send(200, body, "application/json; charset=utf-8") if body else self.not_found()
        if path.startswith("/facilities/"):
            facility_id = path.split("/")[2]
            body = d.read(f"facilities/{facility_id}.json") if UUID.match(facility_id) else None
            return self.send(200, body, "application/json; charset=utf-8") if body else self.not_found("Objekt s tímto id neexistuje")
        self.not_found()

    def filter_csv(self, body: bytes, query: dict[str, list[str]]) -> bytes:
        allowed = {f["id"] for f in self.data.facilities if matches(f, query)}
        text = body.decode("utf-8-sig")
        rows = list(csv.reader(io.StringIO(text)))
        output = io.StringIO()
        writer = csv.writer(output, lineterminator="\r\n")
        writer.writerow(rows[0])
        writer.writerows(row for row in rows[1:] if row and row[0] in allowed)
        return "﻿".encode("utf-8") + output.getvalue().encode("utf-8")

    # ---------- HTML (veřejné karty pro QR kódy)
    def html(self, title: str, body: str) -> None:
        exported = html.escape(self.data.manifest.get("exportedAt", "neznámo"))
        page = f"""<!doctype html><html lang="cs"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>{html.escape(title)} · SmartAudit KHK</title>
<style>
body{{margin:0;font-family:system-ui,-apple-system,"Segoe UI",sans-serif;background:#f1f5f9;color:#0b192c}}
header{{background:#0b192c;color:#fff;padding:14px 20px;border-bottom:4px solid #d90429;font-weight:700}}
main{{max-width:760px;margin:0 auto;padding:20px}} a{{color:#1e3e62}} .card{{background:#fff;border:1px solid #e2e8f0;border-radius:16px;padding:18px;margin-top:14px}}
.pill{{display:inline-block;border-radius:999px;padding:3px 10px;font-size:12px;font-weight:700}} .muted{{color:#64748b;font-size:13px}}
dl{{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:12px}} dt{{font-size:12px;color:#64748b}} dd{{margin:2px 0 0;font-weight:700;font-family:ui-monospace,monospace}}
li{{margin:6px 0}} footer{{max-width:760px;margin:0 auto;padding:0 20px 30px;font-size:12px;color:#64748b}}
</style></head><body><header>SmartAudit KHK · otevřená data kraje</header><main>{body}</main>
<footer>Statický mirror otevřených dat (export {exported}) · CC BY 4.0 · <a href="{API}/catalog">katalog DCAT-AP-CZ</a> · {html.escape(MODEL_TEXT)}</footer></body></html>"""
        self.send(200, page.encode("utf-8"), "text/html; charset=utf-8", cache="public, max-age=60")

    def page_index(self) -> None:
        counts = self.data.manifest.get("counts", {})
        links = "".join(
            f'<li><a href="{API}{p}">{API}{p}</a> — {html.escape(t)}</li>'
            for p, t in [
                ("/catalog", "katalog DCAT-AP-CZ (JSON-LD)"),
                ("/facilities", "objekty (JSON)"),
                ("/facilities.geojson", "objekty (GeoJSON)"),
                ("/facilities.csv", "objekty (CSV)"),
                ("/preaudit", "pre-audit z otevřených dat (JSON)"),
                ("/preaudit.csv", "pre-audit (CSV)"),
                ("/datasets", "použité zdroje dat"),
            ]
        )
        self.html(
            "Otevřená data",
            f"""<h1>Otevřená data SmartAudit KHK</h1>
<p class="muted">Živá aplikace je právě nedostupná — tady je poslední export otevřených dat ({counts.get('facilities', '?')} objektů kraje).</p>
<div class="card"><ul>{links}</ul><p><a href="/objekty">Veřejné karty objektů →</a></p></div>""",
        )

    def page_list(self, query: dict[str, list[str]]) -> None:
        items = "".join(
            f'<li><a href="/objekty/{html.escape(f["code"])}">{html.escape(f["name"])}</a> <span class="muted">· {html.escape(f.get("municipalityName", ""))} · {STATUS_LABEL.get(f.get("status"), "")}</span></li>'
            for f in sorted(self.data.facilities, key=lambda f: f.get("name", ""))
            if matches(f, query)
        )
        self.html("Objekty", f"<h1>Objekty kraje</h1><div class=\"card\"><ul>{items}</ul></div>")

    def page_card(self, code: str) -> None:
        facility = self.data.by_code.get(code.upper())
        if not facility:
            return self.not_found("Objekt s tímto kódem neexistuje")

        status = facility.get("status", "Nominal")
        live = facility.get("dataMode") == "LiveIqrf" or facility.get("hasLiveSensors")
        mode = "📡 Živé měření IQRF (MQTT Standard Sensor)" if live else "📐 Fyzikální model z otevřených dat (ČÚZK + Data KHK + Open-Meteo)"

        def fmt(value, unit: str = "", digits: int = 0) -> str:
            if value is None:
                return "—"
            text = f"{value:,.{digits}f}".replace(",", " ").replace(".", ",")
            return f"{text} {unit}".strip()

        address = ", ".join(x for x in [" ".join(x for x in [facility.get("street"), facility.get("houseNumber")] if x), facility.get("municipalityName")] if x)
        self.html(
            facility["name"],
            f"""<p class="muted">{html.escape(facility.get("code", ""))} · okres {html.escape(facility.get("districtName", ""))}</p>
<h1 style="margin:4px 0 6px">{html.escape(facility["name"])}</h1>
<p class="muted">{html.escape(address)}</p>
<span class="pill" style="background:{STATUS_BG.get(status)};color:{STATUS_COLOR.get(status)}">{STATUS_LABEL.get(status, status)}</span>
<span class="pill" style="background:#eff4fa;color:#1e3e62;margin-left:6px" title="{html.escape(MODEL_TEXT)}">{mode}</span>
<div class="card"><h2 style="margin-top:0;font-size:18px">{html.escape(facility.get("headline", ""))}</h2>
<dl><div><dt>CO₂ v nejhorší místnosti</dt><dd>{fmt(facility.get("co2"), "ppm")}</dd></div>
<div><dt>Teplota</dt><dd>{fmt(facility.get("temperature"), "°C", 1)}</dd></div>
<div><dt>Hodin nad limitem CO₂ / 7 dní</dt><dd>{fmt(facility.get("co2HoursOverLimit"), "h")}</dd></div>
<div><dt>Pre-audit z otevřených dat</dt><dd>{fmt(facility.get("preAuditScore"))}/100</dd></div>
<div><dt>Měrná tepelná ztráta</dt><dd>{fmt(facility.get("specificHeatLoss"), "W/m²K", 2)}</dd></div>
<div><dt>Denostupně lokality</dt><dd>{fmt(facility.get("degreeDays"), "K·den")}</dd></div></dl>
<p class="muted">Hlavní příčina podle pre-auditu: {html.escape(facility.get("preAuditDriver") or "—")}</p></div>
<p class="muted">Data: <a href="{API}/facilities/{facility['id']}">{API}/facilities/{facility['id']}</a></p>""",
        )


def main() -> None:
    parser = argparse.ArgumentParser(description="Statický mirror otevřených dat SmartAuditu")
    parser.add_argument("--data", default="data", help="složka s exportem z export-static-opendata.py")
    parser.add_argument("--host", default="127.0.0.1", help="127.0.0.1 = jen za nginxem (doporučeno), 0.0.0.0 = celá síť")
    parser.add_argument("--port", type=int, default=8090)
    parser.add_argument("--base-url", help="veřejná adresa pro IRI v katalogu, např. https://smartaudit.nasedomena.cz")
    args = parser.parse_args()

    Handler.data = MirrorData(Path(args.data), args.base_url)
    Handler.data.refresh()
    print(f"▶ SmartAudit mirror na http://{args.host}:{args.port} — data {Path(args.data).resolve()} ({len(Handler.data.facilities)} objektů)")
    ThreadingHTTPServer((args.host, args.port), Handler).serve_forever()


if __name__ == "__main__":
    main()
