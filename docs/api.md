# API Documentation

## Visión General

La API del **Activity History Service** proporciona endpoints REST para registrar y consultar el historial de actividades de usuarios. Utiliza Swagger/OpenAPI para documentación interactiva.

**Base URL**: `http://localhost:7182/api/activityhistory`

**Swagger UI**: `http://localhost:7182/swagger` (solo en Development)

## Autenticación

⚠️ **Actualmente no implementada**: La API no requiere autenticación. Todos los endpoints son públicos.

> **Nota de Seguridad**: En un entorno de producción, se recomienda implementar autenticación mediante JWT tokens u otro mecanismo de seguridad.

## Endpoints Disponibles

### 1. Registrar Actividad

Registra una nueva actividad para un usuario específico identificado por su email.

**Endpoint**: `POST /api/activityhistory/registerActivity/{email}`

**Parámetros de URL**:
- `email` (string, requerido): Email del usuario que realiza la actividad

**Request Body** (JSON):
```json
{
  "category": "string",
  "action": "string"
}
```

**Campos del Request**:
| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `category` | `string` | Sí | Categoría de la actividad (ej: "Login", "Purchase", "Update") |
| `action` | `string` | Sí | Descripción detallada de la acción realizada |

**Respuesta Exitosa** (200 OK):
```json
{
  "usuario": {
    "category": "string",
    "action": "string",
    "timeDate": "2026-01-20T02:30:45.123Z",
    "userEmail": "string"
  },
  "mensaje": "Actividad registrada exitosamente."
}
```

**Respuestas de Error**:

| Código | Descripción |
|--------|-------------|
| `400 Bad Request` | Request body inválido o email mal formado |
| `500 Internal Server Error` | El usuario no existe o error al comunicarse con el servicio de usuarios |

**Ejemplo de Llamada con cURL**:
```bash
curl -X POST "http://localhost:7182/api/activityhistory/registerActivity/john.doe@example.com" \
  -H "Content-Type: application/json" \
  -d '{
    "category": "Login",
    "action": "Usuario inició sesión desde dispositivo móvil"
  }'
```

**Ejemplo de Llamada con JavaScript (Fetch)**:
```javascript
const response = await fetch(
  'http://localhost:7182/api/activityhistory/registerActivity/john.doe@example.com',
  {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      category: 'Login',
      action: 'Usuario inició sesión desde navegador Chrome'
    })
  }
);

const data = await response.json();
console.log(data);
```

---

### 2. Obtener Historial de Actividades por Email

Recupera todas las actividades registradas para un usuario específico identificado por su email.

**Endpoint**: `GET /api/activityhistory/getHistory/{email}`

**Parámetros de URL**:
- `email` (string, requerido): Email del usuario cuyo historial se desea consultar

**Respuesta Exitosa** (200 OK):
```json
[
  {
    "category": "string",
    "action": "string",
    "timeDate": "2026-01-20T02:30:45.123Z"
  },
  {
    "category": "string",
    "action": "string",
    "timeDate": "2026-01-20T03:15:22.456Z"
  }
]
```

**Campos de la Respuesta**:
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `category` | `string` | Categoría de la actividad |
| `action` | `string` | Descripción de la acción realizada |
| `timeDate` | `string` | Timestamp en formato ISO 8601 cuando se registró la actividad |

**Respuestas de Error**:

| Código | Descripción |
|--------|-------------|
| `400 Bad Request` | Email mal formado |
| `500 Internal Server Error` | El usuario no existe o error al consultar la base de datos |

**Ejemplo de Llamada con cURL**:
```bash
curl -X GET "http://localhost:7182/api/activityhistory/getHistory/john.doe@example.com"
```

**Ejemplo de Llamada con JavaScript (Fetch)**:
```javascript
const response = await fetch(
  'http://localhost:7182/api/activityhistory/getHistory/john.doe@example.com'
);

const activities = await response.json();
console.log('Historial de actividades:', activities);
```

---

## Ejemplos Completos del Endpoint Más Complejo

### Endpoint: POST /api/activityhistory/registerActivity/{email}

Este endpoint es el más complejo ya que:
1. Valida la existencia del usuario mediante un microservicio externo
2. Crea una entidad de dominio con timestamp automático
3. Persiste la actividad en la base de datos
4. Retorna una respuesta estructurada con confirmación

#### Ejemplo 1: Registro de Login Exitoso

