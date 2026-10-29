#!/usr/bin/env bash
# Продовження HTTPS-сертифіката Let's Encrypt і перезавантаження Nginx.
# Запускає таймер systemd двічі на день; certbot продовжує, лише коли лишилось < 30 днів.
set -euo pipefail                # зупинитися на першій помилці
cd "$(dirname "$0")/../.."       # корінь репозиторію, де docker-compose.yml
docker compose run --rm certbot renew --webroot -w /var/www/certbot --quiet
docker compose exec -T nginx nginx -s reload     # підхопити сертифікат без зупинки сайту
