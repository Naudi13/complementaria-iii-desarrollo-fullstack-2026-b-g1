# Gestor de Tareas — App fullstack segura (JWT) · .NET + Angular

CORHUILA · Ingeniería de Sistemas · Desarrollo FullStack 2026-B · Corte 3 (Semana 14) Autor: Victor Alfonso Tovar Ramirez (@Naudi13)

## Estructura de carpetas

```
c3-activity/
├── backend/                     # API ASP.NET Core 8
│   ├── Program.cs               # JWT + CORS + servicios
│   ├── appsettings.json         # Jwt, Cors, usuario demo
│   ├── Controllers/             # AuthController (login), TasksController (protegido)
│   ├── Services/                # TokenService (JWT), PasswordHasher (PBKDF2)
│   ├── Data/                    # Repositorios en memoria (tareas, usuarios)
│   ├── Models/  Dtos/
├── backend.Tests/               # Pruebas xUnit (integración HTTP)
├── frontend/                    # Angular 22 (componentes standalone)
│   └── src/app/
│       ├── core/                # auth.service, auth.interceptor, auth.guard, tasks.service
│       └── pages/               # login, tasks (vista protegida con datos)
├── Dockerfile                   # Compila backend + frontend y los sirve juntos
└── docker-compose.yml
```

## Usuario de demostración

| Usuario | Contraseña  |
|---------|-------------|
| `admin` | `Admin123!` |

(Configurable en `backend/appsettings.json` → `Auth`.)

## Ejecutar en desarrollo

Backend (puerto **5080**):

```bash
cd backend
dotnet run
```

Frontend (puerto **4200**, en otra terminal):

```bash
cd frontend
npm install
npm start
```

Abre http://localhost:4200. El frontend llama a `http://localhost:5080/api`; al ser otro origen,
el backend habilita **CORS** solo para `http://localhost:4200` (ver `Cors:AllowedOrigins`).

## Endpoints

| Método | Ruta                      | Protegido | Descripción                  |
|--------|---------------------------|-----------|------------------------------|
| POST   | `/api/auth/login`         | No        | Devuelve el JWT              |
| GET    | `/api/tasks`              | Sí        | Lista las tareas             |
| POST   | `/api/tasks`              | Sí        | Crea una tarea (201)         |
| PATCH  | `/api/tasks/{id}/toggle`  | Sí        | Marca / desmarca como hecha  |
| DELETE | `/api/tasks/{id}`         | Sí        | Elimina una tarea            |

Códigos de estado: `200` éxito, `201` creado, `400` datos inválidos, `401` sin token / token
inválido / credenciales incorrectas, `404` tarea inexistente.

## Pruebas

```bash
# Backend (xUnit): login correcto e incorrecto, 401 sin token / token falso, 400, 201, CORS
dotnet test backend.Tests

# Frontend (Vitest): servicio de auth, interceptor, login y vista de tareas
cd frontend && npm test
```

Casos de error cubiertos: login con contraseña incorrecta (401), endpoint sin token (401), token
manipulado (401), título vacío (400), y en el frontend: error 401 en login, error 500 al cargar
tareas y 401 en el interceptor (cierra sesión y redirige).

## Docker

```bash
docker compose up --build
```

Abre http://localhost:8080 (la misma imagen sirve Angular y la API).

## Security & run

The app uses a JWT-based authentication flow. The user sends a username and password to
`POST /api/auth/login`; if they are valid, the API returns a signed token (HS256) that expires
after 60 minutes, and passwords are never stored in plain text because only a salted PBKDF2 hash
is kept. The Angular frontend saves the token and an HTTP interceptor adds the `Authorization: Bearer`
header to every request to the API, while a route guard blocks the tasks page when there is no
session. Every `/api/tasks` endpoint is protected with `[Authorize]`, so a missing or invalid token
gets a `401` response. The backend has xUnit integration tests and the frontend has Vitest tests,
and both include error cases. To
run everything with Docker, build the image with `docker compose up --build .`, then open `http://localhost:8080`.
