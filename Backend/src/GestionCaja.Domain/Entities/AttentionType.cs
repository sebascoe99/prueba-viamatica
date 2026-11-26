namespace GestionCaja.Domain.Entities;

/// <summary>
/// Catálogo de tipos de atención. La clave son las 2 letras con las que empieza
/// la descripción del turno (AC0001 = Atención al cliente, PS0001 = Pago de servicio).
/// </summary>
public class AttentionType
{
    public string AttentionTypeId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
