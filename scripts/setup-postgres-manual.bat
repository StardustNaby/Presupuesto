@echo off
echo Configurando PostgreSQL local con Docker...

echo Verificando Docker...
docker --version
if %errorlevel% neq 0 (
    echo ERROR: Docker no esta instalado
    echo Por favor instala Docker Desktop desde: https://www.docker.com/products/docker-desktop/
    pause
    exit /b 1
)

echo Deteniendo contenedor existente...
docker stop presupuesto-postgres 2>nul
docker rm presupuesto-postgres 2>nul

echo Iniciando PostgreSQL...
docker run -d --name presupuesto-postgres -e POSTGRES_DB=presupuesto_familiar -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres123 -p 5432:5432 postgres:15

echo Esperando a que PostgreSQL este listo...
timeout /t 10 /nobreak >nul

echo Verificando estado del contenedor...
docker ps --filter "name=presupuesto-postgres"

echo.
echo PostgreSQL configurado exitosamente!
echo.
echo Informacion de conexion:
echo   Host: localhost
echo   Puerto: 5432
echo   Base de datos: presupuesto_familiar
echo   Usuario: postgres
echo   Contrasena: postgres123
echo.
echo Cadena de conexion para DBeaver:
echo   postgresql://postgres:postgres123@localhost:5432/presupuesto_familiar
echo.
echo Comandos utiles:
echo   Para detener: docker stop presupuesto-postgres
echo   Para iniciar: docker start presupuesto-postgres
echo   Para ver logs: docker logs presupuesto-postgres
echo.
pause 