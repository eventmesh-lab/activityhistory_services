# Activity History Service

## Descripción

El **Activity History Service** es un microservicio desarrollado en .NET 8 que registra y gestiona el historial de actividades de usuarios. Permite rastrear acciones específicas realizadas por usuarios identificados mediante su correo electrónico, proporcionando un registro cronológico completo de sus actividades en el sistema.

### Problema de Negocio que Resuelve

Este servicio soluciona la necesidad de:
- **Auditoría de Actividades**: Mantener un registro detallado de las acciones realizadas por cada usuario
- **Trazabilidad**: Permitir el seguimiento temporal de las actividades para análisis y cumplimiento normativo
- **Análisis de Comportamiento**: Proporcionar datos históricos para identificar patrones de uso y comportamiento de usuarios
- **Integración entre Microservicios**: Actuar como servicio centralizado de registro de actividades para otros microservicios del ecosistema

## Tabla de Contenidos

- [Arquitectura](docs/architecture.md) - Flujo de datos, dependencias externas y modelo de datos
- [API](docs/api.md) - Documentación completa de endpoints y ejemplos de uso
- [Configuración](docs/setup.md) - Guía detallada de instalación y configuración

## Stack Tecnológico

| Tecnología | Versión | Propósito |
|-----------|---------|-----------|
| **.NET** | 8.0 | Framework principal del servicio |
| **C#** | 12.0 | Lenguaje de programación |
| **ASP.NET Core** | 8.0 | Framework web para la API REST |
| **PostgreSQL** | 15 | Base de datos relacional |
| **Entity Framework Core** | 8.0.22 | ORM para acceso a datos |
| **MediatR** | 14.0.0 | Implementación de patrón CQRS/Mediator |
| **Docker** | - | Contenedorización del servicio |
| **Swagger/OpenAPI** | 6.6.2 | Documentación interactiva de la API |

## Quick Start

### Opción 1: Docker Compose (Recomendado)

```bash
# Levantar el servicio completo (API + Base de datos)
docker-compose up

# El servicio estará disponible en: http://localhost:7182
# Swagger UI en: http://localhost:7182/swagger
```

### Opción 2: Ejecución Local con .NET

```bash
# Restaurar dependencias
dotnet restore

# Ejecutar migraciones de base de datos
dotnet ef database update --project src/activityhistory_infrastructure --startup-project src/activityhistory_api

# Iniciar el servicio
dotnet run --project src/activityhistory_api

# El servicio estará disponible en: http://localhost:7182
```

### Verificación Rápida

```bash
# Registrar una actividad
curl -X POST "http://localhost:7182/api/activityhistory/registerActivity/user@example.com" \
  -H "Content-Type: application/json" \
  -d '{"category": "Login", "action": "User logged in successfully"}'

# Obtener historial de actividades
curl -X GET "http://localhost:7182/api/activityhistory/getHistory/user@example.com"
```

## Estructura del Proyecto

El proyecto sigue la **Arquitectura Limpia (Clean Architecture)** organizada en capas:

```
activityhistory_services/
├── src/
│   ├── activityhistory_api/          # Capa de Presentación (Controllers, API)
│   ├── activityhistory_application/  # Capa de Aplicación (CQRS, DTOs)
│   ├── activityhistory_domain/       # Capa de Dominio (Entidades, Interfaces)
│   └── activityhistory_infrastructure/ # Capa de Infraestructura (Persistencia, Servicios)
├── tests/                            # Pruebas unitarias y de integración
├── docs/                             # Documentación técnica
├── Dockerfile                        # Definición de imagen Docker
└── docker-compose.yml                # Orquestación de contenedores
```

## Licencia

Este proyecto es de uso interno para el ecosistema de microservicios EventMesh Lab.

## Contacto y Soporte

Para dudas, sugerencias o reportar problemas, consulte la documentación en la carpeta `docs/` o contacte al equipo de desarrollo.
