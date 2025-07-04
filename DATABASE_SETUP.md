# 🗄️ Configuración de Base de Datos PostgreSQL

## 📋 Opciones de Configuración

### Opción 1: PostgreSQL Local con Docker (Recomendado)

#### 1. Instalar Docker Desktop
- Descarga desde: https://www.docker.com/products/docker-desktop/
- Instala y reinicia tu computadora

#### 2. Configurar PostgreSQL Local
```powershell
# Ejecutar el script de configuración
.\scripts\setup-local-db.ps1
```

#### 3. Conectar con DBeaver
1. Abrir DBeaver
2. Crear nueva conexión PostgreSQL
3. Configuración:
   - **Host**: localhost
   - **Puerto**: 5432
   - **Base de datos**: presupuesto_familiar
   - **Usuario**: postgres
   - **Contraseña**: postgres123
   - **SSL**: Use SSL (marcado), Require (marcado), Trust Server Certificate (marcado)

#### 4. Ejecutar Migraciones
```powershell
# Ejecutar migraciones en la base local
.\scripts\migrate-local.ps1
```

#### 5. Probar API Localmente
```powershell
cd src/PresupuestoFamiliarMensual.API
dotnet run
```

### Opción 2: PostgreSQL Local Directo

#### 1. Instalar PostgreSQL
- Descarga desde: https://www.postgresql.org/download/windows/
- Instala con usuario: postgres, contraseña: postgres123
- Puerto: 5432

#### 2. Crear Base de Datos
```sql
CREATE DATABASE presupuesto_familiar;
```

#### 3. Conectar con DBeaver
- Misma configuración que arriba

## 🔗 Conectar a Railway

### Método 1: Usar Base de Datos Local como Backup

1. **Crear base de datos en Railway**:
   - Ve a tu proyecto en Railway
   - "New Service" → "Database" → "PostgreSQL"
   - Railway configurará automáticamente `DATABASE_URL`

2. **Migrar datos locales a Railway**:
   ```powershell
   # Exportar datos locales
   pg_dump -h localhost -U postgres -d presupuesto_familiar > backup_local.sql
   
   # Importar a Railway (usar DATABASE_URL de Railway)
   psql "tu_DATABASE_URL_de_railway" < backup_local.sql
   ```

### Método 2: Usar Railway como Principal

1. **Crear base de datos en Railway**
2. **Configurar variable de entorno**:
   - Railway automáticamente conecta tu API con la base de datos
   - La variable `DATABASE_URL` se configura automáticamente

3. **Ejecutar migraciones en Railway**:
   - Las migraciones se ejecutan automáticamente al desplegar
   - O manualmente en Railway CLI:
   ```bash
   railway run dotnet ef database update
   ```

## 📊 Verificar Conexión

### En DBeaver
1. Conectar a la base de datos
2. Ejecutar: `SELECT version();`
3. Verificar tablas: `\dt`

### En la API
1. Endpoint de salud: `GET /health`
2. Endpoint de diagnóstico: `GET /api/health/database`

## 🔧 Comandos Útiles

### Docker
```bash
# Iniciar PostgreSQL
docker start presupuesto-postgres

# Detener PostgreSQL
docker stop presupuesto-postgres

# Ver logs
docker logs presupuesto-postgres

# Eliminar contenedor
docker rm -f presupuesto-postgres
```

### Migraciones
```bash
# Crear migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Revertir migración
dotnet ef database update NombreMigracionAnterior
```

### Base de Datos
```bash
# Conectar a PostgreSQL local
psql -h localhost -U postgres -d presupuesto_familiar

# Listar tablas
\dt

# Ver estructura de tabla
\d "NombreTabla"
```

## 🚨 Solución de Problemas

### Error: "connection refused"
- Verificar que PostgreSQL esté ejecutándose
- Verificar puerto 5432
- Verificar firewall

### Error: "authentication failed"
- Verificar usuario y contraseña
- Verificar configuración de SSL

### Error: "database does not exist"
- Crear la base de datos: `CREATE DATABASE presupuesto_familiar;`

### Error: "migration already applied"
- Verificar tabla `__EFMigrationsHistory`
- Eliminar migraciones duplicadas si es necesario 