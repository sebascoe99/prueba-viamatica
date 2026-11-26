namespace GestionCaja.Domain.Entities;

public class RolMenu
{
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public int MenuId { get; set; }
    public Menu Menu { get; set; } = null!;
}
