#!/usr/bin/env bash
# Backend разом з базою, як буде на сервері: PostgreSQL налаштований як Azure, ролі з infra/db/roles.sql,
# міграції — самодостатнім пакетом (EF bundle) під marketplace_owner, backend — під marketplace_app з Redis.
# Перевіряє: міграції застосовуються, marketplace_app має права на всі нові таблиці,
# backend стартує не від root і /api/health бачить базу.
# Запуск з кореня репозиторію (потрібен Docker): bash infra/ci/test-backend.sh
set -euo pipefail
cd "$(dirname "$0")/../.."

PG_IMAGE=$(docker compose -f dev/docker-compose.yml config --images | grep '^postgres:')      # як у dev/ і в Azure
REDIS_IMAGE=$(docker compose -f dev/docker-compose.yml config --images | grep '^redis:')     # як у dev/
SDK_IMAGE=$(sed -n 's/^FROM \(mcr\.microsoft\.com\/dotnet\/sdk:[^ ]*\).*/\1/p' backend/FutureShopBackend/Dockerfile)  # як у Dockerfile
APP_IMAGE=marketplace-backend:ci
ID=$$
NET=ci-backend-$ID
DB=ci-backend-db-$ID
REDIS=ci-backend-redis-$ID
APP=ci-backend-app-$ID
PORT=18081
FAILED=0

cleanup() {
    docker rm -f "$APP" "$REDIS" "$DB" >/dev/null 2>&1 || true
    docker network rm "$NET" >/dev/null 2>&1 || true
}
trap cleanup EXIT

ok()   { echo "  ✓ $1"; }
fail() { echo "  ✗ $1"; FAILED=1; }
# psql у контейнері бази від суперкористувача — лише для підготовки і перевірок, backend так не ходить
sql()  { docker exec -i "$DB" psql -U postgres -v ON_ERROR_STOP=1 -qAtX "$@"; }

echo "Образи: $PG_IMAGE, $REDIS_IMAGE, $SDK_IMAGE"

echo "1. Образ backend"
docker build -q -f backend/FutureShopBackend/Dockerfile -t "$APP_IMAGE" backend >/dev/null
ok "зібрано з Dockerfile"

echo "2. База як в Azure і ролі"
docker network create "$NET" >/dev/null
# Паролі тут лише для тимчасової тестової бази, не секрет
docker run -d --name "$DB" --network "$NET" --network-alias db -e POSTGRES_PASSWORD=ci_superuser \
    -v "$PWD/infra/db:/sql:ro" "$PG_IMAGE" >/dev/null
docker run -d --name "$REDIS" --network "$NET" --network-alias redis "$REDIS_IMAGE" >/dev/null
for _ in $(seq 1 60); do
    docker exec "$DB" pg_isready -h 127.0.0.1 -U postgres >/dev/null 2>&1 && break
    sleep 1
done
sql -d postgres -c "CREATE ROLE azure_pg_admin NOLOGIN" \
    -c "CREATE ROLE mpadmin LOGIN CREATEROLE CREATEDB PASSWORD 'ci_mpadmin'" \
    -c "GRANT azure_pg_admin TO mpadmin"
sql -d template1 -c "ALTER SCHEMA public OWNER TO azure_pg_admin"
docker exec -i "$DB" psql -U mpadmin -d postgres -v ON_ERROR_STOP=1 -qAtX -f /sql/roles.sql
sql -d postgres -c "ALTER ROLE marketplace_owner PASSWORD 'ci_owner'" \
    -c "ALTER ROLE marketplace_app PASSWORD 'ci_app'"
ok "roles.sql під адміном без суперкористувача"

