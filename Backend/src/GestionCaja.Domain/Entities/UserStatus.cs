namespace GestionCaja.Domain.Entities;

public class UserStatus
{
    public string StatusId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
