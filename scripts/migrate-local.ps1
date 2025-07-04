# Script para ejecutar migraciones en PostgreSQL local
Write-Host "🔄 Ejecutando migraciones en PostgreSQL local..." -ForegroundColor Green

# Configurar variable de entorno para la base de datos local
$env:DATABASE_URL = "postgresql://postgres:postgres123@localhost:5432/presupuesto_familiar"

Write-Host "🔗 Usando base de datos: $env:DATABASE_URL" -ForegroundColor Cyan

# Navegar al directorio de la API
Set-Location "src/PresupuestoFamiliarMensual.API"

# Ejecutar migraciones
Write-Host "🚀 Ejecutando migraciones..." -ForegroundColor Yellow
dotnet ef database update

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Migraciones ejecutadas correctamente!" -ForegroundColor Green
} else {
    Write-Host "❌ Error ejecutando migraciones" -ForegroundColor Red
}

# Volver al directorio raíz
Set-Location "../../"

Write-Host "📋 Para probar la API localmente:" -ForegroundColor Cyan
Write-Host "   cd src/PresupuestoFamiliarMensual.API" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor White
Write-Host "   # La API estará disponible en: http://localhost:5000" -ForegroundColor White 