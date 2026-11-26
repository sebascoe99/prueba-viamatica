namespace GestionCaja.Domain.Entities;

/// <summary>
/// Catálogo de estados del usuario. La clave es un código de 3 letras (ACT, INA, PEN, BLO).
/// </summary>
public class UserStatus
{
    public string StatusId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
