@echo off
echo Ejecutando migraciones en PostgreSQL local...

set DATABASE_URL=postgresql://postgres:postgres123@localhost:5432/presupuesto_familiar
echo Usando base de datos: %DATABASE_URL%

cd src\PresupuestoFamiliarMensual.API

echo Ejecutando migraciones...
dotnet ef database update

if %errorlevel% equ 0 (
    echo Migraciones ejecutadas correctamente!
) else (
    echo Error ejecutando migraciones
)

cd ..\..

echo.
echo Para probar la API localmente:
echo   cd src\PresupuestoFamiliarMensual.API
echo   dotnet run
echo   La API estara disponible en: http://localhost:5000
echo.
pause 