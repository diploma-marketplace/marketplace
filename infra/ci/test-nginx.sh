#!/usr/bin/env bash
# Перевірка образу сайту і конфігу Nginx: образ збирається з frontend/Dockerfile, як на сервері (Node як у .nvmrc),
# тимчасовий самопідписаний сертифікат, nginx -t без попереджень і справжні відповіді сайту
# (редірект, Let's Encrypt, HTTPS, зібрана сторінка і її JS, заголовки).
# Запуск з кореня репозиторію (потрібен Docker): bash infra/ci/test-nginx.sh
set -euo pipefail
cd "$(dirname "$0")/../.."

DOMAIN=diploma-marketplace.denmarkeast.cloudapp.azure.com
IMAGE=marketplace-web:ci
NAME=ci-nginx-$$
HTTP=18080
HTTPS=18443
TMP=$(mktemp -d)
FAILED=0

cleanup() { docker rm -f "$NAME" >/dev/null 2>&1 || true; rm -rf "$TMP"; }
trap cleanup EXIT

ok()   { echo "  ✓ $1"; }
fail() { echo "  ✗ $1"; FAILED=1; }

echo "1. Образ сайту (frontend/Dockerfile)"
# Node у Dockerfile має збігатися з .nvmrc: інакше на сервері збирається не тим Node, що в програмістів і в CI
NVMRC=$(tr -d '[:space:]' < frontend/.nvmrc)
NVMRC=${NVMRC#v}
NODE=$(sed -n 's/^FROM node:\([^ ]*\).*/\1/p' frontend/Dockerfile)
NODE=${NODE%%-*}                               # 26.13.1-alpine → 26.13.1
if [[ -n $NODE && ( $NODE == "$NVMRC" || $NODE == "$NVMRC".* ) ]]; then
    ok "Node $NODE відповідає frontend/.nvmrc ($NVMRC)"
else
    fail "Node у frontend/Dockerfile: '${NODE}', у frontend/.nvmrc: '$NVMRC' — має бути однаково"
fi
if OUT=$(docker build -q -t "$IMAGE" frontend 2>&1); then
    ok "зібрано"
else
    echo "$OUT" | tail -40; fail "образ сайту не зібрався"
    echo "РЕЗУЛЬТАТ: є помилки"; exit 1
fi

# Тимчасовий сертифікат там, де nginx чекає справжній від Let's Encrypt
mkdir -p "$TMP/letsencrypt/live/$DOMAIN"
openssl req -x509 -newkey rsa:2048 -nodes -days 1 -subj "/CN=$DOMAIN" \
    -keyout "$TMP/letsencrypt/live/$DOMAIN/privkey.pem" \
    -out "$TMP/letsencrypt/live/$DOMAIN/fullchain.pem" 2>/dev/null
chmod -R a+rX "$TMP/letsencrypt"

VOLUMES=(
    -v "$PWD/infra/nginx/conf.d:/etc/nginx/conf.d:ro"
    -v "$TMP/letsencrypt:/etc/letsencrypt:ro"
)

echo "2. nginx -t"
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

echo "3. Відповіді сайту"
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

# HTTPS віддає сторінку зі збірки frontend, а не заглушку
CODE=$(curl -sk -o /dev/null -w '%{http_code}' "${RES[@]}" "https://$DOMAIN:$HTTPS/" || true)
PAGE=$(curl -sk "${RES[@]}" "https://$DOMAIN:$HTTPS/" || true)
[[ $CODE == 200 && $PAGE == *'id="root"'* ]] && ok "HTTPS віддає зібрану сторінку (200)" || fail "HTTPS: код $CODE, немає зібраної сторінки"

# JS зі збірки: 200 і тип javascript (з іншим типом браузер скрипт не запустить, сторінка буде порожня)
JS=$(grep -o '/assets/[^"]*\.js' <<<"$PAGE" | head -1 || true)
if [[ -n $JS ]]; then
    CODE=$(curl -sk -o /dev/null -w '%{http_code} %{content_type}' "${RES[@]}" "https://$DOMAIN:$HTTPS$JS" || true)
    [[ $CODE == 200*javascript* ]] && ok "JS зібраного сайту (200, javascript)" || fail "JS $JS: $CODE"
else
    fail "у сторінці немає посилання на /assets/*.js"
fi

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
