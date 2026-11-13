using CitasMed.Domain.Entities;

namespace CitasMed.Application.Common.Interfaces;

/// <summary>Token emitido tras un login exitoso.</summary>
public record TokenResult(string AccessToken, DateTime ExpiresAt);

/// <summary>
/// Emite tokens JWT. Application solo conoce el contrato;
/// la firma y los claims se resuelven en Infrastructure.
/// </summary>
public interface IJwtService
{
    TokenResult Generate(User user);
}
