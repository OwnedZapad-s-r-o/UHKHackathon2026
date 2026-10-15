#!/usr/bin/env bash
# Jednorázová příprava Raspberry Pi (Raspberry Pi OS / Debian / Ubuntu) — spouští se NA RASPBERRY PI:
#   sudo ./setup-rpi.sh smartaudit.nasedomena.cz admin@nasedomena.cz
# Nainstaluje nginx + certbot, získá HTTPS certifikát, nainstaluje mirror otevřených dat jako službu
# a nastaví nginx: doména → tunel na notebook (Varianta A), při výpadku → mirror (Varianta B).
set -euo pipefail

DOMAIN="${1:?Použití: sudo $0 <domena> <email-pro-lets-encrypt>}"
EMAIL="${2:?Použití: sudo $0 <domena> <email-pro-lets-encrypt>}"
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MIRROR_DIR=/opt/smartaudit-mirror
SITE=/etc/nginx/sites-available/smartaudit.conf

if [[ $EUID -ne 0 ]]; then
  echo "Spusťte přes sudo." >&2
  exit 1
fi

echo "▶ 1/5 Balíčky (nginx, certbot, python3)"
apt-get update -qq
apt-get install -y -qq nginx certbot python3 >/dev/null

echo "▶ 2/5 Mirror otevřených dat → ${MIRROR_DIR}"
id -u smartaudit >/dev/null 2>&1 || useradd --system --home "$MIRROR_DIR" --shell /usr/sbin/nologin smartaudit
mkdir -p "$MIRROR_DIR/data"
install -m 0755 "$HERE/opendata-mirror.py" "$MIRROR_DIR/opendata-mirror.py"
# Uživatel, přes kterého nahráváte export (scp), smí zapisovat do data/
SUDO_OWNER="${SUDO_USER:-root}"
chown -R "$SUDO_OWNER":smartaudit "$MIRROR_DIR/data"
chmod -R g+rX "$MIRROR_DIR/data"
sed "s#__BASE_URL__#https://${DOMAIN}#" "$HERE/systemd/smartaudit-mirror.service" > /etc/systemd/system/smartaudit-mirror.service
systemctl daemon-reload
systemctl enable --now smartaudit-mirror.service

echo "▶ 3/5 HTTPS certifikát Let's Encrypt pro ${DOMAIN}"
mkdir -p /var/www/html
if [[ ! -f "/etc/letsencrypt/live/${DOMAIN}/fullchain.pem" ]]; then
  # Dočasně jen port 80 pro ověření domény
  cat > "$SITE" <<EOF
server {
    listen 80;
    server_name ${DOMAIN};
    location /.well-known/acme-challenge/ { root /var/www/html; }
    location / { return 503; }
}
EOF
  ln -sf "$SITE" /etc/nginx/sites-enabled/smartaudit.conf
  rm -f /etc/nginx/sites-enabled/default
  nginx -t && systemctl reload nginx
  certbot certonly --webroot -w /var/www/html -d "$DOMAIN" --email "$EMAIL" --agree-tos --non-interactive
fi

echo "▶ 4/5 Nginx: ${DOMAIN} → tunel 127.0.0.1:3000 / 8080, záloha mirror 127.0.0.1:8090"
sed "s/__DOMAIN__/${DOMAIN}/g" "$HERE/nginx/smartaudit.conf" > "$SITE"
ln -sf "$SITE" /etc/nginx/sites-enabled/smartaudit.conf
rm -f /etc/nginx/sites-enabled/default
nginx -t
systemctl reload nginx

echo "▶ 5/5 SSH: tunel smí poslouchat jen na 127.0.0.1 (výchozí GatewayPorts no) — nic dalšího netřeba"
systemctl is-active --quiet ssh || systemctl enable --now ssh

cat <<EOF

✔ Hotovo. Teď na notebooku:
   1) docker compose up --build            (s PUBLIC_URL=https://${DOMAIN} v .env)
   2) ./deploy/raspberry-pi/expose-via-rpi.sh ${SUDO_OWNER}@${DOMAIN}
   3) python deploy/raspberry-pi/export-static-opendata.py --upload ${SUDO_OWNER}@${DOMAIN}:${MIRROR_DIR}/data

Kontrola:  curl -I https://${DOMAIN}/            (X-SmartAudit-Source: live / mirror)
           curl https://${DOMAIN}/mirror/health
EOF
