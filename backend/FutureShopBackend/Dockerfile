# Контекст збірки — папка backend/ (бо API посилається на сусідні проєкти).
# Збірка: docker compose -f dev/docker-compose.yml --profile api up -d --build

FROM mcr.microsoft.com/dotnet/aspnet:10.0.15 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0.404 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Спершу лише .csproj, щоб restore кешувався між збірками
COPY FutureShopBackend/FutureShopBackend.Api.csproj FutureShopBackend/
COPY FutureShopBackend.Application/FutureShopBackend.Application.csproj FutureShopBackend.Application/
COPY FutureShopBackend.Domain/FutureShopBackend.Domain.csproj FutureShopBackend.Domain/
COPY FutureShopBackend.Infrastructure/FutureShopBackend.Infrastructure.csproj FutureShopBackend.Infrastructure/
RUN dotnet restore FutureShopBackend/FutureShopBackend.Api.csproj
COPY . .
RUN dotnet publish FutureShopBackend/FutureShopBackend.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FutureShopBackend.Api.dll"]
