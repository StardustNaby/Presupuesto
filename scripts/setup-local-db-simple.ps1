# Script para configurar PostgreSQL local con Docker
Write-Host "Configurando PostgreSQL local..." -ForegroundColor Green

# Verificar si Docker esta instalado
try {
    docker --version | Out-Null
    Write-Host "Docker encontrado" -ForegroundColor Green
} catch {
    Write-Host "Docker no esta instalado. Por favor instala Docker Desktop desde: https://www.docker.com/products/docker-desktop/" -ForegroundColor Red
    exit 1
}

# Detener contenedor existente si existe
Write-Host "Deteniendo contenedor PostgreSQL existente..." -ForegroundColor Yellow
docker stop presupuesto-postgres 2>$null
docker rm presupuesto-postgres 2>$null

# Crear y ejecutar contenedor PostgreSQL
Write-Host "Iniciando PostgreSQL..." -ForegroundColor Green
docker run -d --name presupuesto-postgres -e POSTGRES_DB=presupuesto_familiar -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres123 -p 5432:5432 postgres:15

# Esperar a que PostgreSQL este listo
Write-Host "Esperando a que PostgreSQL este listo..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Verificar que el contenedor este ejecutandose
$containerStatus = docker ps --filter "name=presupuesto-postgres" --format "table {{.Names}}\t{{.Status}}"
Write-Host "Estado del contenedor:" -ForegroundColor Cyan
Write-Host $containerStatus

Write-Host "PostgreSQL configurado exitosamente!" -ForegroundColor Green
Write-Host "Informacion de conexion:" -ForegroundColor Cyan
Write-Host "   Host: localhost" -ForegroundColor White
Write-Host "   Puerto: 5432" -ForegroundColor White
Write-Host "   Base de datos: presupuesto_familiar" -ForegroundColor White
Write-Host "   Usuario: postgres" -ForegroundColor White
Write-Host "   Contrasena: postgres123" -ForegroundColor White
Write-Host ""
Write-Host "Cadena de conexion para DBeaver:" -ForegroundColor Cyan
Write-Host "   postgresql://postgres:postgres123@localhost:5432/presupuesto_familiar" -ForegroundColor White
Write-Host ""
Write-Host "Para detener PostgreSQL: docker stop presupuesto-postgres" -ForegroundColor Yellow
Write-Host "Para iniciar PostgreSQL: docker start presupuesto-postgres" -ForegroundColor Yellow 