echo "3. Міграції під marketplace_owner (EF bundle)"
# Код копіюється всередину контейнера без bin/obj: у робочу папку нічого не пишеться
if OUT=$(docker run --rm --network "$NET" -v "$PWD/backend:/src:ro" \
        -e CONN="Host=db;Port=5432;Database=marketplace;Username=marketplace_owner;Password=ci_owner" \
        "$SDK_IMAGE" bash -euo pipefail -c '
            mkdir /work
            tar -C /src --exclude=bin --exclude=obj -cf - . | tar -C /work -xf -
            cd /work/FutureShopBackend
            dotnet tool restore
            dotnet restore FutureShopBackend.Api.csproj
            dotnet ef migrations bundle \
                --project ../FutureShopBackend.Infrastructure/FutureShopBackend.Infrastructure.csproj \
                --startup-project FutureShopBackend.Api.csproj \
                --self-contained -r linux-x64 -o /tmp/efbundle
            /tmp/efbundle --connection "$CONN"
        ' 2>&1); then
    ok "міграції застосовано"
else
    echo "$OUT" | tail -40
    fail "міграції не застосувались"
    echo "РЕЗУЛЬТАТ: є помилки"; exit 1
fi

echo "4. Права на нові таблиці"
TABLES=$(sql -d marketplace -c "SELECT count(*) FROM pg_tables WHERE schemaname = 'public'")
[[ $TABLES -gt 0 ]] && ok "таблиць створено: $TABLES" || fail "міграції не створили жодної таблиці"
NOT_OWNER=$(sql -d marketplace -c "SELECT string_agg(tablename, ', ') FROM pg_tables
    WHERE schemaname = 'public' AND tableowner <> 'marketplace_owner'")
[[ -z $NOT_OWNER ]] && ok "усі таблиці належать marketplace_owner" || fail "не marketplace_owner: $NOT_OWNER"
# Кожне право окремо: has_table_privilege зі списком прав повертає true, якщо є хоч одне
NO_DML=$(sql -d marketplace -c "SELECT string_agg(tablename, ', ') FROM pg_tables
    CROSS JOIN LATERAL (SELECT format('%I.%I', schemaname, tablename) AS t) n
    WHERE schemaname = 'public'
      AND NOT (has_table_privilege('marketplace_app', n.t, 'SELECT') AND has_table_privilege('marketplace_app', n.t, 'INSERT')
           AND has_table_privilege('marketplace_app', n.t, 'UPDATE') AND has_table_privilege('marketplace_app', n.t, 'DELETE'))")
[[ -z $NO_DML ]] && ok "marketplace_app читає і змінює дані в усіх таблицях" || fail "marketplace_app без прав: $NO_DML"
NO_SEQ=$(sql -d marketplace -c "SELECT string_agg(sequencename, ', ') FROM pg_sequences
    WHERE schemaname = 'public'
      AND NOT has_sequence_privilege('marketplace_app', format('%I.%I', schemaname, sequencename), 'USAGE')")
[[ -z $NO_SEQ ]] && ok "marketplace_app бере номери з усіх послідовностей" || fail "послідовності без прав: $NO_SEQ"

echo "5. Backend під marketplace_app"
docker run -d --name "$APP" --network "$NET" -p "127.0.0.1:$PORT:8080" \
    -e ASPNETCORE_ENVIRONMENT=Production \
    -e ASPNETCORE_HTTP_PORTS=8080 \
    -e ConnectionStrings__Default="Host=db;Port=5432;Database=marketplace;Username=marketplace_app;Password=ci_app" \
    -e ConnectionStrings__Redis=redis:6379 \
    "$APP_IMAGE" >/dev/null
CODE=000
for _ in $(seq 1 60); do
    CODE=$(curl -s --noproxy '*' -o /dev/null -w '%{http_code}' "http://127.0.0.1:$PORT/api/health" || true)
    [[ $CODE != 000 ]] && break
    sleep 1
done
BODY=$(curl -s --noproxy '*' "http://127.0.0.1:$PORT/api/health" || true)
[[ $CODE == 200 && $BODY == *'"database":"up"'* ]] && ok "/api/health: 200, база доступна" \
    || fail "/api/health: код $CODE, відповідь: $BODY"
UID_IN=$(docker exec "$APP" id -u 2>/dev/null || echo "?")
[[ $UID_IN != 0 && $UID_IN != "?" ]] && ok "backend працює не від root (uid $UID_IN)" || fail "backend від root або не запущений (uid $UID_IN)"

if [[ $FAILED != 0 ]]; then
    echo "Логи backend:"; docker logs "$APP" 2>&1 | tail -30
    echo "РЕЗУЛЬТАТ: є помилки"; exit 1
fi
echo "РЕЗУЛЬТАТ: усе ок"
