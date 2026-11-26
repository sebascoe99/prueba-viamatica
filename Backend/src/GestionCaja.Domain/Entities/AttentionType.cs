using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class AttentionType : BaseEntity
{
    public string AttentionTypeId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
