using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Turn : BaseEntity
{
    public int TurnId { get; set; }

    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int CashId { get; set; }
    public Cash Cash { get; set; } = null!;

    public int UserGestorId { get; set; }
    public User UserGestor { get; set; } = null!;

    public string AttentionTypeId { get; set; } = string.Empty;
    public AttentionType AttentionType { get; set; } = null!;

    public ICollection<Attention> Attentions { get; set; } = new List<Attention>();
}
