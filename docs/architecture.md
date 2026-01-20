# Arquitectura del Sistema

## Visión General

El **Activity History Service** está diseñado siguiendo los principios de **Clean Architecture** (Arquitectura Limpia) y el patrón **CQRS** (Command Query Responsibility Segregation) mediante la biblioteca MediatR. Esta estructura garantiza:

- **Separación de Responsabilidades**: Cada capa tiene una función específica y bien definida
- **Independencia de Frameworks**: La lógica de negocio no depende de tecnologías específicas
- **Testabilidad**: Cada componente puede ser probado de forma aislada
- **Mantenibilidad**: El código es fácil de entender, modificar y extender

## Flujo de Datos (Request Pipeline)

### Flujo Completo de una Petición

```
Cliente HTTP
    ↓
[1] Controller (API Layer)
    ↓
[2] MediatR → Command/Query
    ↓
[3] Handler (Application Layer)
    ↓
[4] Domain Services / Validación
    ↓
[5] Repository (Infrastructure Layer)
    ↓
[6] Database (PostgreSQL)
    ↓
[7] Response (DTOs)
    ↓
Cliente HTTP
```

### Descripción Narrativa del Flujo

#### 1. **Entrada: Controller (Capa API)**

Cuando un cliente realiza una petición HTTP (por ejemplo, `POST /api/activityhistory/registerActivity/{email}`):

1. La petición llega al **ActivityHistoryController**
2. El controller valida los parámetros de entrada (email, JSON body)
3. Se crea un **Command** o **Query** según el tipo de operación
4. El command/query se envía a **MediatR** para su procesamiento

**Ejemplo de código:**
```csharp
[HttpPost("registerActivity/{email}")]
public async Task<IActionResult> CreateActivity(string email, [FromBody] CreateActivityDTO request)
{
    var command = new CreateActivityCommand(request, email);
    var response = await _mediator.Send(command, cancellationToken);
    return Ok(response);
}
```

#### 2. **Mediación: MediatR (Patrón Mediator)**

MediatR actúa como intermediario desacoplando el controller del handler:
- Recibe el comando/query
- Encuentra el handler apropiado registrado en el contenedor de dependencias
- Invoca el handler de forma asíncrona

#### 3. **Procesamiento: Handler (Capa de Aplicación)**

El **Handler** correspondiente ejecuta la lógica de aplicación:

**Para CreateActivityCommand:**
1. Valida que el usuario exista mediante el servicio `IUserServices`
2. Mapea el DTO a una entidad de dominio usando `ActivityHistoryMappers`
3. Invoca el repositorio para persistir la actividad
4. Retorna un DTO de respuesta

**Para GetActivitiesByUserEmailQuery:**
1. Valida que el usuario exista
2. Consulta el repositorio por todas las actividades del usuario
3. Mapea las entidades a DTOs de respuesta
4. Retorna la lista de actividades

#### 4. **Persistencia: Repository (Capa de Infraestructura)**

El **ActivityHistoryRepositoryPostgres**:
- Utiliza Entity Framework Core para interactuar con PostgreSQL
- Mapea entidades de dominio a modelos de persistencia (y viceversa)
- Ejecuta operaciones CRUD en la base de datos

#### 5. **Almacenamiento: Base de Datos PostgreSQL**

Los datos se almacenan en la tabla `ActivityHistories` con la siguiente estructura:
- `Id` (UUID): Identificador único de la actividad
- `Category` (string): Categoría de la actividad (ej: "Login", "Purchase")
- `Action` (string): Descripción de la acción realizada
- `TimeDate` (string): Timestamp de cuando ocurrió la actividad
- `UserEmail` (string): Email del usuario que realizó la actividad

#### 6. **Retorno: Response DTOs**

Los handlers retornan DTOs específicos que son serializados a JSON por el controller:
- `CreateActivityResponseDto`: Confirma el registro de la actividad
- `GetActivityGetByUserEmailResponseDTO`: Lista de actividades históricas

## Dependencias Externas

### 1. **Microservicio de Usuarios (User Service)**

**URL Base**: `http://localhost:7181/api/users/`

**Propósito**: Validar la existencia de usuarios antes de registrar actividades

