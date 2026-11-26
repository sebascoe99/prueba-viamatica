using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class MethodPayment : BaseEntity
{
    public int MethodPaymentId { get; set; }
    public string Description { get; set; } = string.Empty;
}
