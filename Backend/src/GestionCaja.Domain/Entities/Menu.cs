using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Menu : BaseEntity
{
    public int MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<RolMenu> RolMenus { get; set; } = new List<RolMenu>();
}