**Request**:
```http
POST /api/activityhistory/registerActivity/maria.garcia@company.com HTTP/1.1
Host: localhost:7182
Content-Type: application/json

{
  "category": "Authentication",
  "action": "Usuario inició sesión exitosamente desde IP 192.168.1.100"
}
```

**Response**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "usuario": {
    "category": "Authentication",
    "action": "Usuario inició sesión exitosamente desde IP 192.168.1.100",
    "timeDate": "2026-01-20T14:30:45.1234567",
    "userEmail": "maria.garcia@company.com"
  },
  "mensaje": "Actividad registrada exitosamente."
}
```

#### Ejemplo 2: Registro de Compra de Producto

**Request**:
```http
POST /api/activityhistory/registerActivity/carlos.lopez@store.com HTTP/1.1
Host: localhost:7182
Content-Type: application/json

{
  "category": "Purchase",
  "action": "Compró producto 'Laptop Dell XPS 15' por $1,299.99 USD - Order #ORD-2026-001234"
}
```

**Response**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "usuario": {
    "category": "Purchase",
    "action": "Compró producto 'Laptop Dell XPS 15' por $1,299.99 USD - Order #ORD-2026-001234",
    "timeDate": "2026-01-20T15:45:12.9876543",
    "userEmail": "carlos.lopez@store.com"
  },
  "mensaje": "Actividad registrada exitosamente."
}
```

#### Ejemplo 3: Registro de Actualización de Perfil

**Request**:
```http
POST /api/activityhistory/registerActivity/ana.rodriguez@platform.io HTTP/1.1
Host: localhost:7182
Content-Type: application/json

{
  "category": "Profile",
  "action": "Actualizó su foto de perfil y biografía"
}
```

**Response**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "usuario": {
    "category": "Profile",
    "action": "Actualizó su foto de perfil y biografía",
    "timeDate": "2026-01-20T16:20:33.4567890",
    "userEmail": "ana.rodriguez@platform.io"
  },
  "mensaje": "Actividad registrada exitosamente."
}
```

#### Ejemplo 4: Error - Usuario No Existe

**Request**:
```http
POST /api/activityhistory/registerActivity/noexiste@fake.com HTTP/1.1
Host: localhost:7182
Content-Type: application/json

{
  "category": "Login",
  "action": "Intento de inicio de sesión"
}
```

**Response**:
```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "No se pudo registar la actividad en la base de datos"
}
```

#### Ejemplo 5: Error - Request Body Inválido

**Request**:
```http
POST /api/activityhistory/registerActivity/user@example.com HTTP/1.1
Host: localhost:7182
Content-Type: application/json

{
  "wrongField": "This is not valid"
}
```

**Response**:
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "category": ["The category field is required."],
    "action": ["The action field is required."]
  }
}
```

---

## Colección Postman

Para facilitar las pruebas, aquí hay una colección de Postman en formato JSON:

```json
{
  "info": {
    "name": "Activity History Service",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Registrar Actividad",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"category\": \"Login\",\n  \"action\": \"Usuario inició sesión desde navegador\"\n}"
        },
        "url": {
          "raw": "http://localhost:7182/api/activityhistory/registerActivity/test@example.com",
          "protocol": "http",
          "host": ["localhost"],
          "port": "7182",
          "path": ["api", "activityhistory", "registerActivity", "test@example.com"]
        }
      }
    },
    {
      "name": "Obtener Historial",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "http://localhost:7182/api/activityhistory/getHistory/test@example.com",
          "protocol": "http",
          "host": ["localhost"],
          "port": "7182",
          "path": ["api", "activityhistory", "getHistory", "test@example.com"]
        }
      }
    }
  ]
}
```

---

## Códigos de Estado HTTP

La API utiliza los siguientes códigos de estado HTTP:

| Código | Significado | Cuándo se Usa |
|--------|-------------|---------------|
| `200 OK` | Operación exitosa | Request procesado correctamente |
| `400 Bad Request` | Request inválido | Request body mal formado o parámetros faltantes |
| `404 Not Found` | Recurso no encontrado | Endpoint no existe |
| `500 Internal Server Error` | Error del servidor | Usuario no existe, error de BD, o error en dependencias externas |

---

## Headers Recomendados

### Request Headers
```
Content-Type: application/json
Accept: application/json
```

### Response Headers (típicos)
```
Content-Type: application/json; charset=utf-8
Date: Mon, 20 Jan 2026 14:30:45 GMT
Server: Kestrel
```

