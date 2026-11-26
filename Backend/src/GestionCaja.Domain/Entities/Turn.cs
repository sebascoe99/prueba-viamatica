using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Turn : BaseEntity
{
    public int TurnId { get; set; }

    // 2 letras del tipo de atención + 4 números secuenciales. Ej: AC0001
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int CashId { get; set; }
    public Cash Cash { get; set; } = null!;

    // Gestor que asignó el turno a la caja
    public int UserGestorId { get; set; }
    public User UserGestor { get; set; } = null!;

    public string AttentionTypeId { get; set; } = string.Empty;
    public AttentionType AttentionType { get; set; } = null!;

    public ICollection<Attention> Attentions { get; set; } = new List<Attention>();
}
