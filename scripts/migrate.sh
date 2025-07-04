#!/bin/bash

echo "🚀 Iniciando migración de base de datos..."

# Esperar un poco para que la base de datos esté lista
sleep 5

# Ejecutar migraciones de Entity Framework
echo "📊 Ejecutando migraciones..."
dotnet ef database update --project src/PresupuestoFamiliarMensual.Infrastructure --startup-project src/PresupuestoFamiliarMensual.API

if [ $? -eq 0 ]; then
    echo "✅ Migración completada exitosamente"
else
    echo "⚠️ Error en migración, continuando..."
fi

echo "🎉 Configuración de base de datos completada" 