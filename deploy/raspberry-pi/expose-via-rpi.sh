#!/usr/bin/env bash
# Varianta A — vystaví SmartAudit běžící na notebooku přes Raspberry Pi s veřejnou doménou.
# Spouští se NA NOTEBOOKU (Linux, macOS, Windows Git Bash / WSL). Otevře reverzní SSH tunel:
#   Raspberry Pi 127.0.0.1:3000  →  notebook localhost:3000  (Next.js — web, QR karty, /api/* přes proxy)
#   Raspberry Pi 127.0.0.1:8080  →  notebook localhost:8080  (ASP.NET API — otevřené API /api/open/v1/*)
# Nginx na Raspberry Pi pak publikuje https://<doména> → 127.0.0.1:3000 (viz nginx/smartaudit.conf).
# Když tunel spadne (notebook usne), nginx sám přepne na statický mirror otevřených dat (Varianta B).
#
# Použití:
#   ./expose-via-rpi.sh pi@smartaudit.nasedomena.cz
#   RPI_SSH_PORT=2222 LOCAL_WEB_PORT=3000 ./expose-via-rpi.sh pi@192.168.1.50
set -euo pipefail

TARGET="${1:-${RPI_TARGET:-}}"
if [[ -z "$TARGET" ]]; then
  echo "Použití: $0 uzivatel@raspberry-pi   (např. pi@smartaudit.nasedomena.cz)" >&2
  exit 1
fi

SSH_PORT="${RPI_SSH_PORT:-22}"
LOCAL_WEB_PORT="${LOCAL_WEB_PORT:-3000}"
LOCAL_API_PORT="${LOCAL_API_PORT:-8080}"
REMOTE_WEB_PORT="${REMOTE_WEB_PORT:-3000}"
REMOTE_API_PORT="${REMOTE_API_PORT:-8080}"

# Běží aplikace na notebooku?
if command -v curl >/dev/null 2>&1; then
  if ! curl -fsS -m 5 -o /dev/null "http://localhost:${LOCAL_WEB_PORT}/"; then
    echo "⚠  Na http://localhost:${LOCAL_WEB_PORT} nic neodpovídá — nejdřív: docker compose up --build" >&2
  fi
fi

SSH_OPTS=(
  -N
  -p "$SSH_PORT"
  -o ExitOnForwardFailure=yes
  -o ServerAliveInterval=15
  -o ServerAliveCountMax=3
  -o TCPKeepAlive=yes
  -R "127.0.0.1:${REMOTE_WEB_PORT}:localhost:${LOCAL_WEB_PORT}"
  -R "127.0.0.1:${REMOTE_API_PORT}:localhost:${LOCAL_API_PORT}"
)

echo "▶ Tunel ${TARGET}: RPi:${REMOTE_WEB_PORT} → notebook:${LOCAL_WEB_PORT}, RPi:${REMOTE_API_PORT} → notebook:${LOCAL_API_PORT}"
echo "  Ukončení: Ctrl+C. Při výpadku Wi-Fi se tunel sám obnoví."

# autossh drží tunel i přes výpadky sítě; bez něj smyčka s obyčejným ssh
if command -v autossh >/dev/null 2>&1; then
  export AUTOSSH_GATETIME=0
  exec autossh -M 0 "${SSH_OPTS[@]}" "$TARGET"
fi

while true; do
  ssh "${SSH_OPTS[@]}" "$TARGET" || true
  echo "↻ Tunel spadl ($(date +%H:%M:%S)), znovu za 3 s…"
  sleep 3
done
