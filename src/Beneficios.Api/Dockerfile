# Etapa de build
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

# Etapa final (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Instala curl para healthcheck
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Configurações de ambiente
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Copia artefatos publicados
COPY --from=build /app/publish .

# Cria usuário não-root para rodar a aplicação
RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /app/logs \
    && chown -R appuser /app

USER appuser

# Expõe porta
EXPOSE 8080

# Healthcheck
HEALTHCHECK --interval=30s --timeout=5s --start-period=25s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Beneficios.Api.dll"]
