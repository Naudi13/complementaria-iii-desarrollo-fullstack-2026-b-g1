// Models/User.cs
// Usuario del sistema. Nunca se guarda la contraseña en claro: solo su hash.
namespace Backend.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
