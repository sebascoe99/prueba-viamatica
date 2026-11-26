using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Service : BaseEntity
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public int SpeedMbps { get; set; }
    public decimal Price { get; set; }
    public bool Active { get; set; } = true;

    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
