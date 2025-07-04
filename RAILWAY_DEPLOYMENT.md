# 🚂 Despliegue en Railway

## Configuración del Proyecto

Este proyecto está configurado para desplegarse automáticamente en Railway.

### 📋 Requisitos Previos

1. **Cuenta en Railway**: [railway.app](https://railway.app)
2. **Repositorio en GitHub**: Con el código del proyecto
3. **Base de datos PostgreSQL**: Proporcionada por Railway

### 🚀 Pasos para Desplegar

#### 1. Conectar Repositorio
1. Ve a [Railway Dashboard](https://railway.app/dashboard)
2. Haz clic en "New Project"
3. Selecciona "Deploy from GitHub repo"
4. Conecta tu repositorio de GitHub
5. Selecciona el repositorio `PresupuestoFamiliarMensual`

#### 2. Configurar Base de Datos
1. En tu proyecto Railway, haz clic en "New Service"
2. Selecciona "Database" → "PostgreSQL"
3. Railway creará automáticamente una base de datos PostgreSQL

#### 3. Configurar Variables de Entorno
Railway detectará automáticamente la variable `DATABASE_URL` de la base de datos PostgreSQL.

**Variables opcionales:**
- `ASPNETCORE_ENVIRONMENT`: `Production`
- `ASPNETCORE_URLS`: `http://0.0.0.0:8080`

#### 4. Desplegar la Aplicación
1. Railway detectará automáticamente el `Dockerfile`
2. El build se ejecutará automáticamente
3. La aplicación se desplegará en un contenedor Docker

### 🔧 Configuración Automática

El proyecto incluye:

- **Dockerfile**: Configuración para contenedor Docker
- **railway.json**: Configuración específica para Railway
- **Health Check**: Endpoint `/api/referencedata/health`
- **Migraciones**: Se ejecutan automáticamente al iniciar

### 📊 Monitoreo

- **Logs**: Disponibles en Railway Dashboard
- **Health Check**: Automático en `/api/referencedata/health`
- **Métricas**: CPU, memoria y red en tiempo real

### 🔗 URLs

- **API**: `https://tu-proyecto.railway.app`
- **Swagger**: `https://tu-proyecto.railway.app/swagger`
- **Health Check**: `https://tu-proyecto.railway.app/api/referencedata/health`

### 🛠️ Comandos Útiles

```bash
# Ver logs en tiempo real
railway logs

# Ejecutar migraciones manualmente
railway run dotnet ef database update --project src/PresupuestoFamiliarMensual.Infrastructure

# Conectar a la base de datos
railway connect
```

### 🔒 Seguridad

- **HTTPS**: Automático en Railway
- **Variables de entorno**: Encriptadas
- **Base de datos**: Aislada y segura

### 📈 Escalabilidad

Railway permite escalar automáticamente según la demanda:
- **Auto-scaling**: Basado en CPU y memoria
- **Load balancing**: Automático
- **CDN**: Global para mejor rendimiento

### 🆘 Solución de Problemas

#### Error de Conexión a Base de Datos
1. Verifica que la variable `DATABASE_URL` esté configurada
2. Revisa los logs de la aplicación
3. Verifica que la base de datos esté activa

#### Error de Migraciones
1. Ejecuta manualmente: `railway run dotnet ef database update`
2. Verifica la conectividad a la base de datos
3. Revisa los logs de migración

#### Error de Build
1. Verifica que el Dockerfile esté correcto
2. Revisa los logs de build
3. Asegúrate de que todas las dependencias estén incluidas

### 📞 Soporte

- **Railway Docs**: [docs.railway.app](https://docs.railway.app)
- **Discord**: [Railway Discord](https://discord.gg/railway)
- **GitHub Issues**: Para problemas específicos del proyecto

# 🚀 Optimización de Despliegue en Railway

## Problemas Identificados y Soluciones

### 1. **Tiempo de Health Check Reducido**
- **Antes**: 300 segundos (5 minutos)
- **Ahora**: 60 segundos (1 minuto)
- **Archivo**: `railway.json`

### 2. **Dockerfile Optimizado**
- Variables de entorno configuradas directamente
- Optimización de capas de Docker
- Flags `--no-restore` y `--no-build` para evitar recompilaciones innecesarias

### 3. **Program.cs Optimizado**
- Swagger deshabilitado en producción
- Configuración de retry para base de datos
- Seed data solo en desarrollo
- Logs simplificados

### 4. **Health Check Simplificado**
- Endpoint `/api/health/simple` optimizado para velocidad
- Respuesta mínima: solo "OK"

### 5. **Archivos Excluidos**
- `.dockerignore` configurado para excluir archivos innecesarios
- Reduce el tamaño del contexto de construcción

## 🛠️ Configuraciones Aplicadas

### railway.json
```json
{
  "deploy": {
    "healthcheckTimeout": 60,
    "restartPolicyMaxRetries": 3,
    "numReplicas": 1
  }
}
```

### Dockerfile
- Variables de entorno: `ASPNETCORE_URLS` y `ASPNETCORE_ENVIRONMENT`
- Optimización de capas con `--no-restore` y `--no-build`

### Program.cs
- Swagger solo en desarrollo
- Configuración de retry para PostgreSQL
- Seed data condicional

## 📋 Pasos para Desplegar

1. **Instalar Railway CLI** (si no lo tienes):
   ```bash
   npm install -g @railway/cli
   ```

2. **Iniciar sesión**:
   ```bash
   railway login
   ```

3. **Desplegar**:
   ```bash
   railway up
   ```

4. **Verificar logs**:
   ```bash
   railway logs
   ```

## 🔍 Monitoreo

### Health Check
- Endpoint: `/api/health/simple`
- Timeout: 60 segundos
- Respuesta esperada: "OK"

### Logs Importantes
- `🚀 Puerto detectado: [puerto]`
- `✅ Aplicación iniciada correctamente`
- `⚠️ No se encontró cadena de conexión` (si no hay DB)

## 🚨 Solución de Problemas

### Si sigue tardando mucho:
1. **Verificar variables de entorno** en Railway Dashboard
2. **Revisar logs** con `railway logs`
3. **Verificar base de datos** - asegúrate de que `DATABASE_URL` esté configurada
4. **Reiniciar servicio** desde Railway Dashboard

### Variables de Entorno Requeridas
- `DATABASE_URL`: Cadena de conexión a PostgreSQL
- `PORT`: Puerto (Railway lo asigna automáticamente)
- `ASPNETCORE_ENVIRONMENT`: Environment (Railway lo asigna)

## 📊 Mejoras de Rendimiento Esperadas

- **Tiempo de construcción**: 30-50% más rápido
- **Tiempo de inicio**: 40-60% más rápido
- **Health check**: 80% más rápido
- **Tamaño de imagen**: 20-30% más pequeño

## 🔄 Próximos Pasos

Si el problema persiste, considera:
1. Usar una base de datos más cercana geográficamente
2. Implementar cache en memoria
3. Optimizar consultas de base de datos
4. Usar CDN para archivos estáticos 