# Diagrama Entidad-Relación

## Sistema de Gestión de Tickets HelpDesk

El siguiente modelo representa las tablas persistidas por Entity Framework Core en SQL Server. Los perfiles de técnico y supervisor son objetos propios embebidos en `users`; el perfil de técnico posee una colección persistida en `technician_categories`.

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

## Relaciones principales

| Relación | Cardinalidad | Descripción |
|---|---:|---|
| `USERS` - `TICKETS` por `creator_user_id` | 1:N | Un usuario puede crear muchos tickets; cada ticket tiene un creador. |
| `USERS` - `TICKETS` por `current_technician_user_id` | 0..1:N | Un ticket puede tener técnico actual; un técnico puede atender varios tickets según su capacidad. |
| `SUPPORT_CATEGORIES` - `TICKETS` | 1:N | Cada ticket pertenece a una categoría activa. |
| `SUPPORT_CATEGORIES` - `SLA_POLICIES` | 1:4 o más | Cada categoría debe tener una política para cada prioridad: baja, media, alta y crítica. La unicidad se garantiza por categoría y prioridad. |
| `USERS` - `TECHNICIAN_CATEGORIES` - `SUPPORT_CATEGORIES` | N:M | Un técnico puede atender varias categorías y una categoría puede tener varios técnicos habilitados. |
| `TICKETS` - `TICKET_ASSIGNMENTS` | 1:N | Conserva el historial de asignaciones y reasignaciones. |
| `TICKETS` - `TICKET_COMMENTS` | 1:N | Conserva comentarios generales, de resolución y de justificación. |
| `TICKETS` - `TICKET_STATUS_HISTORY` | 1:N | Registra el flujo de estados y si el cambio fue automático. |
| `TICKETS` - `TICKET_SLA_CYCLES` | 1:N | Registra el SLA inicial y los ciclos posteriores asociados a reaperturas o cambios del flujo. |
| `USERS` - `TICKET_SLA_CYCLES` | 0..1:N | Un ciclo puede identificar al técnico responsable de la atención. |

## Reglas representadas en el modelo

- `users.role` determina el perfil. Los roles `User` y `SuperAdmin` no tienen perfil adicional.
- Un `Technician` requiere `technician_max_active_tickets` y al menos una fila en `technician_categories`.
- Un `Supervisor` requiere `supervisor_support_category_id`.
- `TICKET_COMMENTS.satisfies_resolution_requirement` permite comprobar que existe comentario de resolución antes del cierre.
- `TICKET_SLA_CYCLES.outcome`, `deadline_at_utc` y `breached_at_utc` permiten detectar vencimientos.
- `TICKET_STATUS_HISTORY.is_automatic` distingue transiciones realizadas por el sistema, como el vencimiento SLA.
- Las colecciones dependientes del ticket se eliminan en cascada al eliminar físicamente el ticket; el ticket usa además `is_deleted` y un filtro global para borrado lógico.
- Las relaciones con usuarios y categorías usan restricciones de no acción al eliminar para proteger la integridad histórica.

## Índices y restricciones relevantes

- Unicidad en `users.email`, `support_categories.name` y `tickets.ticket_number`.
- Unicidad compuesta en `sla_policies(support_category_id, priority)`.
- Clave compuesta en `technician_categories(technician_user_id, support_category_id)`.
- Índices de consulta para creador, técnico actual, categoría, estado y fechas de SLA.
- Restricciones de base de datos para capacidad positiva de técnicos y duración positiva de las políticas/ciclos SLA.
- `ticket_number_sequences` controla la numeración anual de tickets y no se relaciona por FK con el agregado `tickets` porque el número se materializa en `ticket_number`.

