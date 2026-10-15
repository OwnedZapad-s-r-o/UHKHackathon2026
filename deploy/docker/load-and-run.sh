#!/usr/bin/env bash
# Na cílovém stroji (Linux / Raspberry Pi): načte image z archivu a spustí stack.
#   ./load-and-run.sh            # bez tunelu, nebo s tunelem, když je v .env CLOUDFLARE_TUNNEL_TOKEN
#   ./load-and-run.sh quick      # + dočasná adresa *.trycloudflare.com
set -euo pipefail
cd "$(dirname "$0")"

ARCHIVE="$(grep -E '^SMARTAUDIT_ARCHIVE=' .env | cut -d= -f2- || true)"
ARCHIVE="${ARCHIVE:-$(ls smartaudit-stack.tar* 2>/dev/null | head -n1)}"
[[ -f "$ARCHIVE" ]] || { echo "Archiv s image nenalezen (smartaudit-stack.tar[.gz])" >&2; exit 1; }

echo "▶ docker load -i $ARCHIVE"
docker load -i "$ARCHIVE"

mkdir -p logs/nginx
PROFILES=()
if grep -qE '^CLOUDFLARE_TUNNEL_TOKEN=.+' .env; then PROFILES+=(--profile tunnel); fi
if [[ "${1:-}" == "quick" ]]; then PROFILES+=(--profile quicktunnel); fi

echo "▶ docker compose up -d ${PROFILES[*]:-}"
docker compose -f docker-compose.prod.yml "${PROFILES[@]}" up -d

PORT="$(grep -E '^HTTP_PORT=' .env | cut -d= -f2- || true)"
echo
echo "✔ Běží: http://localhost:${PORT:-8088}  (první start importuje otevřená data ~1–2 min)"
echo "  Návštěvníci:  docker compose -f docker-compose.prod.yml logs -f nginx"
echo "  JSON log:     tail -f logs/nginx/access.json.log"
if [[ "${1:-}" == "quick" ]]; then
  sleep 8
  echo "  Veřejná adresa:"; docker compose -f docker-compose.prod.yml logs cloudflared-quick 2>&1 | grep -o 'https://[a-z0-9-]*\.trycloudflare\.com' | tail -n1
fi
