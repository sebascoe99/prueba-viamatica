using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

/// <summary>
/// Asignación de un cajero a una caja. La hace el gestor; máximo 2 cajeros por caja.
/// </summary>
public class UserCash : BaseEntity
{
    public int UserCashId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int CashId { get; set; }
    public Cash Cash { get; set; } = null!;

    // Gestor que realizó la asignación
    public int AssignedBy { get; set; }
}
