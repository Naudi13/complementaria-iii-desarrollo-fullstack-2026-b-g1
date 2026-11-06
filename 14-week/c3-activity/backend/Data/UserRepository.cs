// Data/UserRepository.cs
// Usuarios en memoria. El usuario de demostración se crea al arrancar
// con la contraseña tomada de la configuración (ver appsettings.json).
using Backend.Api.Models;
using Backend.Api.Services;

namespace Backend.Api.Data;

public class UserRepository
{
    private readonly List<User> _users = new();

    public UserRepository(IConfiguration config)
    {
        var username = config["Auth:DemoUser"] ?? "admin";
        var password = config["Auth:DemoPassword"] ?? "Admin123!";
        _users.Add(new User
        {
            Id = 1,
            Username = username,
            PasswordHash = PasswordHasher.Hash(password),
        });
    }

    public User? FindByUsername(string username) =>
        _users.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
}
