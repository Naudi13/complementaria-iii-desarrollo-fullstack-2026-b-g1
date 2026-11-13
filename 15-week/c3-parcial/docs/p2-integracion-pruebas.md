# Problema 2 — Integración y pruebas

## 1. Cómo el frontend consume un endpoint protegido

La responsabilidad está repartida en capas, cada una con una sola tarea:

```mermaid
flowchart LR
    C[Componente<br/>MisCitasPage] -->|getMias()| S[CitaService]
    S -->|HttpClient GET /api/citas/mias| I1[authInterceptor<br/>agrega Bearer token]
    I1 --> I2[errorInterceptor<br/>401 → logout]
    I2 --> API[(CitasMed.Api)]
    API -->|200 / 401 / 403 / 409| I2
    I2 --> C
    C --> V{{Vista: spinner · error · datos}}
```

| Pieza | Archivo | Responsabilidad |
|---|---|---|
| Servicio por feature | [`cita.service.ts`](../frontend/src/app/features/citas/services/cita.service.ts) | Solo conoce URLs y tipos; no maneja tokens |
| `authInterceptor` | [`auth.interceptor.ts`](../frontend/src/app/core/auth/auth.interceptor.ts) | Añade `Authorization: Bearer <token>` solo hacia la API |
| `errorInterceptor` | [`error.interceptor.ts`](../frontend/src/app/core/http/error.interceptor.ts) | `401` (token vencido o inválido) → cierra sesión y lleva a `/login?expired=1` |
| `getErrorMessage` | [`api-error.ts`](../frontend/src/app/core/http/api-error.ts) | Convierte la respuesta `ProblemDetails` de la API en un mensaje legible |
| `authGuard` / `roleGuard` | [`core/auth`](../frontend/src/app/core/auth) | Evitan navegar a pantallas sin sesión o sin el rol (el backend vuelve a validar) |

**Manejo de carga y error** en el componente ([`mis-citas-page.ts`](../frontend/src/app/features/citas/pages/mis-citas-page/mis-citas-page.ts)):

```ts
protected readonly loading = signal(false);
protected readonly error = signal<string | null>(null);
protected readonly citas = signal<Cita[]>([]);

load(): void {
  this.loading.set(true);
  this.error.set(null);
  this.citaService.getMias()
    .pipe(finalize(() => this.loading.set(false)))        // se apaga en éxito y en error
    .subscribe({
      next: (data) => this.citas.set(data),
      error: (err) => this.error.set(getErrorMessage(err)), // "El médico ya tiene una cita en ese horario."
    });
}
```

```html
@if (loading()) {
  <app-loading-spinner message="Cargando tus citas..." />
} @else if (error(); as message) {
  <app-error-alert [message]="message" [retryable]="true" (retry)="load()" />
} @else {
  <app-cita-list [citas]="citas()" ... />
}
```

Además, en las acciones (cancelar, atender) el botón se deshabilita mientras la petición está en curso (`procesandoId`) para evitar dobles envíos. El error de la acción se muestra sin perder la lista ya cargada.

Del lado del backend, todas las respuestas de error tienen el mismo formato **ProblemDetails** (RFC 9457), gracias a [`GlobalExceptionHandler`](../backend/src/CitasMed.Api/Middleware/GlobalExceptionHandler.cs). Por eso el frontend puede mostrar `detail` o `errors` sin conocer cada endpoint:

```json
{ "status": 409, "title": "Conflicto", "detail": "El médico ya tiene una cita en ese horario." }
```

## 2. CORS

**El problema:** en desarrollo, Angular corre en `http://localhost:4200` y la API en `http://localhost:5142`. Son **orígenes distintos**, así que el navegador bloquea la respuesta salvo que la API lo autorice. Como la petición lleva el header `Authorization`, el navegador además envía antes un *preflight* `OPTIONS`.

**Solución en desarrollo:** una política CORS con **lista blanca** leída de configuración ([`CorsExtensions.cs`](../backend/src/CitasMed.Api/Extensions/CorsExtensions.cs)):

```csharp
var origins = configuration["Cors:AllowedOrigins"]!.Split(',', StringSplitOptions.TrimEntries);

services.AddCors(o => o.AddPolicy("Frontend", p => p
    .WithOrigins(origins)                         // nunca AllowAnyOrigin en producción
    .WithHeaders("Authorization", "Content-Type")
    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")));

app.UseCors("Frontend");      // ANTES de UseAuthentication: el preflight no lleva token
```

Resultado verificado:

| Preflight desde | Respuesta |
|---|---|
| `http://localhost:4200` (permitido) | `204` + `Access-Control-Allow-Origin: http://localhost:4200` |
| `http://evil.test` (no permitido) | sin `Access-Control-Allow-Origin` → el navegador bloquea |

