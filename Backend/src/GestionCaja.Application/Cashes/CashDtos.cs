using System.ComponentModel.DataAnnotations;

namespace GestionCaja.Application.Cashes;

public class CashRequest
{
    [Required(ErrorMessage = "La descripción de la caja es obligatoria")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar 50 caracteres")]
    public string CashDescription { get; set; } = string.Empty;

    public bool Active { get; set; } = true;
}

public class AssignCashierRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "El cajero es obligatorio")]
    public int UserId { get; set; }
}

public class CashierResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class CashResponse
{
    public int CashId { get; set; }
    public string CashDescription { get; set; } = string.Empty;
    public bool Active { get; set; }

    // Cajero que la está operando en este momento (null = libre)
    public int? CurrentUserId { get; set; }
    public string? CurrentUserName { get; set; }
    public DateTime? OpenedAt { get; set; }

    public List<CashierResponse> Cashiers { get; set; } = new();
}
