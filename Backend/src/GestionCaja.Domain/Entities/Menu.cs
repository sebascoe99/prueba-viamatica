using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

/// <summary>
/// Opción del menú del front. Se carga desde la base según el rol del usuario.
/// </summary>
public class Menu : BaseEntity
{
    public int MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<RolMenu> RolMenus { get; set; } = new List<RolMenu>();
}
