namespace GestionCaja.Application.Common;

/// <summary>Resultado de una carga masiva: resumen y detalle fila por fila.</summary>
public class BulkResult
{
    public int Total { get; set; }
    public int Created { get; set; }
    public int Failed { get; set; }
    public List<BulkRowResult> Rows { get; set; } = new();
}

public class BulkRowResult
{
    public int Row { get; set; }
    public string Key { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
