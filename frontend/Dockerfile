# Образ сайту: Node збирає frontend, у готовий образ ідуть лише Nginx і зібрані файли (без Node і node_modules).
# Конфіг Nginx тут не зашитий: він у infra/nginx/conf.d і підключається на сервері (docker-compose.yml).

# 1. Збірка. Версія Node та сама, що в .nvmrc (перевіряє infra/ci/test-nginx.sh)
FROM node:26.13.1-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY . .
RUN npm run build

# 2. Готовий образ: Nginx і файли з dist/
FROM nginx:1.30.7-alpine
COPY --from=build /app/dist /usr/share/nginx/html
