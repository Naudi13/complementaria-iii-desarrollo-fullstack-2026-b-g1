using System.ComponentModel.DataAnnotations;

namespace CitasMed.Application.Features.Auth;

// En un record posicional, las validaciones van sobre los parámetros del constructor.
public record LoginRequest(
    [Required(ErrorMessage = "El email es obligatorio."),
     EmailAddress(ErrorMessage = "El email no es válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Password);
