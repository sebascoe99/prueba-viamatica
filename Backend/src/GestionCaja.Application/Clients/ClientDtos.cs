using System.ComponentModel.DataAnnotations;

namespace GestionCaja.Application.Clients;

public static class ClientRules
{
    public const string Identification = @"^[0-9]{10,13}$";
    public const string IdentificationMessage = "La identificación debe tener entre 10 y 13 dígitos numéricos";

    // Solo números, empieza con 09 y tiene al menos 10 dígitos
    public const string Phone = @"^09[0-9]{8,11}$";
    public const string PhoneMessage = "El teléfono debe tener al menos 10 dígitos, solo números y empezar con 09";
}

public class ClientRequest
{
    [Required(ErrorMessage = "Los nombres son obligatorios")]
    [StringLength(50, ErrorMessage = "Los nombres no pueden superar 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(50, ErrorMessage = "Los apellidos no pueden superar 50 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es obligatoria")]
    [RegularExpression(ClientRules.Identification, ErrorMessage = ClientRules.IdentificationMessage)]
    public string Identification { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
    [StringLength(120, ErrorMessage = "El correo no puede superar 120 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(ClientRules.Phone, ErrorMessage = ClientRules.PhoneMessage)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria")]
    [StringLength(100, MinimumLength = 20, ErrorMessage = "La dirección debe tener entre 20 y 100 caracteres")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "La referencia de la dirección es obligatoria")]
    [StringLength(100, MinimumLength = 20, ErrorMessage = "La referencia de la dirección debe tener entre 20 y 100 caracteres")]
    public string ReferenceAddress { get; set; } = string.Empty;
}

/// <summary>Fila de la carga masiva, sin validación automática (se valida fila por fila en el servicio).</summary>
public class ClientBulkRow
{
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? Identification { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? ReferenceAddress { get; set; }

    public ClientRequest ToRequest() => new()
    {
        Name = Name ?? string.Empty,
        LastName = LastName ?? string.Empty,
        Identification = Identification ?? string.Empty,
        Email = Email ?? string.Empty,
        PhoneNumber = PhoneNumber ?? string.Empty,
        Address = Address ?? string.Empty,
        ReferenceAddress = ReferenceAddress ?? string.Empty
    };
}

public class ClientStatusRequest
{
    public bool Active { get; set; }
}

public class ClientResponse
{
    public int ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ReferenceAddress { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}
