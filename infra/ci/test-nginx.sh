#!/usr/bin/env bash
# Перевірка конфігу Nginx: той самий образ, що на сервері, тимчасовий самопідписаний сертифікат,
# nginx -t без попереджень і справжні відповіді сайту (редірект, Let's Encrypt, HTTPS, заголовки).
# Запуск з кореня репозиторію (потрібен Docker): bash infra/ci/test-nginx.sh
set -euo pipefail
cd "$(dirname "$0")/../.."

DOMAIN=diploma-marketplace.denmarkeast.cloudapp.azure.com
IMAGE=$(docker compose -f docker-compose.yml config --images | grep '^nginx:')   # версія з docker-compose.yml
NAME=ci-nginx-$$
HTTP=18080
HTTPS=18443
TMP=$(mktemp -d)
FAILED=0

cleanup() { docker rm -f "$NAME" >/dev/null 2>&1 || true; rm -rf "$TMP"; }
trap cleanup EXIT

ok()   { echo "  ✓ $1"; }
fail() { echo "  ✗ $1"; FAILED=1; }

echo "Образ: $IMAGE"

# Тимчасовий сертифікат там, де nginx чекає справжній від Let's Encrypt
mkdir -p "$TMP/letsencrypt/live/$DOMAIN"
openssl req -x509 -newkey rsa:2048 -nodes -days 1 -subj "/CN=$DOMAIN" \
    -keyout "$TMP/letsencrypt/live/$DOMAIN/privkey.pem" \
    -out "$TMP/letsencrypt/live/$DOMAIN/fullchain.pem" 2>/dev/null
chmod -R a+rX "$TMP/letsencrypt"

VOLUMES=(
    -v "$PWD/infra/nginx/conf.d:/etc/nginx/conf.d:ro"
    -v "$PWD/infra/nginx/html:/usr/share/nginx/html:ro"
    -v "$TMP/letsencrypt:/etc/letsencrypt:ro"
)

echo "1. nginx -t"
if OUT=$(docker run --rm "${VOLUMES[@]}" "$IMAGE" nginx -t 2>&1); then
    if grep -q '\[warn\]' <<<"$OUT"; then
        echo "$OUT"; fail "nginx -t пройшов, але з попередженнями"
    else
        ok "синтаксис без помилок і попереджень"
    fi
else
    echo "$OUT"; fail "nginx -t: помилка в конфігу"
    exit 1
fi

echo "2. Відповіді сайту"
docker run -d --name "$NAME" -p "127.0.0.1:$HTTP:80" -p "127.0.0.1:$HTTPS:443" "${VOLUMES[@]}" "$IMAGE" >/dev/null
for _ in $(seq 1 20); do
    curl -s -o /dev/null "http://127.0.0.1:$HTTP/" && break
    sleep 0.5
done

RES=(--noproxy '*' --resolve "$DOMAIN:$HTTP:127.0.0.1" --resolve "$DOMAIN:$HTTPS:127.0.0.1")

# HTTP → HTTPS
CODE=$(curl -s -o /dev/null -w '%{http_code}' "${RES[@]}" "http://$DOMAIN:$HTTP/" || true)
LOC=$(curl -s -o /dev/null -w '%{redirect_url}' "${RES[@]}" "http://$DOMAIN:$HTTP/" || true)
[[ $CODE == 301 && $LOC == https://$DOMAIN* ]] && ok "HTTP → HTTPS (301)" || fail "HTTP → HTTPS: код $CODE, куди: $LOC"

# Шлях Let's Encrypt без редіректу (інакше сертифікат не продовжиться)
CODE=$(curl -s -o /dev/null -w '%{http_code}' "${RES[@]}" "http://$DOMAIN:$HTTP/.well-known/acme-challenge/ci-test" || true)
[[ $CODE == 404 ]] && ok "шлях Let's Encrypt без редіректу (404)" || fail "шлях Let's Encrypt: код $CODE, очікувався 404"

# HTTPS віддає сторінку
CODE=$(curl -sk -o /dev/null -w '%{http_code}' "${RES[@]}" "https://$DOMAIN:$HTTPS/" || true)
[[ $CODE == 200 ]] && ok "HTTPS віддає сторінку (200)" || fail "HTTPS: код $CODE"

# Будь-яка адреса React → index.html (try_files)
CODE=$(curl -sk -o /dev/null -w '%{http_code}' "${RES[@]}" "https://$DOMAIN:$HTTPS/catalog/ci-test" || true)
[[ $CODE == 200 ]] && ok "маршрути React (try_files)" || fail "маршрути React: код $CODE"

# HTTP/2
VER=$(curl -sk --http2 -o /dev/null -w '%{http_version}' "${RES[@]}" "https://$DOMAIN:$HTTPS/" || true)
[[ $VER == 2 ]] && ok "HTTP/2" || fail "HTTP/2: версія $VER"

# Заголовки безпеки і прихована версія nginx
HEADERS=$(curl -skI "${RES[@]}" "https://$DOMAIN:$HTTPS/" | tr -d '\r' || true)
for H in Strict-Transport-Security Content-Security-Policy X-Frame-Options X-Content-Type-Options Referrer-Policy Permissions-Policy; do
    grep -qi "^$H:" <<<"$HEADERS" && ok "заголовок $H" || fail "немає заголовка $H"
done
SERVER=$(grep -i '^server:' <<<"$HEADERS" | cut -d' ' -f2- || true)
[[ $SERVER == nginx ]] && ok "версія nginx прихована" || fail "заголовок Server: $SERVER"

if [[ $FAILED != 0 ]]; then
    echo "Логи nginx:"; docker logs "$NAME" 2>&1 | tail -20
    echo "РЕЗУЛЬТАТ: є помилки"; exit 1
fi
echo "РЕЗУЛЬТАТ: усе ок"
