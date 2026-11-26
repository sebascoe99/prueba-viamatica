using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Device : BaseEntity
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
}
