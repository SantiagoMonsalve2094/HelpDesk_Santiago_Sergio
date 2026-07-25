# Frontend HelpDesk

Aplicación React para operar el sistema de gestión de tickets de soporte técnico. Consume la API .NET expuesta por `backend/Presentation/HelpDesk.Backend.Api` y respeta el alcance por rol que valida el backend.

## Ejecución local

1. Levantar la API en `http://localhost:8080`.
2. Instalar dependencias en `frontend`.
3. Ejecutar `npm run dev`.
4. Abrir `http://localhost:5173`.

La URL de API se configura con `VITE_API_BASE_URL`. Si no se define, usa `http://localhost:8080`.

## Vistas incluidas

- Login con `POST /api/auth/login`.
- Tickets con filtros, creación, detalle, comentarios y acciones del flujo.
- Categorías con consulta de políticas SLA y actualización de tiempos por prioridad para roles autorizados.
- Reporte SLA y alertas para Supervisor y SuperAdmin.
- Usuarios para SuperAdmin.

## Roles

- Cliente: ve y crea sus propios tickets.
- Técnico: ve tickets asignados por las reglas de Application y puede iniciar o resolver.
- Supervisor: ve tickets de su alcance, asigna, reasigna, consulta técnicos asignables, alertas y reportes.
- SuperAdmin: administra usuarios, categorías y ve el alcance global.
