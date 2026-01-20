# Guía de Configuración y Despliegue

Esta guía proporciona instrucciones detalladas para configurar, construir y desplegar el **Activity History Service**.

## Tabla de Contenidos

- [Requisitos Previos](#requisitos-previos)
- [Variables de Entorno](#variables-de-entorno)
- [Configuración de Base de Datos](#configuración-de-base-de-datos)
- [Instalación y Ejecución](#instalación-y-ejecución)
- [Docker](#docker)
- [Scripts Disponibles](#scripts-disponibles)
- [Migraciones de Base de Datos](#migraciones-de-base-de-datos)
- [Configuración de Dependencias Externas](#configuración-de-dependencias-externas)

---

## Requisitos Previos

### Desarrollo Local

| Herramienta | Versión Mínima | Propósito |
|------------|----------------|-----------|
| **.NET SDK** | 8.0 | Framework de desarrollo y runtime |
| **PostgreSQL** | 15+ | Base de datos relacional |
| **Docker** (opcional) | 20.10+ | Contenedorización |
| **Docker Compose** (opcional) | 2.0+ | Orquestación de contenedores |
| **Git** | 2.0+ | Control de versiones |

### Verificar Instalaciones

```bash
# Verificar .NET SDK
dotnet --version
# Debe mostrar: 8.0.x

# Verificar PostgreSQL
psql --version
# Debe mostrar: psql (PostgreSQL) 15.x o superior

# Verificar Docker
docker --version
docker-compose --version
```

---

## Variables de Entorno

### Configuración Completa

El servicio utiliza el sistema de configuración de ASP.NET Core que soporta múltiples fuentes:
1. `appsettings.json` (configuración base)
2. `appsettings.{Environment}.json` (configuración por ambiente)
3. Variables de entorno (sobrescriben configuraciones anteriores)

### Tabla de Variables de Entorno

| Variable | Tipo | Requerida | Default | Descripción |
|----------|------|-----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | string | No | `Production` | Entorno de ejecución: `Development`, `Staging`, `Production` |
| `ASPNETCORE_URLS` | string | No | `http://*:7182` | URLs donde escucha el servicio (ej: `http://*:5000;https://*:5001`) |
| `ConnectionStrings__ConnectionPostgre` | string | **Sí** | - | Connection string completo para PostgreSQL |

### Configuración del Connection String

**Formato**:
```
Host={host};Port={port};Database={database};Username={user};Password={password}
```

**Ejemplo Local**:
```bash
export ConnectionStrings__ConnectionPostgre="Host=localhost;Port=5432;Database=activityhistory_service;Username=postgres;Password=postgres"
```

**Ejemplo Docker**:
```bash
export ConnectionStrings__ConnectionPostgre="Host=db;Port=5432;Database=activityhistory_service;Username=postgres;Password=postgres"
```

**Ejemplo Producción (con SSL)**:
```bash
export ConnectionStrings__ConnectionPostgre="Host=prod-db.example.com;Port=5432;Database=activityhistory_service;Username=app_user;Password=SecureP@ssw0rd;SSL Mode=Require"
```

### Archivos de Configuración

#### `appsettings.json` (Base)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ConnectionPostgre": "Host=localhost;Port=5432;Database=activityhistory_service;Username=postgres;Password=postgres"
  }
}
```

#### `appsettings.Development.json` (Desarrollo)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

> **Nota**: Los archivos `appsettings.*.json` ya existen en el proyecto. Solo edita `ConnectionStrings__ConnectionPostgre` si usas variables de entorno.

### Configuración de Logging

El servicio utiliza el sistema de logging de ASP.NET Core. Los niveles disponibles son:
- `Trace` (nivel más detallado)
- `Debug`
- `Information`
- `Warning`
- `Error`
- `Critical`

**Cambiar nivel de logging mediante variable de entorno**:
```bash
export Logging__LogLevel__Default=Debug
export Logging__LogLevel__Microsoft.AspNetCore=Information
```

---

## Configuración de Base de Datos

### Instalación de PostgreSQL (Local)

#### Linux (Ubuntu/Debian)
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

#### macOS (Homebrew)
```bash
brew install postgresql@15
brew services start postgresql@15
```

#### Windows
Descarga el instalador desde [postgresql.org](https://www.postgresql.org/download/windows/)

### Crear Base de Datos

```bash
# Conectar como superusuario postgres
sudo -u postgres psql

# Dentro de psql:
CREATE DATABASE activityhistory_service;
CREATE USER activityhistory_user WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE activityhistory_service TO activityhistory_user;
\q
```

### Configurar Connection String con Usuario Personalizado

```bash
export ConnectionStrings__ConnectionPostgre="Host=localhost;Port=5432;Database=activityhistory_service;Username=activityhistory_user;Password=your_secure_password"
```

---

## Instalación y Ejecución

### Opción 1: Ejecución Local (Sin Docker)

#### 1. Clonar el Repositorio

```bash
git clone https://github.com/eventmesh-lab/activityhistory_services.git
cd activityhistory_services
```

#### 2. Restaurar Dependencias

```bash
dotnet restore
```

Esto descarga todos los paquetes NuGet especificados en los archivos `.csproj`.

#### 3. Configurar Base de Datos

Edita `src/activityhistory_api/appsettings.json` o configura variables de entorno:

```bash
export ConnectionStrings__ConnectionPostgre="Host=localhost;Port=5432;Database=activityhistory_service;Username=postgres;Password=postgres"
```

#### 4. Aplicar Migraciones

```bash
cd src/activityhistory_api
dotnet ef database update --project ../activityhistory_infrastructure
```

> **Nota**: Las migraciones también se aplican automáticamente al iniciar la aplicación si están pendientes.

#### 5. Ejecutar el Servicio

```bash
dotnet run --project src/activityhistory_api
```

O con watch mode para desarrollo (recarga automática):

```bash
dotnet watch run --project src/activityhistory_api
```

#### 6. Verificar Funcionamiento

```bash
# Verificar que el servicio está corriendo
curl http://localhost:7182/swagger/index.html

# O abre en tu navegador:
# http://localhost:7182/swagger
```

### Opción 2: Ejecución con Docker Compose (Recomendado)

#### 1. Levantar Todos los Servicios

```bash
# Construir y levantar en modo detached (background)
docker-compose up -d

# Ver logs
docker-compose logs -f api
```

#### 2. Verificar Estado de Contenedores

```bash
docker-compose ps
```

Deberías ver:
```
NAME                    STATUS                 PORTS
activityhistory-api     Up 2 minutes          0.0.0.0:7182->7182/tcp
activityhistory-db      Up 2 minutes (healthy) 0.0.0.0:3439->5432/tcp
```

#### 3. Acceder a la API

```bash
curl http://localhost:7182/swagger/index.html
```

#### 4. Detener los Servicios

```bash
# Detener sin eliminar contenedores
docker-compose stop

# Detener y eliminar contenedores
docker-compose down

# Detener, eliminar contenedores y volúmenes (⚠️ elimina datos de BD)
docker-compose down -v
```

### Opción 3: Modo Desarrollo con Docker

Para desarrollo con hot-reload:

```bash
docker-compose --profile dev up api-dev
```

Este modo monta el código fuente como volumen, permitiendo cambios en tiempo real.

---

## Docker

### Dockerfile Explicado

```dockerfile
# Etapa 1: Build (Compilación)
ARG APP_PORT=7182
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/activityhistory_api/activityhistory_api.csproj
RUN dotnet publish src/activityhistory_api/activityhistory_api.csproj -c Release -o /app/publish

# Etapa 2: Runtime (Ejecución)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
ARG APP_PORT=7182
ENV ASPNETCORE_URLS=http://*:${APP_PORT}
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE ${APP_PORT}
ENTRYPOINT ["dotnet", "activityhistory_api.dll"]
```

**Explicación de las Etapas**:

1. **Build Stage**: 
   - Usa imagen completa del SDK de .NET 8.0
   - Restaura dependencias NuGet
   - Compila el proyecto en modo Release
   - Genera archivos publicados en `/app/publish`

2. **Runtime Stage**:
   - Usa imagen ligera de runtime de ASP.NET 8.0 (sin SDK)
   - Copia solo los archivos compilados desde la etapa de build
   - Configura el puerto mediante variable de entorno
   - Define el punto de entrada de la aplicación

**Ventajas del Multi-Stage Build**:
- ✅ Imagen final más pequeña (~210 MB vs ~1 GB con SDK)
- ✅ Mayor seguridad (no incluye herramientas de desarrollo)
- ✅ Compilación reproducible

### Construir Imagen Manualmente

```bash
# Construir imagen
docker build -t activityhistory-api:latest .

# Construir con puerto personalizado
docker build --build-arg APP_PORT=8080 -t activityhistory-api:latest .

# Ver la imagen creada
docker images | grep activityhistory
```

### Ejecutar Contenedor Manualmente

```bash
# Ejecutar contenedor con variables de entorno
docker run -d \
  --name activityhistory-api \
  -p 7182:7182 \
  -e ConnectionStrings__ConnectionPostgre="Host=host.docker.internal;Port=5432;Database=activityhistory_service;Username=postgres;Password=postgres" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  activityhistory-api:latest

# Ver logs
docker logs -f activityhistory-api

# Detener y eliminar
docker stop activityhistory-api
docker rm activityhistory-api
```

### Docker Compose Explicado

#### Servicio de Base de Datos (`db`)

```yaml
db:
  image: postgres:15-alpine          # Imagen ligera de PostgreSQL 15
  container_name: activityhistory-db
  environment:
    POSTGRES_USER: postgres          # Usuario admin de PostgreSQL
    POSTGRES_PASSWORD: postgres      # ⚠️ Cambiar en producción
    POSTGRES_DB: activityhistory_service
  ports:
    - "3439:5432"                    # Puerto externo:interno (evita conflicto con PG local)
  volumes:
    - activityhistory_pg_data:/var/lib/postgresql/data  # Persistencia de datos
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U postgres"]
    interval: 10s                    # Verifica cada 10 segundos
    timeout: 5s
    retries: 5
  restart: unless-stopped
```

**Notas**:
- Puerto `3439` en el host para evitar conflictos con PostgreSQL local (que usa 5432)
- Healthcheck asegura que la API no inicie hasta que la BD esté lista
- Volumen nombrado para persistir datos entre reinicios

#### Servicio API (`api`)

```yaml
api:
  build:
    context: .
    dockerfile: Dockerfile
    target: runtime                  # Etapa del multi-stage build
  container_name: activityhistory-api
  ports:
    - "7182:7182"
  depends_on:
    db:
      condition: service_healthy     # Espera healthcheck de la BD
  environment:
    ASPNETCORE_ENVIRONMENT: Production
    ConnectionStrings__ConnectionPostgre: "Host=db;Port=5432;..."  # 'db' = nombre del servicio
  restart: unless-stopped
```

**Notas**:
- `depends_on` con `service_healthy` asegura que la BD esté lista antes de iniciar
- Connection string usa `Host=db` (nombre del servicio en Docker network)

### Networking en Docker Compose

Docker Compose crea automáticamente una red bridge donde:
- Los servicios se comunican por nombre (ej: `db` resuelve a la IP del contenedor)
- El DNS interno de Docker resuelve los nombres de servicio

```bash
# Ver la red creada
docker network ls | grep activityhistory

# Inspeccionar la red
docker network inspect activityhistory_services_default
```

### Volúmenes y Persistencia

```bash
# Listar volúmenes
docker volume ls | grep activityhistory

# Inspeccionar volumen
docker volume inspect activityhistory_services_activityhistory_pg_data

# Backup del volumen
docker run --rm \
  -v activityhistory_services_activityhistory_pg_data:/data \
  -v $(pwd):/backup \
  alpine tar czf /backup/db-backup.tar.gz -C /data .

# Restaurar backup
docker run --rm \
  -v activityhistory_services_activityhistory_pg_data:/data \
  -v $(pwd):/backup \
  alpine tar xzf /backup/db-backup.tar.gz -C /data
```

---

## Scripts Disponibles

### Comandos .NET CLI

El proyecto utiliza el .NET CLI estándar. Aquí están los comandos más comunes:

#### Restaurar Dependencias

```bash
dotnet restore
```

**Descripción**: Descarga todos los paquetes NuGet especificados en los archivos `.csproj` de todos los proyectos.

#### Compilar el Proyecto

```bash
# Compilar en modo Debug
dotnet build

# Compilar en modo Release
dotnet build -c Release

# Compilar un proyecto específico
dotnet build src/activityhistory_api/activityhistory_api.csproj
```

**Descripción**: Compila el código fuente en archivos `.dll` sin ejecutar la aplicación.

#### Ejecutar el Servicio

```bash
# Ejecutar con configuración por defecto
dotnet run --project src/activityhistory_api

# Ejecutar en modo Development
dotnet run --project src/activityhistory_api --environment Development

# Ejecutar con watch (recarga automática en desarrollo)
dotnet watch run --project src/activityhistory_api
```

**Descripción**: Compila (si es necesario) y ejecuta la aplicación.

#### Ejecutar Tests

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar tests de un proyecto específico
dotnet test tests/activityhistory_api.Tests/activityhistory_api.Tests.csproj

# Ejecutar tests con verbosidad detallada
dotnet test --verbosity detailed
```

**Descripción**: Ejecuta todas las pruebas unitarias y de integración del proyecto.

#### Publicar para Despliegue

```bash
# Publicar en modo Release
dotnet publish src/activityhistory_api -c Release -o ./publish

# Publicar para Linux x64
dotnet publish src/activityhistory_api -c Release -r linux-x64 --self-contained false -o ./publish
```

**Descripción**: Genera los archivos necesarios para desplegar en un servidor de producción.

#### Limpiar Build Artifacts

```bash
# Limpiar archivos de compilación
dotnet clean

# Limpiar y eliminar también carpetas bin/obj
dotnet clean && find . -type d -name "bin" -o -name "obj" | xargs rm -rf
```

### Comandos de Inspección

```bash
# Ver información del SDK instalado
dotnet --info

# Listar todos los proyectos en la solución
dotnet sln list

# Verificar referencias entre proyectos
dotnet list src/activityhistory_api reference

# Ver paquetes NuGet instalados
dotnet list src/activityhistory_api package
```

---

## Migraciones de Base de Datos

El proyecto usa **Entity Framework Core Migrations** para gestionar el esquema de la base de datos.

### Conceptos Clave

- **Migraciones**: Archivos de código que representan cambios en el esquema de BD
- **DbContext**: `AppDbContext` en el proyecto `activityhistory_infrastructure`
- **Assembly de Migraciones**: Las migraciones se almacenan en `activityhistory_infrastructure/Migrations`

### Comandos de Migraciones

#### Ver Migraciones Existentes

```bash
dotnet ef migrations list \
  --project src/activityhistory_infrastructure \
  --startup-project src/activityhistory_api
```

#### Aplicar Migraciones a la Base de Datos

```bash
# Aplicar todas las migraciones pendientes
dotnet ef database update \
  --project src/activityhistory_infrastructure \
  --startup-project src/activityhistory_api

# Aplicar hasta una migración específica
dotnet ef database update AddMediaColumns \
  --project src/activityhistory_infrastructure \
  --startup-project src/activityhistory_api

# Revertir todas las migraciones (⚠️ elimina datos)
dotnet ef database update 0 \
  --project src/activityhistory_infrastructure \
  --startup-project src/activityhistory_api
```

#### Crear Nueva Migración

```bash
# Crear migración con nombre descriptivo
dotnet ef migrations add AddNewColumn \
  --project src/activityhistory_infrastructure \
  --startup-project src/activityhistory_api

# Crear migración con output en carpeta específica
dotnet ef migrations add AddNewColumn \
  --project src/activityhistory_infrastructure \
  --startup-project src/activityhistory_api \
  --output-dir Migrations
```

**Pasos**:
1. Modifica las entidades en `activityhistory_domain/Entities`
2. Ejecuta el comando de migración
3. Revisa los archivos generados en `activityhistory_infrastructure/Migrations`
4. Aplica la migración con `dotnet ef database update`

#### Eliminar Última Migración

```bash
# Eliminar migración (solo si no se ha aplicado a la BD)
dotnet ef migrations remove \
  --project src/activityhistory_infrastructure \
  --startup-project src/activityhistory_api
```

#### Generar Script SQL de Migración

```bash
# Generar script SQL de todas las migraciones
dotnet ef migrations script \
  --project src/activityhistory_infrastructure \
  --startup-project src/activityhistory_api \
  --output migrations.sql

# Generar script SQL desde migración específica
dotnet ef migrations script InitialCreate AddMediaColumns \
  --project src/activityhistory_infrastructure \
  --startup-project src/activityhistory_api \
  --output update.sql
```

**Uso**: Scripts SQL para aplicar migraciones manualmente en entornos de producción.

### Aplicación Automática de Migraciones

El servicio aplica migraciones automáticamente al iniciar (ver `Program.cs` líneas 63-78):

```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();  // ← Aplica migraciones pendientes
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones a la base de datos.");
    }
}
```

**Pros**:
- ✅ Simplifica despliegue (no requiere pasos manuales)
- ✅ Garantiza que la BD esté actualizada

**Contras**:
- ⚠️ Puede causar problemas en entornos con múltiples instancias (race conditions)
- ⚠️ No hay control sobre cuándo se aplican las migraciones

**Recomendación**: En producción, considera aplicar migraciones manualmente en un pipeline de CI/CD.

---

## Configuración de Dependencias Externas

### User Service (Microservicio de Usuarios)

El servicio depende de un microservicio externo para validar usuarios.

**Configuración Actual** (`Program.cs` líneas 33-36):

```csharp
builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7181/api/users/");
});
```

#### Cambiar la URL del User Service

**Opción 1: Modificar código** (No recomendado)

Edita `Program.cs` y cambia la URL base.

**Opción 2: Mediante configuración** (Recomendado)

Modifica `appsettings.json`:

```json
{
  "ExternalServices": {
    "UserServiceBaseUrl": "http://user-service:7181/api/users/"
  }
}
```

Luego actualiza `Program.cs`:

```csharp
var userServiceUrl = builder.Configuration["ExternalServices:UserServiceBaseUrl"];
builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri(userServiceUrl);
});
```

**Opción 3: Variable de entorno**

```bash
export ExternalServices__UserServiceBaseUrl="https://users-api.production.com/api/users/"
```

#### Configurar Timeouts y Retry Policies

**Instalar Polly** (recomendado para resiliencia):

```bash
dotnet add src/activityhistory_api package Microsoft.Extensions.Http.Polly
```

**Configurar en Program.cs**:

```csharp
using Polly;
using Polly.Extensions.Http;

builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7181/api/users/");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

### CORS Configuration

Para permitir requests desde orígenes adicionales, edita `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://myapp.com",
                "https://staging.myapp.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

O usa configuración para orígenes dinámicos:

```csharp
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DynamicOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

En `appsettings.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://myapp.com"
    ]
  }
}
```

---

## Despliegue en Producción

### Checklist de Seguridad y Configuración

- [ ] **Cambiar credenciales de base de datos** (no usar `postgres/postgres`)
- [ ] **Usar HTTPS** en producción (configurar certificados SSL/TLS)
- [ ] **Deshabilitar Swagger** en producción (solo Development)
- [ ] **Configurar logging estructurado** (Serilog, Application Insights, etc.)
- [ ] **Implementar autenticación y autorización** (JWT tokens)
- [ ] **Configurar rate limiting** (prevenir abuso)
- [ ] **Usar secretos seguros** (Azure Key Vault, AWS Secrets Manager, etc.)
- [ ] **Configurar healthchecks** (`/health` endpoint)
- [ ] **Configurar monitoreo y alertas** (Prometheus, Grafana, etc.)
- [ ] **Revisar CORS** (solo orígenes confiables)
- [ ] **Aplicar migraciones de BD** antes de desplegar nueva versión

### Variables de Entorno para Producción

```bash
# Entorno
export ASPNETCORE_ENVIRONMENT=Production

# URLs (HTTPS)
export ASPNETCORE_URLS=https://*:443

# Base de datos con credenciales seguras
export ConnectionStrings__ConnectionPostgre="Host=prod-db.example.com;Port=5432;Database=activityhistory_service;Username=app_user;Password=${DB_PASSWORD};SSL Mode=Require"

# User Service (URL de producción)
export ExternalServices__UserServiceBaseUrl="https://users-api.production.com/api/users/"

# Logging (nivel apropiado para producción)
export Logging__LogLevel__Default=Warning
export Logging__LogLevel__Microsoft.AspNetCore=Error
```

### Despliegue con Docker en Producción

```bash
# Construir imagen de producción
docker build -t activityhistory-api:1.0.0 .

# Tag para registry
docker tag activityhistory-api:1.0.0 myregistry.azurecr.io/activityhistory-api:1.0.0

# Push a registry
docker push myregistry.azurecr.io/activityhistory-api:1.0.0

# Ejecutar en producción (ejemplo con Kubernetes)
kubectl apply -f k8s/deployment.yaml
```

---

## Troubleshooting (Resolución de Problemas)

### Problema: "Connection refused" a la base de datos

**Síntomas**: Error al iniciar la aplicación relacionado con conexión a PostgreSQL

**Soluciones**:

1. Verificar que PostgreSQL está corriendo:
   ```bash
   # Linux/macOS
   sudo systemctl status postgresql
   # Docker
   docker-compose ps db
   ```

2. Verificar el connection string:
   ```bash
   echo $ConnectionStrings__ConnectionPostgre
   ```

3. Probar conexión manualmente:
   ```bash
   psql -h localhost -p 5432 -U postgres -d activityhistory_service
   ```

### Problema: Error al aplicar migraciones

**Síntomas**: `dotnet ef database update` falla

**Soluciones**:

1. Verificar que `dotnet-ef` está instalado:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. Verificar connection string en `appsettings.json`

3. Verificar permisos del usuario de BD

### Problema: Puerto 7182 ya está en uso

**Síntomas**: Error "Address already in use" al iniciar

**Soluciones**:

1. Identificar qué proceso usa el puerto:
   ```bash
   # Linux/macOS
   lsof -i :7182
   # Windows
   netstat -ano | findstr :7182
   ```

2. Cambiar el puerto en `appsettings.json` o variable de entorno:
   ```bash
   export ASPNETCORE_URLS="http://*:8080"
   ```

### Problema: User Service no responde

**Síntomas**: Errores 500 al registrar o consultar actividades

**Soluciones**:

1. Verificar que el User Service está corriendo:
   ```bash
   curl http://localhost:7181/api/users/getIdUser/test@example.com
   ```

2. Revisar logs del User Service

3. Configurar timeout más alto o implementar fallback

---

## Recursos Adicionales

- [Documentación de .NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

---

## Contacto y Soporte

Para problemas de configuración, contacte al equipo de DevOps o abra un issue en el repositorio del proyecto.