El origen se cambia **sin recompilar**: `Cors__AllowedOrigins=http://localhost:4200,https://citasmed.midominio.com`.

**Solución en producción (Docker):** nginx sirve el frontend y reenvía `/api` al contenedor de la API ([`nginx.conf`](../frontend/nginx.conf)). El navegador ve **un solo origen**, así que no hay petición cross-origin y no hace falta CORS. Por eso `environment.ts` usa `apiUrl: '/api'` y `environment.development.ts` usa `http://localhost:5142/api`.

## 3. Pruebas

### Prueba unitaria propuesta: `CitaService.CrearAsync` con horario ocupado

[`CitaServiceTests.cs`](../backend/tests/CitasMed.UnitTests/Application/CitaServiceTests.cs)

```csharp
[Fact]
public async Task CrearAsync_EnHorarioOcupado_LanzaConflictYNoGuarda()
{
    ComoUsuario(PacienteId, Rol.Paciente);
    _citas.Setup(c => c.HorarioOcupadoAsync(MedicoId, _manana9am, It.IsAny<CancellationToken>()))
          .ReturnsAsync(true);

    await Assert.ThrowsAsync<ConflictException>(() =>
        CrearServicio().CrearAsync(new CrearCitaRequest(MedicoId, _manana9am, "Control")));

    _citas.Verify(c => c.Add(It.IsAny<Cita>()), Times.Never);
    _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}
```

**Qué verifica:** la **regla de negocio** de forma aislada. Si el médico ya tiene una cita en ese horario, se rechaza con `ConflictException` y **no se guarda nada**. No usa base de datos ni HTTP: los repositorios son *mocks* (Moq) y el reloj es un `FakeTimeProvider`, así que la prueba es rápida (milisegundos) y determinista.

### Prueba de integración propuesta: seguridad del endpoint de citas

[`AuthEndpointsTests.cs`](../backend/tests/CitasMed.IntegrationTests/AuthEndpointsTests.cs) / [`CitasEndpointsTests.cs`](../backend/tests/CitasMed.IntegrationTests/CitasEndpointsTests.cs)

```csharp
public class CitasEndpointsTests(CitasMedApiFactory factory) : IClassFixture<CitasMedApiFactory>
{
    [Fact]
    public async Task Cancelar_CitaDeOtroPaciente_Devuelve403()
    {
        var duena = await factory.CreateClientAsAsync("paciente@citasmed.test");   // login real → JWT
        var otro  = await factory.CreateClientAsAsync("paciente2@citasmed.test");
        var medicoId = await MedicoIdDeLauraAsync(duena);
        var crear = await duena.PostAsJsonAsync("/api/citas", new CrearCitaRequest(medicoId, Manana(10), "Control"));
        var cita  = await crear.Content.ReadFromJsonAsync<CitaDto>();

        var response = await otro.PatchAsync($"/api/citas/{cita!.Id}/cancelar", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
```

**Qué verifica:** que **todas las capas funcionan juntas**. `WebApplicationFactory<Program>` levanta la API real en memoria (routing, CORS, middleware JWT, autorización por rol, controlador, servicio, EF Core) contra una base **SQLite temporal** con migraciones y seed. Se hace login de verdad, se usa el token emitido y se comprueba el código HTTP final.

### Inventario de pruebas

| Proyecto | Tipo | Cantidad | Qué cubre |
|---|---|---|---|
| `CitasMed.UnitTests` | Unitarias (xUnit + Moq) | 21 | Reglas de `Cita` (fecha futura, franjas de 30 min, horario, estados), `AuthService` (login válido, contraseña incorrecta, email inexistente), `CitaService` (crear, conflicto, médico inexistente, cancelar propia, ajena o como Admin, atender sin ser el asignado) |
| `CitasMed.IntegrationTests` | Integración (WebApplicationFactory) | 15 | Login `200`/`401`; `401` sin token o con firma alterada; `403` por rol; `200` admin; CORS permitido y bloqueado; flujo reservar → agenda → atender; `409` horario ocupado; `400` fecha pasada; `403` cita ajena |
| `frontend` | Unitarias (Vitest) | 11 | `authInterceptor` (agrega Bearer, no sin sesión, no a dominios externos), `errorInterceptor` (logout en 401, no en 403 ni en login), `authGuard`, `roleGuard` |

**Total: 47 pruebas, todas en verde.**

### Cómo ejecutarlas

```bash
cd backend && dotnet test
```

```bash
cd frontend && npx ng test --watch=false
```

```text
Correctas! - Con error: 0, Superado: 21, Total: 21 - CitasMed.UnitTests.dll
Correctas! - Con error: 0, Superado: 15, Total: 15 - CitasMed.IntegrationTests.dll
Test Files  4 passed (4)   Tests  11 passed (11)
```
