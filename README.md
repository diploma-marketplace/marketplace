# marketplace

Дипломний проєкт: маркетплейс у стилі Rozetka, де кожен користувач може бути і покупцем, і продавцем.

## Стек

| Частина | Технологія |
| --- | --- |
| Backend | .NET |
| Frontend | React + TypeScript |
| База даних | PostgreSQL |
| Запуск | Docker + docker-compose |
| Вхід на сайт | Nginx (reverse proxy, HTTPS) |
| CI/CD | GitHub Actions |
| Сервер | Microsoft Azure, Ubuntu |

## Структура

```
backend/            серверна частина (.NET)
frontend/           клієнтська частина (React)
infra/nginx/        налаштування Nginx
infra/scripts/      скрипти для сервера: деплой, бекапи
.github/workflows/  конвеєри CI/CD
```

## Запуск

Інструкція з'явиться разом з `docker-compose.yml`. Усе буде запускатися однією командою через Docker, однаково у всіх і на сервері.

## Як працювати з репозиторієм

Правила роботи з гілками, Pull Request і секретами: [CONTRIBUTING.md](CONTRIBUTING.md).
