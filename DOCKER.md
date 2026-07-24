# Ejecución local con Docker

## Servicios disponibles

- `sqlserver`: SQL Server 2022 Developer con volumen persistente.
- `api`: API .NET 8, migraciones de Development, bootstrap del primer SuperAdmin y Swagger.
- `frontend`: servicio opcional para React/Vite; no se construye hasta activar el perfil `frontend`.

## Primera ejecución

1. Copie `.env.example` como `.env`.
2. Reemplace todas las contraseñas y la clave JWT.
3. Inicie API y SQL Server:

```powershell
docker compose up --build -d
```

4. Abra:

```text
http://localhost:8080/swagger/index.html
```

## Comandos útiles

```powershell
docker compose ps
docker compose logs -f api
docker compose down
```

`docker compose down` conserva la base de datos. Para eliminar también el volumen local:

```powershell
docker compose down --volumes
```

## Frontend

El servicio `frontend` está bajo un perfil para que el backend pueda iniciar mientras la carpeta solo contiene el marcador inicial.

Cuando el proyecto React/Vite incluya `package.json`, `package-lock.json` y genere `dist`, se inicia todo con:

```powershell
docker compose --profile frontend up --build -d
```

El frontend quedará en `http://localhost:5173` y recibirá `VITE_API_BASE_URL` durante la compilación.
