using System.ComponentModel.DataAnnotations;

namespace GestionCaja.Application.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string RolName { get; set; } = string.Empty;
}
