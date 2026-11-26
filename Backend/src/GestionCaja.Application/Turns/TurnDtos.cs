using System.ComponentModel.DataAnnotations;

namespace GestionCaja.Application.Turns;

public class TurnCreateRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "La caja es obligatoria")]
    public int CashId { get; set; }

    [Required(ErrorMessage = "El tipo de atención es obligatorio")]
    public string AttentionTypeId { get; set; } = string.Empty;
}

public class TurnUpdateRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "La caja es obligatoria")]
    public int CashId { get; set; }
}

public class TurnAttendRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "El cliente es obligatorio")]
    public int ClientId { get; set; }
}

public class TurnResponse
{
    public int TurnId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    public int CashId { get; set; }
    public string CashDescription { get; set; } = string.Empty;

    public string AttentionTypeId { get; set; } = string.Empty;
    public string AttentionTypeDescription { get; set; } = string.Empty;

    public int UserGestorId { get; set; }
    public string GestorUserName { get; set; } = string.Empty;

    // Estado del turno, tomado de su atención. Sin atención = Pendiente.
    public int StatusId { get; set; }
    public string StatusDescription { get; set; } = string.Empty;

    public int? AttentionId { get; set; }
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientIdentification { get; set; }
    public string? CashierUserName { get; set; }
    public DateTime? AttendedAt { get; set; }
}
