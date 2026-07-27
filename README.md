# HelpDesk

Sistema de gestión de tickets de soporte técnico para clientes internos, desarrollado con React, ASP.NET Core .NET 8 y SQL Server.

## Diagrama de arquitectura

La solución separa frontend, backend y persistencia. El backend aplica DDD, CQRS y una arquitectura por capas.

![Diagrama de arquitectura del sistema HelpDesk](docs/diagrams/diagrama-arquitectura.png)

## Diagrama de componentes

Representa los componentes principales de React, los controladores y casos de uso del backend, y su comunicación mediante la API REST.

![Diagrama de componentes del sistema HelpDesk](docs/diagrams/diagrama-componentes.png)

## Diagrama entidad-relación

El modelo conserva usuarios, categorías, tickets y sus historiales. Las reasignaciones, comentarios, cambios de estado y ciclos SLA se almacenan sin perder la trazabilidad.

```mermaid
erDiagram
    USERS {
        uuid id PK
        string full_name
        string email UK
        string password_hash
        string role
        boolean is_active
        int technician_max_active_tickets "nullable"
        uuid supervisor_support_category_id FK "nullable"
        datetimeoffset created_at_utc
        datetimeoffset updated_at_utc
        rowversion row_version
    }

    SUPPORT_CATEGORIES {
        uuid id PK
        string name UK
        string description
        boolean is_active
        datetimeoffset created_at_utc
        datetimeoffset updated_at_utc
        rowversion row_version
    }

    SLA_POLICIES {
        uuid id PK
        uuid support_category_id FK
        string priority
        bigint response_time_ticks
    }

    TECHNICIAN_CATEGORIES {
        uuid technician_user_id PK, FK
        uuid support_category_id PK, FK
    }

    TICKETS {
        uuid id PK
        string ticket_number UK
        string subject
        string description
        uuid creator_user_id FK
        uuid support_category_id FK
        string priority
        string status
        uuid current_technician_user_id FK "nullable"
        boolean is_deleted
        datetimeoffset created_at_utc
        datetimeoffset updated_at_utc
        datetimeoffset resolved_at_utc "nullable"
        datetimeoffset closed_at_utc "nullable"
        rowversion row_version
    }

    TICKET_ASSIGNMENTS {
        uuid id PK
        uuid ticket_id FK
        uuid technician_user_id FK
        uuid assigned_by_user_id FK
        datetimeoffset assigned_at_utc
        datetimeoffset ended_at_utc "nullable"
        string reason "nullable"
    }

    TICKET_COMMENTS {
        uuid id PK
        uuid ticket_id FK
        uuid author_user_id FK
        string type
        string body
        boolean satisfies_resolution_requirement
        datetimeoffset created_at_utc
    }

    TICKET_STATUS_HISTORY {
        uuid id PK
        uuid ticket_id FK
        string previous_status "nullable"
        string new_status
        uuid changed_by_user_id FK "nullable"
        string reason "nullable"
        boolean is_automatic
        datetimeoffset changed_at_utc
    }

    TICKET_SLA_CYCLES {
        uuid id PK
        uuid ticket_id FK
        string trigger
        uuid support_category_id FK
        string priority
        bigint duration_ticks
        datetimeoffset started_at_utc
        datetimeoffset deadline_at_utc
        datetimeoffset responded_at_utc "nullable"
        datetimeoffset breached_at_utc "nullable"
        uuid responsible_technician_user_id FK "nullable"
        string outcome
    }

    TICKET_NUMBER_SEQUENCES {
        int year PK
        int last_value
    }

    USERS ||--o{ TICKETS : "crea"
    USERS o|--o{ TICKETS : "atiende actualmente"
    SUPPORT_CATEGORIES ||--o{ TICKETS : "clasifica"
    SUPPORT_CATEGORIES ||--|{ SLA_POLICIES : "define"
    USERS ||--o{ TECHNICIAN_CATEGORIES : "tiene especialidades"
    SUPPORT_CATEGORIES ||--o{ TECHNICIAN_CATEGORIES : "habilita técnicos"
    SUPPORT_CATEGORIES o|--o{ USERS : "categoría de supervisor"
    TICKETS ||--o{ TICKET_ASSIGNMENTS : "registra asignaciones"
    USERS ||--o{ TICKET_ASSIGNMENTS : "recibe como técnico"
    USERS ||--o{ TICKET_ASSIGNMENTS : "asigna"
    TICKETS ||--o{ TICKET_COMMENTS : "contiene"
    USERS ||--o{ TICKET_COMMENTS : "escribe"
    TICKETS ||--o{ TICKET_STATUS_HISTORY : "mantiene historial"
    USERS o|--o{ TICKET_STATUS_HISTORY : "cambia estado"
    TICKETS ||--o{ TICKET_SLA_CYCLES : "genera ciclos"
    SUPPORT_CATEGORIES ||--o{ TICKET_SLA_CYCLES : "origina SLA"
    USERS o|--o{ TICKET_SLA_CYCLES : "responsable técnico"
```

Los técnicos pueden atender varias categorías mediante `technician_categories`. Cada categoría define un SLA por prioridad y cada ticket conserva sus asignaciones, comentarios, estados y ciclos SLA.

## Ejecución local

Requiere Docker Desktop.

1. Cree el archivo de variables de entorno y configure sus contraseñas:

   ```powershell
   Copy-Item .env.example .env
   ```

2. Levante frontend, API y SQL Server:

   ```powershell
   docker compose --profile frontend up --build -d
   ```

3. Abra:

   - Frontend: `http://localhost:5173`
   - Swagger: `http://localhost:8080/swagger/index.html`
   - SQL Server: `localhost,14330`

Los puertos pueden modificarse desde `.env`. Para detener los contenedores sin eliminar la base de datos:

```powershell
docker compose down
```
