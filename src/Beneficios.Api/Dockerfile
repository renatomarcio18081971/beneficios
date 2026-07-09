FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Beneficios.Api/Beneficios.Api.csproj", "src/Beneficios.Api/"]
COPY ["src/Beneficios.Application/Beneficios.Application.csproj", "src/Beneficios.Application/"]
COPY ["src/Beneficios.Domain/Beneficios.Domain.csproj", "src/Beneficios.Domain/"]
COPY ["src/Beneficios.Infrastructure/Beneficios.Infrastructure.csproj", "src/Beneficios.Infrastructure/"]

RUN dotnet restore "src/Beneficios.Api/Beneficios.Api.csproj"

COPY src/ ./src/

WORKDIR /src/src/Beneficios.Api
RUN dotnet publish "Beneficios.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=build /app/publish .

RUN mkdir -p /app/logs \
    && chown -R "$APP_UID" /app

USER $APP_UID

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=25s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Beneficios.Api.dll"]