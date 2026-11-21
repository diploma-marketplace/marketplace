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

Збірку коду перевіряє [.github/workflows/build.yml](../.github/workflows/build.yml): backend — збірка Docker-образу з `backend/FutureShopBackend/Dockerfile`, frontend — `npm ci`, лінтер і збірка на версії Node з `frontend/.nvmrc`. Локально, з кореня репозиторію:

```bash
docker build -f backend/FutureShopBackend/Dockerfile -t marketplace-backend:ci backend
cd frontend && npm ci && npm run lint && npm run build
```

Далі в тій самій перевірці backend запускається з базою, як при викладці: PostgreSQL, налаштований як Azure, ролі з `infra/db/roles.sql`, міграції самодостатнім пакетом (EF bundle) під `marketplace_owner`, backend з Redis під `marketplace_app`. Перевіряється, що міграції застосовуються, `marketplace_app` має права на всі нові таблиці й послідовності, backend працює не від root і `/api/health` бачить базу. Локально:

```bash
bash infra/ci/test-backend.sh
```

Версії образів скрипти не вписують окремо, а беруть із `docker-compose.yml`, `dev/docker-compose.yml` і Dockerfile: оновили версію там — перевірки підхопили її самі.

## Оновлення версій (Dependabot)

Версії закріплені (у npm точні версії фіксує `package-lock.json`) і змінюються лише через PR. Раз на місяць, 1-го числа, Dependabot ([.github/dependabot.yml](../.github/dependabot.yml)) сам відкриває такі PR для GitHub Actions, образів Docker, NuGet і npm: дрібні оновлення одним PR на кожен вид, великі (лише Actions і Docker) — кожне окремо. Їх перевіряють ті самі автоперевірки й ревʼю, що й звичайні PR.

Обмеження: PostgreSQL лише в межах 17 (як в Azure), .NET лише в межах 10 (LTS), Nginx лише patch стабільної гілки. NuGet і npm — лише patch (виправлення помилок), не раніше ніж через тиждень після виходу; більші оновлення програмісти роблять самі, коли вирішать.

Після злиття PR з новим образом на сервері недостатньо `git pull`, контейнер треба перестворити (`/opt/marketplace`):

```bash
sudo git pull --ff-only
sudo docker compose pull
sudo docker compose up -d
```

Certbot окремо оновлювати не треба: таймер запускає його через `docker compose run`, і нова версія підтягнеться сама.

## База даних

Ролі, права і перше налаштування бази в Azure — [db/README.md](db/README.md).