**Endpoint Utilizado**:
- `GET /api/users/getIdUser/{email}` - Obtiene el ID de usuario por email

**Implementación**: 
- Servicio: `UserService` (Infrastructure Layer)
- Cliente HTTP configurado mediante `HttpClientFactory` en `Program.cs`

**Acoplamiento**:
- **Riesgo Identificado**: El servicio está fuertemente acoplado a la disponibilidad del microservicio de usuarios
- Si el servicio de usuarios está caído, las operaciones de registro y consulta fallarán
- No existe mecanismo de circuit breaker o fallback implementado

**Código de integración:**
```csharp
builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7181/api/users/");
});
```

### 2. **Base de Datos PostgreSQL**

**Host**: Configurable mediante Connection String
- **Local**: `localhost:5432`
- **Docker**: `db:5432` (nombre del contenedor)

**Base de Datos**: `activityhistory_service`

**Usuario/Password**: Configurables (por defecto: `postgres/postgres`)

**Gestión de Migraciones**:
- Las migraciones se ejecutan automáticamente al iniciar la aplicación
- Assembly de migraciones: `activityhistory_infrastructure`

### 3. **Frontend Client (CORS)**

**Origin Permitido**: `http://localhost:3000`

**Configuración**: Política CORS configurada para aceptar peticiones desde un cliente frontend local

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

## Modelo de Datos

### Entidad Principal: ActivityHistory

**Ubicación**: `activityhistory_domain.Entities.ActivityHistory`

**Propósito**: Representa una actividad individual realizada por un usuario en el sistema

**Propiedades**:

| Campo | Tipo | Descripción | Restricciones |
|-------|------|-------------|---------------|
| `Id` | `Guid` | Identificador único de la actividad | PK, Auto-generado |
| `Category` | `string` | Categoría de la acción (ej: Login, Purchase, Update) | Requerido |
| `Action` | `string` | Descripción textual de la acción realizada | Requerido |
| `TimeDate` | `DateTime` | Timestamp de cuándo se registró la actividad | Auto-generado en el handler |
| `UserEmail` | `string` | Email del usuario que realizó la acción | Requerido, usado como índice |

**Constructores**:
1. Constructor completo (sin ID): Para crear nuevas actividades
2. Constructor con ID: Para reconstruir entidades desde la base de datos
3. Constructor sin parámetros: Requerido por Entity Framework

**Nota**: La entidad se mapea a un modelo de persistencia (`ActivityHistoryPostgres`) donde `TimeDate` se almacena como `string` en lugar de `DateTime`.

## Patrones de Diseño Implementados

### 1. **Clean Architecture (Arquitectura Limpia)**

El código se organiza en 4 capas concéntricas:

```
┌──────────────────────────────────┐
│  API (Controllers)               │ ← Capa de Presentación
├──────────────────────────────────┤
│  Application (Commands/Queries)  │ ← Casos de Uso
├──────────────────────────────────┤
│  Domain (Entities/Interfaces)    │ ← Lógica de Negocio
├──────────────────────────────────┤
│  Infrastructure (DB/Services)    │ ← Detalles de Implementación
└──────────────────────────────────┘
```

**Regla de Dependencia**: Las capas internas no dependen de las externas

### 2. **CQRS (Command Query Responsibility Segregation)**

Separación clara entre operaciones de lectura y escritura:

**Commands (Escritura)**:
- `CreateActivityCommand` → `CreateActivityHandler`

**Queries (Lectura)**:
- `GetActivitiesByUserEmailQuery` → `GetActivitiesByUserEmailHandler`

### 3. **Repository Pattern**

Abstracción de la capa de persistencia mediante interfaces:
- `IActivityHistoryRepositoryPostgres` (Domain)
- `ActivityHistoryRepositoryPostgres` (Infrastructure)

### 4. **Dependency Injection**

Todos los servicios y repositorios se registran en el contenedor de DI de ASP.NET Core:
```csharp
builder.Services.AddScoped<IActivityHistoryRepositoryPostgres, ActivityHistoryRepositoryPostgres>();
builder.Services.AddScoped<IUserServices, UserService>();
```

### 5. **Mapper Pattern**

Conversión entre diferentes representaciones de datos:
- `ActivityHistoryMappers` (Application Layer): Domain ↔ DTOs
- `ActivityHistoryMappers` (Infrastructure Layer): Domain ↔ Persistence Models

