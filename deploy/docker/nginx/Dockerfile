# Nginx před SmartAuditem s přístupovými logy návštěvníků (konfigurace zapečená v image — na cílovém stroji stačí docker load)
FROM nginx:1.27-alpine

COPY nginx.conf /etc/nginx/nginx.conf

# Image má access.log jako odkaz na stdout — vlastní JSON log musí být skutečný soubor
RUN rm -f /var/log/nginx/access.log /var/log/nginx/error.log \
    && touch /var/log/nginx/access.json.log \
    && nginx -t

EXPOSE 80

HEALTHCHECK --interval=15s --timeout=3s --retries=3 CMD wget -q -O /dev/null http://127.0.0.1/nginx-health || exit 1
