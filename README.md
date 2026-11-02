# marketplace

Дипломний проєкт: маркетплейс у стилі Rozetka, де кожен користувач може бути і покупцем, і продавцем.

## Стек

| Частина | Технологія |
| --- | --- |
| Backend | .NET |
| Frontend | React + TypeScript |
| База даних | PostgreSQL |
| Запуск | Docker + Docker Compose |
| Вхід на сайт | Nginx (reverse proxy, HTTPS) |
| CI/CD | GitHub Actions |
| Сервер | Microsoft Azure, Ubuntu |

## Структура

```
backend/            серверна частина (.NET)
frontend/           клієнтська частина (React)
infra/nginx/        налаштування Nginx
infra/scripts/      скрипти для сервера: сертифікат, захист, далі деплой і бекапи
infra/systemd/      таймер автопродовження сертифіката
dev/                локальна база для розробки
.github/workflows/  конвеєри CI/CD
```

## Запуск

- **Локальна розробка:** база PostgreSQL у Docker — [dev/README.md](dev/README.md).
- **Сервер:** порядок першого запуску — [infra/README.md](infra/README.md).

## Як працювати з репозиторієм

Правила роботи з гілками, Pull Request і секретами: [CONTRIBUTING.md](CONTRIBUTING.md).
