namespace CitasMed.Application.Common.Interfaces;

/// <summary>
/// Abstracción del hashing de contraseñas. La implementación (BCrypt)
/// vive en Infrastructure, así Application no depende de la librería.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
