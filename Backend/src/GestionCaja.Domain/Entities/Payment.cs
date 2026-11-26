using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Payment : BaseEntity
{
    public int PaymentId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public int MethodPaymentId { get; set; }
    public MethodPayment MethodPayment { get; set; } = null!;

    // Cajero que registró el pago
    public int UserCashierId { get; set; }
    public User UserCashier { get; set; } = null!;
}
