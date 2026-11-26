namespace GestionCaja.Domain.Entities;

/// <summary>
/// Catálogo de estados del contrato (VIG, SUS, REN, CAN).
/// </summary>
public class StatusContract
{
    public string StatusId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
