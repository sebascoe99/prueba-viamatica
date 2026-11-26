using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

/// <summary>
/// Atención que un cajero le da a un cliente dentro de un turno.
/// </summary>
public class Attention : BaseEntity
{
    public int AttentionId { get; set; }

    public int TurnId { get; set; }
    public Turn Turn { get; set; } = null!;

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public string AttentionTypeId { get; set; } = string.Empty;
    public AttentionType AttentionType { get; set; } = null!;

    public int AttentionStatusId { get; set; }
    public AttentionStatus AttentionStatus { get; set; } = null!;

    // Cajero que atendió y momento en que finalizó (para los indicadores)
    public int? UserCashierId { get; set; }
    public User? UserCashier { get; set; }
    public DateTime? AttendedAt { get; set; }
}
