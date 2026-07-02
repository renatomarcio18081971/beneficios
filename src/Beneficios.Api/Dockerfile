FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Beneficios.Api/Beneficios.Api.csproj", "src/Beneficios.Api/"]
COPY ["src/Beneficios.Application/Beneficios.Application.csproj", "src/Beneficios.Application/"]
COPY ["src/Beneficios.Domain/Beneficios.Domain.csproj", "src/Beneficios.Domain/"]
COPY ["src/Beneficios.Infrastructure/Beneficios.Infrastructure.csproj", "src/Beneficios.Infrastructure/"]
RUN dotnet restore "src/Beneficios.Api/Beneficios.Api.csproj"
COPY . .
WORKDIR "/src/src/Beneficios.Api"
RUN dotnet build "Beneficios.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Beneficios.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Beneficios.Api.dll"]
