using CitasMed.Application.Common.Exceptions;
using CitasMed.Application.Common.Interfaces;
using CitasMed.Application.Features.Auth;
using CitasMed.Application.Features.Usuarios;
using CitasMed.Domain.Entities;
using CitasMed.Domain.Enums;
using Moq;

namespace CitasMed.UnitTests.Application;

/// <summary>
/// Prueba unitaria del login: se aísla AuthService reemplazando
/// repositorio, hasher y generador de JWT por mocks.
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly AuthService _sut;

    private readonly User _paciente = new()
    {
        Id = 5, Nombre = "Ana", Email = "ana@citasmed.test", PasswordHash = "hash", Rol = Rol.Paciente
    };

    public AuthServiceTests()
    {
        _sut = new AuthService(_users.Object, _hasher.Object, _jwt.Object);
    }

    [Fact]
    public async Task LoginAsync_ConCredencialesValidas_DevuelveTokenYUsuario()
    {
        _users.Setup(r => r.GetByEmailAsync("ana@citasmed.test", It.IsAny<CancellationToken>())).ReturnsAsync(_paciente);
        _hasher.Setup(h => h.Verify("correcta", "hash")).Returns(true);
        _jwt.Setup(j => j.Generate(_paciente)).Returns(new TokenResult("jwt-de-prueba", DateTime.UtcNow.AddHours(1)));

        // Email con mayúsculas y espacios: debe normalizarse antes de buscar.
        var result = await _sut.LoginAsync(new LoginRequest("  ANA@citasmed.test ", "correcta"));

        Assert.Equal("jwt-de-prueba", result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal("Paciente", result.Usuario.Rol);
        _jwt.Verify(j => j.Generate(_paciente), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ConPasswordIncorrecta_LanzaUnauthorizedYNoEmiteToken()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(_paciente);
        _hasher.Setup(h => h.Verify("mala", "hash")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest("ana@citasmed.test", "mala")));

        _jwt.Verify(j => j.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ConEmailInexistente_LanzaUnauthorizedConMismoMensaje()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest("nadie@citasmed.test", "x")));

        // Mismo mensaje que con contraseña incorrecta: no revela qué emails existen.
        Assert.Equal("Email o contraseña incorrectos.", ex.Message);
    }
}
