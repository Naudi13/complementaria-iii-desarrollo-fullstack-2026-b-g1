# CitasMed · Parcial práctico Corte 3

Sistema de **agenda de citas médicas**: los pacientes reservan citas, los médicos atienden su agenda y el administrador gestiona médicos y citas.
Se usa para responder los tres problemas del parcial: seguridad, integración y pruebas, y despliegue.

| Problema | Documento | Valor |
|---|---|---|
| **P1 — Seguridad (JWT, autenticación vs. autorización, inglés)** | [docs/p1-seguridad-jwt.md](docs/p1-seguridad-jwt.md) | 1.5 |
| **P2 — Integración, CORS y pruebas** | [docs/p2-integracion-pruebas.md](docs/p2-integracion-pruebas.md) | 2.0 |
| **P3 — Dockerfile, despliegue y variables de entorno** | [docs/p3-docker-despliegue.md](docs/p3-docker-despliegue.md) | 1.5 |

## Stack

- **Backend:** .NET 10 · ASP.NET Core Web API · EF Core + SQLite · JWT Bearer · BCrypt · Swagger
- **Frontend:** Angular 21 (standalone, signals, lazy loading) · Reactive Forms
- **Pruebas:** xUnit + Moq + WebApplicationFactory · Vitest
- **Contenedores:** Docker multi-stage · nginx · Docker Compose

## Arquitectura

### Backend en capas (Clean Architecture)

```
backend/
├── src/
│   ├── CitasMed.Domain/          Entidades y reglas de negocio puras (sin dependencias)
│   ├── CitasMed.Application/     Casos de uso por feature (Auth, Usuarios, Medicos, Citas…),
│   │                             DTOs, interfaces de repositorio, excepciones
│   ├── CitasMed.Infrastructure/  EF Core, repositorios, migraciones, JWT, BCrypt
│   └── CitasMed.Api/             Controladores delgados, middleware de errores, auth, CORS
└── tests/
    ├── CitasMed.UnitTests/
    └── CitasMed.IntegrationTests/
```

Dependencias: `Api → Application ← Infrastructure`, `Application → Domain`. El dominio no conoce a nadie.

### Frontend por módulos (features)

```
frontend/src/app/
├── core/       auth (servicio, interceptor, guards), http (errores), layout (Shell)
├── shared/     componentes reutilizables: spinner, alerta de error, badge de estado
└── features/
    ├── auth/      login
    ├── medicos/   listado, filtro por especialidad, alta y baja (Admin)
    └── citas/     mis citas, reservar (Paciente) · agenda (Médico) · todas (Admin)
```

## Roles y endpoints

| Endpoint | Acceso |
|---|---|
| `POST /api/auth/login` | Público |
| `GET /api/auth/me` · `GET /api/medicos` · `GET /api/especialidades` | Autenticado |
| `POST /api/citas` · `GET /api/citas/mias` | Paciente |
| `PATCH /api/citas/{id}/cancelar` | Paciente **dueño** de la cita o Admin |
| `GET /api/citas/agenda` | Médico |
| `PATCH /api/citas/{id}/atender` | Médico **asignado** a la cita |
| `GET /api/citas` · `GET /api/usuarios` · `POST/DELETE /api/medicos` | Admin |

## Cómo ejecutar

### Opción 1 — Docker (recomendada)

```bash
cp .env.example .env
```

Define `JWT_KEY` en `.env` (por ejemplo con `openssl rand -base64 48`) y luego:

```bash
docker compose up --build
```

- App: <http://localhost:8088>
- Swagger: <http://localhost:5080/swagger>

### Opción 2 — Desarrollo local

Requisitos: .NET SDK 10 y Node.js 22.12 o superior.

```bash
cd backend && dotnet run --project src/CitasMed.Api --launch-profile http
```

```bash
cd frontend && npm install && npx ng serve
```

- App: <http://localhost:4200> · API y Swagger: <http://localhost:5142/swagger>

### Usuarios de demostración

Se crean automáticamente en desarrollo (y en Docker si `SEED_DEMO_DATA=true`). Todos usan la contraseña definida en `DbSeeder.DevPassword` ([DbSeeder.cs](backend/src/CitasMed.Infrastructure/Persistence/DbSeeder.cs)).

| Rol | Email |
|---|---|
| Admin | `admin@citasmed.test` |
| Paciente | `paciente@citasmed.test` |
| Paciente | `paciente2@citasmed.test` |
| Médico (Medicina General) | `laura.gomez@citasmed.test` |
| Médico (Pediatría) | `andres.rojas@citasmed.test` |

## Pruebas

```bash
cd backend && dotnet test
```

```bash
cd frontend && npx ng test --watch=false
```

47 pruebas en total: 21 unitarias y 15 de integración en el backend, y 11 en el frontend. El detalle está en [P2](docs/p2-integracion-pruebas.md#3-pruebas).
