using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Contract : BaseEntity
{
    public int ContractId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public string StatusContractId { get; set; } = string.Empty;
    public StatusContract StatusContract { get; set; } = null!;

    public int MethodPaymentId { get; set; }
    public MethodPayment MethodPayment { get; set; } = null!;

    // En un cambio de servicio el contrato anterior queda sustituido (SUS) y se crea uno
    // nuevo (REN) que apunta al anterior. Así se conserva el historial del cliente.
    public int? PreviousContractId { get; set; }
    public Contract? PreviousContract { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
