using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Cash : BaseEntity
{
    public int CashId { get; set; }
    public string CashDescription { get; set; } = string.Empty;
    public bool Active { get; set; } = true;

    public int? CurrentUserId { get; set; }
    public User? CurrentUser { get; set; }
    public DateTime? OpenedAt { get; set; }

    public uint Version { get; set; }

    public ICollection<UserCash> UserCashes { get; set; } = new List<UserCash>();
    public ICollection<Turn> Turns { get; set; } = new List<Turn>();
}
