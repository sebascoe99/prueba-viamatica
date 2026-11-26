namespace GestionCaja.Domain.Common;

/// <summary>
/// Campos comunes de auditoría y eliminación lógica.
/// Ningún registro se borra físicamente: se marca IsDeleted = true.
/// </summary>
public abstract class BaseEntity
{
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
