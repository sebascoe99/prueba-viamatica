namespace GestionCaja.Domain.Entities;

/// <summary>
/// Tabla intermedia: qué opciones de menú puede ver cada rol.
/// </summary>
public class RolMenu
{
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public int MenuId { get; set; }
    public Menu Menu { get; set; } = null!;
}
