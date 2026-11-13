using CitasMed.Application.Common.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CitasMed.Api.Services;

/// <summary>
/// Implementa ICurrentUser leyendo los claims del JWT ya validado por el middleware.
/// </summary>
public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private System.Security.Claims.ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int? Id =>
        int.TryParse(Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : null;

    public string? Rol => Principal?.FindFirst("role")?.Value;
}
