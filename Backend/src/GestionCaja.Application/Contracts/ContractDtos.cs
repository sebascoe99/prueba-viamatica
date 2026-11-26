using System.ComponentModel.DataAnnotations;

namespace GestionCaja.Application.Contracts;

public class ContractCreateRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "El cliente es obligatorio")]
    public int ClientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El servicio es obligatorio")]
    public int ServiceId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La forma de pago es obligatoria")]
    public int MethodPaymentId { get; set; }

    [Range(1, 60, ErrorMessage = "La duración debe estar entre 1 y 60 meses")]
    public int Months { get; set; } = 12;
}

public class ChangeServiceRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "El servicio es obligatorio")]
    public int ServiceId { get; set; }
}

public class ChangePaymentMethodRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "La forma de pago es obligatoria")]
    public int MethodPaymentId { get; set; }
}

public class PaymentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "El contrato es obligatorio")]
    public int ContractId { get; set; }

    // Si no se envía, se cobra el precio del servicio contratado
    [Range(0.01, 100000, ErrorMessage = "El monto debe ser mayor a cero")]
    public decimal? Amount { get; set; }

    // Si no se envía, se usa la forma de pago del contrato
    public int? MethodPaymentId { get; set; }
}

public class ContractResponse
{
    public int ContractId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientIdentification { get; set; } = string.Empty;

    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int SpeedMbps { get; set; }
    public decimal Price { get; set; }

    public string StatusId { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;

    public int MethodPaymentId { get; set; }
    public string MethodPaymentDescription { get; set; } = string.Empty;

    public int? PreviousContractId { get; set; }
}

public class PaymentResponse
{
    public int PaymentId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public int ContractId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int MethodPaymentId { get; set; }
    public string MethodPaymentDescription { get; set; } = string.Empty;
    public string CashierUserName { get; set; } = string.Empty;
}
