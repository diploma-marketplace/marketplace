-- Виконується один раз, при першому запуску (коли база ще порожня).
-- Локально спрощено: marketplace_app — власник бази marketplace, тож і застосунок,
-- і міграції (dotnet ef database update) працюють під одним користувачем.
-- На сервері ролі розділені (marketplace_owner / marketplace_app) — це перевіряє CI.
-- marketplace_app не може чіпати інші бази і створювати користувачів. Про розширення — dev/README.md.
CREATE ROLE marketplace_app LOGIN PASSWORD 'marketplace_dev' NOSUPERUSER NOCREATEDB NOCREATEROLE;
CREATE DATABASE marketplace OWNER marketplace_app;
