# Usar la imagen oficial de .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0

# Usar la imagen de SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/PresupuestoFamiliarMensual.API/PresupuestoFamiliarMensual.API.csproj", "src/PresupuestoFamiliarMensual.API/"]
COPY ["src/PresupuestoFamiliarMensual.Application/PresupuestoFamiliarMensual.Application.csproj", "src/PresupuestoFamiliarMensual.Application/"]
COPY ["src/PresupuestoFamiliarMensual.Core/PresupuestoFamiliarMensual.Core.csproj", "src/PresupuestoFamiliarMensual.Core/"]
COPY ["src/PresupuestoFamiliarMensual.Infrastructure/PresupuestoFamiliarMensual.Infrastructure.csproj", "src/PresupuestoFamiliarMensual.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "src/PresupuestoFamiliarMensual.API/PresupuestoFamiliarMensual.API.csproj"

# Copiar todo el código fuente
COPY . .

# Dar permisos de ejecución al script de migración
RUN chmod +x scripts/migrate.sh

# Compilar y publicar en un solo paso
WORKDIR "/src/src/PresupuestoFamiliarMensual.API"
RUN dotnet publish "PresupuestoFamiliarMensual.API.csproj" -c Release -o /app/publish

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/scripts ./scripts

# Instalar curl para el script de migración
RUN apk add --no-cache curl bash

# Crear usuario no-root
RUN adduser -D -s /bin/sh appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "PresupuestoFamiliarMensual.API.dll"] 