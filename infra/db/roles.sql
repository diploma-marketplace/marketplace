-- Ролі та база маркетплейсу. Один файл для Azure і для CI, щоб CI перевіряв ту саму схему.
-- Виконується один раз адміном сервера бази (в Azure — mpadmin), підключеним до бази postgres:
--   psql "host=<сервер> dbname=postgres user=<адмін> sslmode=verify-full sslrootcert=system" -f infra/db/roles.sql
-- Паролів тут немає: їх задають окремо (інструкція — infra/db/README.md).

\set ON_ERROR_STOP on

-- marketplace_owner — власник бази: під ним лише крок міграцій при викладці
-- marketplace_app   — backend: лише читає і змінює дані, структуру не чіпає
CREATE ROLE marketplace_owner LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE;
CREATE ROLE marketplace_app   LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE;

-- В Azure адмін не суперкористувач: щоб створити базу з власником marketplace_owner
-- і задати права від його імені, адмін має бути членом цієї ролі
GRANT marketplace_owner TO CURRENT_USER;

CREATE DATABASE marketplace OWNER marketplace_owner;

\connect marketplace

-- Підключатися до бази можуть лише свої
REVOKE ALL ON DATABASE marketplace FROM PUBLIC;
GRANT CONNECT ON DATABASE marketplace TO marketplace_app;
GRANT USAGE ON SCHEMA public TO marketplace_app;

-- В Azure схемою public володіє azure_pg_admin, а не власник бази,
-- тому право створювати в ній таблиці видаємо окремо
GRANT USAGE, CREATE ON SCHEMA public TO marketplace_owner;

-- Усе, що marketplace_owner створить міграціями, одразу доступне backend-у на читання і запис.
-- Діє лише на нові об'єкти, тому виконується до першої міграції
ALTER DEFAULT PRIVILEGES FOR ROLE marketplace_owner IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO marketplace_app;
ALTER DEFAULT PRIVILEGES FOR ROLE marketplace_owner IN SCHEMA public
    GRANT USAGE, SELECT ON SEQUENCES TO marketplace_app;