## Deuda Técnica Detectada

### 🔴 **Crítico**

#### 1. **Lógica de Validación Incorrecta en CreateActivityHandler**

**Ubicación**: `CreateActivityHandler.cs`, línea 35-38

```csharp
if (usersRegistered == Guid.Empty) 
{ 
    throw new ApplicationException($"El usuario con email {activity.UserEmail} ya existe en la base de datos.");
}
```

**Problema**: 
- El mensaje de error dice "ya existe" pero la condición valida si el usuario **NO existe** (`Guid.Empty` significa no encontrado)
- El mensaje debería decir: "El usuario con email {email} **no existe** en la base de datos."

**Impacto**: Confusión en el manejo de errores y mensajes engañosos al cliente

#### 2. **Validación Duplicada en GetActivitiesByUserEmailHandler**

**Ubicación**: `GetActivitiesByUserEmailHandler.cs`, líneas 29-32 y 36-39

```csharp
// Primera validación
if (usersRegistered == Guid.Empty)
{
    throw new ApplicationException($"No existen el usuario con email {request.Email} en la base de datos.");
}

var activities = await _activityHistoryServices.GetActivityByUserEmailAsync(request.Email);

// Segunda validación (incorrecta, usa la misma variable)
if (usersRegistered == Guid.Empty)
{
    throw new ApplicationException($"No existe historia para el usuario {request.Email} en la base de datos.");
}
```

**Problema**:
- La segunda validación es redundante y nunca se ejecutará porque la primera ya lanzó la excepción
- La segunda validación debería verificar `activities.Count == 0` en lugar de `usersRegistered`

**Impacto**: Código muerto y lógica de validación incorrecta

#### 3. **Falta de Manejo de Resiliencia para Dependencias Externas**

**Ubicación**: `UserService.cs` y configuración de HttpClient en `Program.cs`

**Problema**:
- No hay retry policies, circuit breakers o timeouts configurados
- Si el servicio de usuarios está caído, todas las operaciones fallan inmediatamente
- No existe fallback o cache

**Impacto**: Baja resiliencia del sistema ante fallos de dependencias

**Recomendación**: Implementar Polly para políticas de retry y circuit breaker

### 🟡 **Medio**

#### 4. **Código de Debugging en Producción**

**Ubicación**: Múltiples archivos (Controllers y Handlers)

```csharp
Console.WriteLine("Llegó al controlador");
Console.WriteLine($"Request Data: {request.Action}");
Console.WriteLine($"Usuario obtenido: {usersRegistered}");
```

**Problema**:
- Uso de `Console.WriteLine` en lugar de un logger estructurado
- Estos mensajes se ejecutarán en producción afectando el rendimiento

**Recomendación**: Reemplazar con `ILogger<T>` inyectado

#### 5. **Inconsistencia en el Modelo de Datos: TimeDate como String**

**Ubicación**: `ActivityHistoryPostgres.cs`, línea 14

```csharp
public string TimeDate { get; set; }
```

**Problema**:
- En la entidad de dominio `TimeDate` es `DateTime`
- En el modelo de persistencia `TimeDate` es `string`
- Esta conversión manual puede generar problemas de timezone y formato

**Recomendación**: Almacenar como `timestamp` en PostgreSQL y usar `DateTime` en el modelo

#### 6. **Namespace con Typo**

**Ubicación**: `GetActivitiesByUserEmailHandler.cs`, línea 13

```csharp
namespace Aplicaactivityhistory_applicationtion.Queries.Handlers
```

**Problema**: El namespace tiene un error tipográfico "Aplica**activityhistory_application**tion"

**Impacto**: Inconsistencia en naming conventions del proyecto

#### 7. **Falta de Validación de DTOs**

**Ubicación**: `CreateActivityDTO.cs` y controllers

**Problema**:
- No hay atributos de validación como `[Required]`, `[EmailAddress]`, etc.
- La validación se delega completamente a los handlers

**Recomendación**: Agregar Data Annotations o FluentValidation

### 🟢 **Bajo**

#### 8. **Import No Utilizado**

**Ubicación**: `ActivityHistoryControllers.cs`, línea 7

