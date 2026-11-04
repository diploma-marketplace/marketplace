# База даних на сервері (Azure)

Сервер бази: `pg-marketplace-diploma` (Azure Database for PostgreSQL 17). З інтернету недоступний, підключення лише з сервера `vm-marketplace`.

## Ролі

| Роль | Хто під нею працює | Що може |
| --- | --- | --- |
| `mpadmin` | адмін інфраструктури | адмін сервера бази; застосунок під ним не ходить |
| `marketplace_owner` | крок міграцій при викладці (CI/CD) | власник бази `marketplace`: створює і змінює таблиці |
| `marketplace_app` | backend | лише читає і змінює дані (`SELECT`, `INSERT`, `UPDATE`, `DELETE`) |

Права на нові таблиці `marketplace_app` отримує автоматично, але тільки на ті, які створив `marketplace_owner`. Тому міграції на сервері — завжди під `marketplace_owner`.

Ролі описані в [roles.sql](roles.sql). Цей самий файл використовує CI, щоб перевіряти ту саму схему, що на сервері. Локальна база для розробки влаштована простіше — [dev/README.md](../../dev/README.md).

## Клієнт PostgreSQL на сервері

Версія клієнта має бути 17, як у бази: `pg_dump` старішої версії з новішою базою не працює. Один раз на новому сервері:

```bash
sudo apt update && sudo apt install -y postgresql-common
sudo /usr/share/postgresql-common/pgdg/apt.postgresql.org.sh -y
sudo apt install -y postgresql-client-17
```

Підключення до бази — завжди з `sslmode=verify-full sslrootcert=system`: дані шифруються, а сертифікат бази перевіряється, тож підмінити базу не вийде.

## Перше налаштування

Один раз, **до першої міграції**: права за замовчуванням діють лише на таблиці, створені після них.

На сервері `vm-marketplace`, з папки `/opt/marketplace`:

```bash
# 1. Ролі та база (запитає пароль mpadmin)
psql "host=pg-marketplace-diploma.postgres.database.azure.com dbname=postgres user=mpadmin sslmode=verify-full sslrootcert=system" \
  -f infra/db/roles.sql

# 2. Паролі ролей: вводяться приховано, у файлах та історії команд не залишаються
psql "host=pg-marketplace-diploma.postgres.database.azure.com dbname=marketplace user=mpadmin sslmode=verify-full sslrootcert=system"
```

У консолі psql:

```
\password marketplace_owner
\password marketplace_app
\q
```

Паролі — у менеджер паролів, далі в GitHub Secrets (для CI/CD) і `.env` на сервері. У репозиторій — ніколи.

## Перевірка

```bash
psql "host=pg-marketplace-diploma.postgres.database.azure.com dbname=marketplace user=mpadmin sslmode=verify-full sslrootcert=system" \
  -c '\du marketplace_*' -c '\ddp'
```

`\du` — обидві ролі без прав адміна, `\ddp` — права за замовчуванням для `marketplace_app` на таблиці й послідовності.

## Розширення PostgreSQL

В Azure кожне розширення спершу дозволяється в налаштуваннях сервера бази (параметр `azure.extensions`), лише потім його можна створити міграцією.

## Якщо щось пішло не так

Скрипт зупиняється на першій помилці. Якщо ролі й база вже створені, повторно його не запускаємо.

Почати з нуля можна **лише поки в базі немає даних** — команда видаляє базу назавжди. Виконується під `mpadmin`, підключившись до бази `postgres`:

```sql
DROP DATABASE marketplace;
DROP ROLE marketplace_app;
DROP ROLE marketplace_owner;
```
