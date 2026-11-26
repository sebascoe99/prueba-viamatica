using GestionCaja.Domain.Common;

namespace GestionCaja.Domain.Entities;

public class Client : BaseEntity
{
    public int ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ReferenceAddress { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}
