#!/bin/bash

echo "🧪 Probando aplicación localmente..."

# Verificar que estamos en el directorio correcto
if [ ! -f "PresupuestoFamiliarMensual.sln" ]; then
    echo "❌ Error: No se encontró el archivo de solución. Ejecuta este script desde la raíz del proyecto."
    exit 1
fi

# Limpiar y restaurar
echo "🧹 Limpiando proyecto..."
dotnet clean
dotnet restore

# Compilar
echo "🔨 Compilando proyecto..."
dotnet build -c Release

# Probar health check
echo "🏥 Probando health check..."
cd src/PresupuestoFamiliarMensual.API

# Iniciar aplicación en background
echo "🚀 Iniciando aplicación..."
dotnet run --no-build --urls "http://localhost:5000" &
APP_PID=$!

# Esperar un poco para que inicie
sleep 5

# Probar health check
echo "📡 Probando endpoints..."
curl -s http://localhost:5000/api/health/ping
echo ""
curl -s http://localhost:5000/api/health/simple
echo ""
curl -s http://localhost:5000/api/health
echo ""

# Detener aplicación
echo "🛑 Deteniendo aplicación..."
kill $APP_PID

echo "✅ Prueba completada" 