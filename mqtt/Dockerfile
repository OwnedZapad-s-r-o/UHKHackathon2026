# Mosquitto s nástroji pro první start: openssl (vlastní CA a certifikát pro TLS) a entrypoint pro Dynamic Security
FROM eclipse-mosquitto:2

RUN apk add --no-cache openssl

COPY mosquitto.conf /mosquitto/config/mosquitto.conf
COPY --chmod=755 entrypoint.sh /usr/local/bin/smartaudit-entrypoint.sh

EXPOSE 1883 8883
ENTRYPOINT ["/usr/local/bin/smartaudit-entrypoint.sh"]