---

## Limitaciones y Consideraciones

### Paginación
⚠️ **No implementada**: El endpoint `GET /getHistory/{email}` retorna todas las actividades del usuario sin paginación. Esto puede causar problemas de rendimiento con grandes volúmenes de datos.

**Recomendación**: Implementar paginación con query parameters:
```
GET /api/activityhistory/getHistory/{email}?page=1&pageSize=50
```

### Ordenamiento
⚠️ **No especificado**: Las actividades se retornan en el orden que las devuelve la base de datos (por defecto, orden de inserción).

**Recomendación**: Agregar ordenamiento explícito por `timeDate` descendente para mostrar las actividades más recientes primero.

### Filtrado
⚠️ **No implementado**: No es posible filtrar actividades por categoría, rango de fechas, u otros criterios.

**Recomendación**: Agregar query parameters de filtrado:
```
GET /api/activityhistory/getHistory/{email}?category=Login&fromDate=2026-01-01&toDate=2026-01-31
```

### Rate Limiting
⚠️ **No implementado**: No hay límites de tasa para prevenir abuso.

**Recomendación**: Implementar throttling (ej: 100 requests por minuto por IP).

---

## Testing de la API

### Con Swagger UI (Desarrollo)

1. Navega a `http://localhost:7182/swagger`
2. Expande el endpoint que deseas probar
3. Click en "Try it out"
4. Ingresa los parámetros necesarios
5. Click en "Execute"
6. Revisa la respuesta en la sección "Response body"

### Con cURL (Línea de Comandos)

```bash
# Test de registro de actividad
curl -X POST "http://localhost:7182/api/activityhistory/registerActivity/test@example.com" \
  -H "Content-Type: application/json" \
  -d '{"category": "Test", "action": "Prueba de API"}'

# Test de consulta de historial
curl -X GET "http://localhost:7182/api/activityhistory/getHistory/test@example.com"
```

### Con HTTPie (Alternativa más legible)

```bash
# Test de registro de actividad
http POST localhost:7182/api/activityhistory/registerActivity/test@example.com \
  category="Test" \
  action="Prueba de API"

# Test de consulta de historial
http GET localhost:7182/api/activityhistory/getHistory/test@example.com
```

---

## Versionamiento de API

⚠️ **No implementado**: La API actualmente no tiene versionamiento.

**Recomendación para el futuro**:
- Agregar versión en la URL: `/api/v1/activityhistory/...`
- O mediante headers: `Accept: application/vnd.activityhistory.v1+json`

---

## CORS Configuration

La API está configurada para aceptar requests desde:
- **Origin**: `http://localhost:3000`
- **Methods**: Todos (GET, POST, PUT, DELETE, etc.)
- **Headers**: Todos

Si necesitas agregar otro origen, edita `Program.cs`:
```csharp
policy.WithOrigins("http://localhost:3000", "https://tuapp.com")
```

---

## Dependencias de API Externas

⚠️ **Importante**: Este servicio depende del **User Service** (`http://localhost:7181/api/users/`) para validar usuarios.

Si el User Service está caído o no responde:
- Los endpoints retornarán `500 Internal Server Error`
- No se podrán registrar ni consultar actividades

**Verificar disponibilidad del User Service**:
```bash
curl http://localhost:7181/api/users/getIdUser/test@example.com
```

---

## Preguntas Frecuentes (FAQ)

### ¿Puedo registrar actividades para usuarios que no existen?

No. El servicio valida contra el **User Service** antes de registrar cualquier actividad. Si el usuario no existe, la operación falla con error 500.

### ¿Las actividades se registran con la fecha/hora del servidor o del cliente?

Con la del **servidor**. El timestamp se genera automáticamente en el handler usando `DateTime.Now` cuando se procesa el command.

### ¿Hay límite en la longitud de los campos `category` y `action`?

No hay validación explícita en la capa de aplicación, pero hay límites implícitos de la base de datos PostgreSQL (generalmente 255 caracteres para campos string sin especificación).

### ¿Se pueden actualizar o eliminar actividades registradas?

No. La API actualmente solo soporta operaciones de **creación** (POST) y **lectura** (GET). No hay endpoints para UPDATE o DELETE.

### ¿El historial retornado está ordenado?

El orden no está especificado explícitamente, pero típicamente se retornan en orden de inserción (actividades más antiguas primero). Se recomienda ordenar en el cliente si necesitas un orden específico.
