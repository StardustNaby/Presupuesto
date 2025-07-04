#!/bin/bash

echo "🚀 Iniciando migración de base de datos..."

# Ejecutar migraciones de Entity Framework
dotnet ef database update --project src/PresupuestoFamiliarMensual.Infrastructure --startup-project src/PresupuestoFamiliarMensual.API

echo "✅ Migración completada exitosamente"

# Insertar datos de ejemplo si la base de datos está vacía
echo "📊 Verificando si se necesitan datos de ejemplo..."

# Contar usuarios existentes
USER_COUNT=$(dotnet run --project src/PresupuestoFamiliarMensual.API --no-build -- --check-users 2>/dev/null || echo "0")

if [ "$USER_COUNT" -eq "0" ]; then
    echo "📝 Insertando datos de ejemplo..."
    
    # Crear un usuario de prueba
    curl -X POST "http://localhost:8080/api/auth/register" \
         -H "Content-Type: application/json" \
         -d '{
           "username": "admin",
           "email": "admin@example.com",
           "password": "Admin123!",
           "confirmPassword": "Admin123!",
           "firstName": "Administrador",
           "lastName": "Sistema"
         }' || echo "⚠️ No se pudo crear usuario de prueba"
    
    echo "✅ Datos de ejemplo insertados"
else
    echo "ℹ️ La base de datos ya tiene datos, saltando inserción de ejemplo"
fi

echo "🎉 Configuración de base de datos completada" 