```csharp
using static System.Runtime.InteropServices.JavaScript.JSType;
```

**Problema**: Este using no se utiliza en el código

#### 9. **Atributo IMediator Declarado pero No Usado**

**Ubicación**: `CreateActivityHandler.cs` y `GetActivitiesByUserEmailHandler.cs`

```csharp
public readonly IMediator _mediator;
```

**Problema**: Se declara pero nunca se inicializa ni se utiliza en los handlers

#### 10. **Configuración de HTTPS Redirect en Entorno Docker**

**Ubicación**: `Program.cs`, línea 80

```csharp
app.UseHttpsRedirection();
```

**Problema**: 
- El Docker container usa HTTP en el puerto 7182
- HTTPS redirect puede causar problemas en entornos de desarrollo/producción sin certificados

**Recomendación**: Condicionar el uso solo en ambientes específicos

## Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────┐
│                    Frontend (localhost:3000)             │
└────────────────────┬────────────────────────────────────┘
                     │ HTTP/REST
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Activity History Service (Port 7182)                    │
│  ┌───────────────────────────────────────────────────┐  │
│  │  Controllers (ActivityHistoryController)          │  │
│  └───────────┬──────────────┬────────────────────────┘  │
│              │              │                            │
│  ┌───────────▼───────┐  ┌──▼────────────────────────┐  │
│  │  Commands         │  │  Queries                   │  │
│  │  (CreateActivity) │  │  (GetActivitiesByEmail)    │  │
│  └───────────┬───────┘  └──┬────────────────────────┘  │
│              │              │                            │
│  ┌───────────▼──────────────▼────────────────────────┐  │
│  │         Handlers (Application Layer)              │  │
│  │  - CreateActivityHandler                          │  │
│  │  - GetActivitiesByUserEmailHandler                │  │
│  └───────────┬───────────────────┬───────────────────┘  │
│              │                   │                       │
│  ┌───────────▼───────┐  ┌────────▼──────────────────┐  │
│  │  Repository       │  │  UserService              │  │
│  │  (Infrastructure) │  │  (HTTP Client)            │  │
│  └───────────┬───────┘  └────────┬──────────────────┘  │
└──────────────┼──────────────────┼─────────────────────┘
               │                  │
               ▼                  ▼
    ┌──────────────────┐  ┌──────────────────┐
    │   PostgreSQL     │  │   User Service   │
    │   (Port 5432)    │  │   (Port 7181)    │
    └──────────────────┘  └──────────────────┘
```

## Consideraciones de Seguridad

### Implementadas
- ✅ CORS configurado para origen específico
- ✅ Variables de entorno para credenciales de base de datos

### Pendientes
- ⚠️ **Sin Autenticación/Autorización**: No hay validación de tokens JWT o similar
- ⚠️ **Sin Rate Limiting**: Vulnerable a ataques de fuerza bruta
- ⚠️ **Sin Validación de Input**: Los DTOs no tienen validaciones robustas
- ⚠️ **Sin Encriptación**: Las comunicaciones HTTP no están aseguradas (usar HTTPS en producción)

## Escalabilidad y Rendimiento

### Fortalezas
- ✅ Uso de async/await en todo el pipeline
- ✅ Entity Framework con queries optimizadas
- ✅ Patrón CQRS permite escalar lecturas y escrituras independientemente

### Limitaciones
- ⚠️ Sin caching implementado
- ⚠️ Sin paginación en consultas (puede ser problema con grandes volúmenes)
- ⚠️ Sin índices específicos en la base de datos más allá de la PK

## Recomendaciones de Mejora

1. **Corregir la lógica de validación incorrecta** en los handlers
2. **Implementar logging estructurado** con `ILogger<T>`
3. **Agregar políticas de resiliencia** (Polly) para llamadas HTTP
4. **Implementar validación de DTOs** con FluentValidation
5. **Añadir autenticación y autorización** (JWT tokens)
6. **Implementar paginación** en queries de historial
7. **Agregar cache distribuido** (Redis) para consultas frecuentes
8. **Corregir el namespace con typo**
9. **Remover código muerto** (imports no usados, variables declaradas sin usar)
10. **Añadir índices a la base de datos** en el campo `UserEmail`
