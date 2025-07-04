# Usar la imagen oficial de .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

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

# Compilar y publicar en un solo paso
WORKDIR "/src/src/PresupuestoFamiliarMensual.API"
RUN dotnet publish "PresupuestoFamiliarMensual.API.csproj" -c Release -o /app/publish

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Crear usuario no-root
RUN adduser -D -s /bin/sh appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "PresupuestoFamiliarMensual.API.dll"] 