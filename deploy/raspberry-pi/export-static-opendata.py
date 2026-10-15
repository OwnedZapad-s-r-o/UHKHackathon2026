#!/usr/bin/env python3
"""Varianta B — export otevřených dat SmartAuditu do statických souborů pro mirror na Raspberry Pi.

Spouští se tam, kde běží aplikace (notebook): stáhne z otevřeného API katalog DCAT-AP-CZ, záznamy datových sad,
seznam objektů (JSON, GeoJSON, CSV), pre-audit (JSON, CSV), zdroje dat a detail každého objektu.
Výsledek nahrajte na Raspberry Pi, kde ho vystaví opendata-mirror.py.

    python export-static-opendata.py --source http://localhost:3000 --out ./mirror-data
    python export-static-opendata.py --upload pi@smartaudit.nasedomena.cz:/opt/smartaudit-mirror/data

Jen standardní knihovna Pythonu 3.9+ (žádný pip install).
"""

from __future__ import annotations

import argparse
import json
import shutil
import subprocess
import sys
import time
import urllib.error
import urllib.request
from datetime import datetime, timezone
from pathlib import Path

# Výpis i v konzoli Windows (cp1250) bez pádu na znacích ▶ ✓
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

API = "/api/open/v1"

# Soubory exportu: cesta v API → soubor v exportu
LISTS = {
    f"{API}/catalog": "catalog.jsonld",
    f"{API}/facilities": "facilities.json",
    f"{API}/facilities.geojson": "facilities.geojson",
    f"{API}/facilities.csv": "facilities.csv",
    f"{API}/preaudit": "preaudit.json",
    f"{API}/preaudit.csv": "preaudit.csv",
    f"{API}/datasets": "datasets.json",
}


def fetch(url: str, timeout: float, retries: int = 3) -> bytes:
    last: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            request = urllib.request.Request(url, headers={"User-Agent": "SmartAudit-export/1.0", "Accept": "*/*"})
            with urllib.request.urlopen(request, timeout=timeout) as response:
                return response.read()
        except (urllib.error.URLError, TimeoutError, ConnectionError) as error:
            last = error
            if attempt < retries:
                time.sleep(1.5 * attempt)
    raise RuntimeError(f"{url}: {last}")


def main() -> int:
    parser = argparse.ArgumentParser(description="Export otevřených dat SmartAuditu pro statický mirror")
    parser.add_argument("--source", default="http://localhost:3000", help="adresa běžící aplikace (web Next.js nebo API :8080)")
    parser.add_argument("--out", default="mirror-data", help="cílová složka exportu")
    parser.add_argument("--timeout", type=float, default=30)
    parser.add_argument("--no-details", action="store_true", help="bez detailu jednotlivých objektů (rychlejší)")
    parser.add_argument("--upload", help="po exportu nahrát přes scp, např. pi@rpi:/opt/smartaudit-mirror/data")
    args = parser.parse_args()

    source = args.source.rstrip("/")
    out = Path(args.out)
    staging = out.with_name(out.name + ".tmp")
    shutil.rmtree(staging, ignore_errors=True)
    (staging / "datasets").mkdir(parents=True)
    (staging / "facilities").mkdir(parents=True)

    print(f"▶ Export z {source}")
    counts: dict[str, int] = {}

    for path, name in LISTS.items():
        body = fetch(f"{source}{path}", args.timeout)
        (staging / name).write_bytes(body)
        print(f"  ✓ {path} → {name} ({len(body) / 1024:.0f} kB)")

    # Záznamy datových sad z katalogu (IRI končí identifikátorem sady)
    catalog = json.loads((staging / "catalog.jsonld").read_text(encoding="utf-8"))
    catalog_iri = catalog.get("iri", f"{source}{API}/catalog")
    for iri in catalog.get("datová_sada", []):
        dataset_id = iri.rstrip("/").rsplit("/", 1)[-1]
        body = fetch(f"{source}{API}/catalog/datasets/{dataset_id}", args.timeout)
        (staging / "datasets" / f"{dataset_id}.jsonld").write_bytes(body)
        print(f"  ✓ datová sada {dataset_id}")
    counts["datasets"] = len(catalog.get("datová_sada", []))

    facilities = json.loads((staging / "facilities.json").read_text(encoding="utf-8"))
    counts["facilities"] = len(facilities)
    counts["preaudits"] = len(json.loads((staging / "preaudit.json").read_text(encoding="utf-8")))

    if not args.no_details:
        for index, facility in enumerate(facilities, start=1):
            body = fetch(f"{source}{API}/facilities/{facility['id']}", args.timeout)
            (staging / "facilities" / f"{facility['id']}.json").write_bytes(body)
            if index % 25 == 0 or index == len(facilities):
                print(f"  ✓ detail objektů {index}/{len(facilities)}")

    manifest = {
        "exportedAt": datetime.now(timezone.utc).isoformat(timespec="seconds"),
        "source": source,
        # Základ IRI v katalogu — mirror ho přepíše na svou veřejnou doménu
        "sourceBaseUrl": catalog_iri.removesuffix(f"{API}/catalog"),
        "license": "CC BY 4.0",
        "counts": counts,
    }
    (staging / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

    shutil.rmtree(out, ignore_errors=True)
    staging.rename(out)
    print(f"✔ Hotovo: {out.resolve()} — {counts['facilities']} objektů, {counts['datasets']} datové sady")

    if args.upload:
        target = args.upload.rstrip("/")
        print(f"▶ Nahrávám na {target}")
        # Obsah složky do cílové složky (scp -r out/. host:path)
        result = subprocess.run(["scp", "-r", f"{out}/.", target], check=False)
        if result.returncode != 0:
            print("✖ Nahrání selhalo — zkuste ručně: scp -r", f"{out}/.", target, file=sys.stderr)
            return result.returncode
        print("✔ Nahráno — mirror načte nová data sám při dalším požadavku")

    return 0


if __name__ == "__main__":
    sys.exit(main())
