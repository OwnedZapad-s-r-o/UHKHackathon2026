#!/bin/sh
# Příprava brokeru při startu kontejneru:
# 1) účet backendu v Dynamic Security (při změně hesla v .env se konfigurace založí znovu — účty gatewayí dosype backend),
# 2) vlastní certifikační autorita a certifikát serveru pro TLS port 8883 (jen poprvé, pak se znovu použijí).
set -eu

DATA=/mosquitto/data
CERTS=/mosquitto/certs
DYNSEC="$DATA/dynamic-security.json"
ADMIN_USER="${MQTT_BACKEND_USERNAME:-smartaudit-backend}"
ADMIN_PASSWORD="${MQTT_BACKEND_PASSWORD:?Nastavte MQTT_BACKEND_PASSWORD}"

mkdir -p "$DATA" "$CERTS"

# Otisk jména a hesla — heslo samotné se na disk neukládá
marker="$(printf '%s:%s' "$ADMIN_USER" "$ADMIN_PASSWORD" | sha256sum | cut -d' ' -f1)"
if [ ! -f "$DYNSEC" ] || [ "$(cat "$DATA/.backend-account" 2>/dev/null)" != "$marker" ]; then
  rm -f "$DYNSEC"
  mosquitto_ctrl dynsec init "$DYNSEC" "$ADMIN_USER" "$ADMIN_PASSWORD" > /dev/null
  printf '%s' "$marker" > "$DATA/.backend-account"
  echo "Dynamic Security: založen účet backendu $ADMIN_USER"
fi

if [ ! -f "$CERTS/server.crt" ]; then
  # Názvy a adresy, na které se gatewaye připojují (MQTT_TLS_HOSTS = čárkami oddělené IP nebo DNS názvy)
  san="DNS:mqtt,DNS:localhost,IP:127.0.0.1"
  for host in $(printf '%s' "${MQTT_TLS_HOSTS:-}" | tr ',' ' '); do
    case "$host" in
      *[!0-9.]*) san="$san,DNS:$host" ;;
      *) san="$san,IP:$host" ;;
    esac
  done

  openssl req -x509 -newkey rsa:3072 -nodes -days 3650 -subj "/CN=SmartAudit KHK MQTT CA" \
    -keyout "$CERTS/ca.key" -out "$CERTS/ca.crt" 2> /dev/null
  openssl req -newkey rsa:3072 -nodes -subj "/CN=smartaudit-mqtt" \
    -keyout "$CERTS/server.key" -out "$CERTS/server.csr" 2> /dev/null
  printf 'subjectAltName=%s\nextendedKeyUsage=serverAuth\n' "$san" > "$CERTS/server.ext"
  openssl x509 -req -in "$CERTS/server.csr" -CA "$CERTS/ca.crt" -CAkey "$CERTS/ca.key" -CAcreateserial \
    -days 825 -sha256 -extfile "$CERTS/server.ext" -out "$CERTS/server.crt" 2> /dev/null
  rm -f "$CERTS/server.csr" "$CERTS/server.ext"

  # Soukromé klíče čte jen broker, veřejný certifikát CA si stáhne správce pro gatewaye
  chmod 600 "$CERTS/ca.key" "$CERTS/server.key"
  chmod 644 "$CERTS/ca.crt" "$CERTS/server.crt"
  echo "TLS: vytvořen certifikát serveru pro $san"
fi

chown -R mosquitto:mosquitto "$DATA" "$CERTS"
chmod 700 "$DATA"

exec /usr/sbin/mosquitto -c /mosquitto/config/mosquitto.conf
