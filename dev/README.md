# Локальна база для розробки

Своя PostgreSQL 17 на вашому комп'ютері в Docker. Це пісочниця: можна ламати, чистити, починати заново — нікому не заважає. З базою в Azure вона ніяк не пов'язана, реальні дані сюди не потрапляють.

## Що потрібно

- **Windows / macOS:** [Docker Desktop](https://www.docker.com/products/docker-desktop/). На Windows — з WSL 2 (Docker Desktop пропонує це при встановленні).
- **Linux:** Docker Engine і плагін Compose.

Перевірка: `docker compose version` показує версію.

## Запуск

Усі команди — з кореня репозиторію. Вони однакові в PowerShell, Terminal на macOS і bash на Linux.

```
docker compose -f dev/docker-compose.yml up -d
```

Перший раз Docker завантажить образ PostgreSQL і створить користувача та базу. Перевірити, що база готова:

```
docker compose -f dev/docker-compose.yml ps
```

У колонці `STATUS` має бути `healthy`.

## Підключення

| Параметр | Значення |
| --- | --- |
| Host | `localhost` |
| Port | `5432` (або ваш `DB_PORT`) |
| Database | `marketplace` |
| User | `marketplace_app` |
| Password | `marketplace_dev` |

Рядок підключення для .NET (Npgsql):

```
Host=localhost;Port=5432;Database=marketplace;Username=marketplace_app;Password=marketplace_dev
```

Ці ж параметри підходять для DBeaver, pgAdmin, Rider, DataGrip. На сервері backend підключається до бази в Azure під `marketplace_app`, але з правами лише на дані (див. нижче), рядок підключення там береться з GitHub Secrets.

## Користувачі та права

Локально все спрощено: один користувач `marketplace_app` — власник бази `marketplace`. Під ним працює і застосунок, і міграції.

| Користувач | Що може |
| --- | --- |
| `marketplace_app` | власник бази `marketplace`: таблиці, міграції, дані. Інші бази і користувачі — ні |
| `postgres` | адмін, лише для обслуговування |

На сервері в Azure ролі розділені: міграції застосовує окремий крок викладки під `marketplace_owner`, а застосунок працює під `marketplace_app` і може лише читати й змінювати дані. Що застосунку вистачає прав, перевіряє CI: там база з такими ж ролями, як у Azure.

### Розширення PostgreSQL

«Довірені» розширення (`pg_trgm` для пошуку по тексту, `citext`, `uuid-ossp`, `pgcrypto` та інші) локально створяться і під `marketplace_app`. Але в Azure кожне розширення спершу треба дозволити в налаштуваннях сервера, інакше міграція з `CREATE EXTENSION` на сервері впаде.

Тому: потрібне розширення — **спершу напишіть адмінам**, ми дозволимо його в Azure, і тільки потім додавайте його в міграцію.

## Якщо порт 5432 зайнятий

Помилка `port is already allocated` або `address already in use` означає, що на комп'ютері вже працює свій PostgreSQL (або інша програма на цьому порту). Свій PostgreSQL чіпати не треба:

1. Скопіювати приклад налаштувань (у ньому вже порт `5433`):
   - Windows (PowerShell): `Copy-Item dev/.env.example dev/.env`
   - macOS / Linux: `cp dev/.env.example dev/.env`
2. Знову `docker compose -f dev/docker-compose.yml up -d`, а в рядку підключення — `Port=5433`.

Зайнятий і `5433` — поставте в `dev/.env` будь-який вільний, наприклад `5434`. `dev/.env` у Git не потрапляє, у кожного свій.

## Щоденні команди

Усі — з кореня репозиторію, з префіксом `docker compose -f dev/docker-compose.yml`:

| Що | Команда після префікса |
| --- | --- |
| Запустити | `up -d` |
| Зупинити (дані зберігаються) | `stop` |
| Подивитися логи | `logs -f db` |
| Консоль під користувачем застосунку | `exec db psql -U marketplace_app -d marketplace` |
| Консоль адміна (розширення, розбір проблем) | `exec db psql -U postgres -d marketplace` |
| Стерти базу й почати з нуля | `down -v` |

Після перезавантаження комп'ютера база сама не стартує — знову `up -d`.

## Важливо

- Користувач і база створюються лише при першому запуску, на порожній базі. Якщо запускали раніше стару версію — один раз `down -v`, потім `up -d`.
- Таблиці створюються міграціями з коду, руками не створюємо. Тоді структура однакова у всіх і на сервері.
- Міграції локально застосовуєте самі: `dotnet ef database update`. У коді `Database.Migrate()` при старті не викликаємо — на сервері міграції застосовує окремий крок викладки.
- Паролі `marketplace_dev` і `postgres_dev` — лише для локальної бази, не секрет. Справжні паролі — тільки в GitHub Secrets і `.env` на сервері.
- Порт відкритий лише для вашого комп'ютера (`127.0.0.1`): з мережі до бази не дістатися.
- Дані лежать у Docker-томі `marketplace-dev_db-data`, не в репозиторії. `down -v` видаляє їх назавжди.
