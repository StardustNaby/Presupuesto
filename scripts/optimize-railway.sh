#!/bin/bash

echo "🚀 Optimizando despliegue para Railway..."

# Verificar que estamos en el directorio correcto
if [ ! -f "PresupuestoFamiliarMensual.sln" ]; then
    echo "❌ Error: No se encontró el archivo de solución. Ejecuta este script desde la raíz del proyecto."
    exit 1
fi

# Limpiar archivos de compilación
echo "🧹 Limpiando archivos de compilación..."
dotnet clean
rm -rf src/*/bin src/*/obj

# Restaurar dependencias
echo "📦 Restaurando dependencias..."
dotnet restore

# Verificar configuración de Railway
echo "🔍 Verificando configuración de Railway..."
if [ ! -f "railway.json" ]; then
    echo "❌ Error: No se encontró railway.json"
    exit 1
fi

if [ ! -f "Dockerfile" ]; then
    echo "❌ Error: No se encontró Dockerfile"
    exit 1
fi

# Verificar health check
echo "🏥 Verificando health check..."
if ! grep -q "healthcheckPath" railway.json; then
    echo "⚠️ Advertencia: No se encontró healthcheckPath en railway.json"
fi

echo "✅ Optimización completada. Listo para desplegar en Railway."
echo ""
echo "📋 Pasos para desplegar:"
echo "1. Asegúrate de tener Railway CLI instalado"
echo "2. Ejecuta: railway login"
echo "3. Ejecuta: railway up"
echo ""
echo "🔧 Configuraciones optimizadas:"
echo "- Health check timeout reducido a 60 segundos"
echo "- Swagger deshabilitado en producción"
echo "- Dockerfile optimizado con variables de entorno"
echo "- .dockerignore configurado para excluir archivos innecesarios" 