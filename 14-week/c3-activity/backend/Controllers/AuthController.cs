// Controllers/AuthController.cs
// POST /api/auth/login: valida credenciales y devuelve un JWT.
// Es el único endpoint público de la API (junto con la raíz "/").
using Backend.Api.Data;
using Backend.Api.Dtos;
using Backend.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserRepository _users;
    private readonly TokenService _tokens;

    public AuthController(UserRepository users, TokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { ok = false, error = "Usuario y contraseña son obligatorios" });
        }

        var user = _users.FindByUsername(request.Username);

        // Mismo mensaje si falla el usuario o la contraseña: no revela cuál de los dos falló.
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { ok = false, error = "Credenciales inválidas" });
        }

        var (token, expiresAt) = _tokens.CreateToken(user);
        return Ok(new LoginResponse(token, expiresAt, user.Username));
    }
}
