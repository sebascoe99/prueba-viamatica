using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class UserCash : BaseEntity
{
    public int UserCashId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int CashId { get; set; }
    public Cash Cash { get; set; } = null!;

    public int AssignedBy { get; set; }
}
