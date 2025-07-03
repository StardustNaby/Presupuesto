# Usar la imagen oficial de .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Usar la imagen de SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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

# Compilar la aplicación
WORKDIR "/src/src/PresupuestoFamiliarMensual.API"
RUN dotnet build "PresupuestoFamiliarMensual.API.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "PresupuestoFamiliarMensual.API.csproj" -c Release -o /app/publish

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear usuario no-root
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "PresupuestoFamiliarMensual.API.dll"] 