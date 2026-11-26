using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class User : BaseEntity
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Estos tres campos se guardan protegidos en la base de datos:
    // Email e Identification cifrados (reversible) y Password como hash (irreversible).
    public string Email { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public string UserStatusId { get; set; } = string.Empty;
    public UserStatus UserStatus { get; set; } = null!;

    // Trazabilidad de quién creó y quién aprobó al usuario
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public int? UserCreate { get; set; }
    public int? UserApproval { get; set; }
    public DateTime? DateApproval { get; set; }

    // Datos de sesión, usados por los indicadores del dashboard
    public bool SessionActive { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? LastLogin { get; set; }
}
