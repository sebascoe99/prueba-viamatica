using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

/// <summary>
/// Plan de internet que la empresa ofrece.
/// </summary>
public class Service : BaseEntity
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public int SpeedMbps { get; set; }
    public decimal Price { get; set; }
    public bool Active { get; set; } = true;

    // Equipos que incluye el plan (router, ONT, etc.)
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
