// Dtos/AuthDtos.cs
// Objetos de entrada/salida de la API de autenticación y tareas.
namespace Backend.Api.Dtos;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt, string Username);

public record CreateTaskRequest(string Title);
