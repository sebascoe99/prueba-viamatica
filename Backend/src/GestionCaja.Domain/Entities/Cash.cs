using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Cash : BaseEntity
{
    public int CashId { get; set; }
    public string CashDescription { get; set; } = string.Empty;
    public bool Active { get; set; } = true;

    // Cajero que está operando la caja en este momento (null = caja libre).
    // Con esto se controla que los 2 cajeros asignados no la usen al mismo tiempo.
    public int? CurrentUserId { get; set; }
    public User? CurrentUser { get; set; }
    public DateTime? OpenedAt { get; set; }

    public ICollection<UserCash> UserCashes { get; set; } = new List<UserCash>();
    public ICollection<Turn> Turns { get; set; } = new List<Turn>();
}
