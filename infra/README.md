# Інфраструктура

Сервер: Azure VM `vm-marketplace` (Ubuntu 24.04, Docker), адреса
`diploma-marketplace.denmarkeast.cloudapp.azure.com`. Репозиторій на сервері лежить у `/opt/marketplace`.

## Перший запуск на чистому сервері

Порядок важливий: Nginx не стартує без сертифіката, тому перший сертифікат
отримуємо certbot-ом у режимі `--standalone` (він сам тимчасово слухає порт 80), **до** запуску Nginx.

```bash
# 1. Репозиторій
sudo git clone https://github.com/diploma-marketplace/marketplace.git /opt/marketplace
cd /opt/marketplace

# 2. Захист сервера: firewall UFW (відкриті 22, 80, 443) і fail2ban
sudo bash infra/scripts/harden.sh

# 3. Перший сертифікат (Nginx ще не запущений, порт 80 вільний)
sudo docker compose run --rm -p 80:80 certbot certonly --standalone \
  -d diploma-marketplace.denmarkeast.cloudapp.azure.com \
  --register-unsafely-without-email --agree-tos

# 4. Запуск сайту
sudo docker compose up -d

# 5. Автопродовження сертифіката (таймер systemd)
sudo cp infra/systemd/cert-renew.service infra/systemd/cert-renew.timer /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable --now cert-renew.timer
```

`--agree-tos` — згода з угодою Let's Encrypt. Email не вказуємо: Let's Encrypt більше не надсилає листи про закінчення сертифіката, продовження автоматичне.

## Перевірка

```bash
curl -I http://diploma-marketplace.denmarkeast.cloudapp.azure.com    # 301 → https
curl -I https://diploma-marketplace.denmarkeast.cloudapp.azure.com   # 200
systemctl list-timers cert-renew.timer                               # коли наступний запуск
sudo /opt/marketplace/infra/scripts/renew-cert.sh                    # ручне продовження
```

## Якщо сертифікат втрачено (переїзд, новий сервер)

`sudo docker compose down`, далі кроки 3–5. На новому сервері — усі кроки з першого.

## Автоперевірка (CI)

На кожен PR у `main` GitHub Actions ([.github/workflows/infra.yml](../.github/workflows/infra.yml)) перевіряє:

- синтаксис Docker Compose (сервер і локальна база);
- Nginx: `nginx -t` без попереджень на тому ж образі, редірект на HTTPS, шлях Let's Encrypt, HTTPS, маршрути React, HTTP/2, заголовки безпеки;
- `infra/db/roles.sql` на PostgreSQL, налаштованому як Azure: права `marketplace_owner` і `marketplace_app`.

Ті самі перевірки локально, з кореня репозиторію (потрібен Docker):

```bash
bash infra/ci/test-nginx.sh
bash infra/ci/test-db-roles.sh
```

## База даних

Ролі, права і перше налаштування бази в Azure — [db/README.md](db/README.md).
