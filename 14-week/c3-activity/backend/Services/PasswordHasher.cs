// Services/PasswordHasher.cs
// Hash de contraseñas con PBKDF2 (SHA-256, salt aleatorio e iteraciones altas).
// Formato guardado: iteraciones.salt.hash (todo en Base64).
using System.Security.Cryptography;

namespace Backend.Api.Services;

public static class PasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations)) return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        // Comparación en tiempo constante para no filtrar información por timing.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
