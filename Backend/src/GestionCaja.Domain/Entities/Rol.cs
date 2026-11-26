using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Rol : BaseEntity
{
    public int RolId { get; set; }
    public string RolName { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RolMenu> RolMenus { get; set; } = new List<RolMenu>();
}
