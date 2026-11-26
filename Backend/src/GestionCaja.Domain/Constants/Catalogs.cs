namespace GestionCaja.Domain.Constants;

public static class Roles
{
    public const int Administrador = 1;
    public const int Gestor = 2;
    public const int Cajero = 3;
}

public static class AttentionTypes
{
    public const string AtencionCliente = "AC";
    public const string ContratacionServicio = "CS";
    public const string PagoServicio = "PS";
    public const string CambioServicio = "CA";
    public const string CambioFormaPago = "FP";
    public const string CancelacionContrato = "CC";
}

public static class AttentionStatuses
{
    public const int Pendiente = 1;
    public const int EnAtencion = 2;
    public const int Atendido = 3;
    public const int Cancelado = 4;
}

public static class UserStatuses
{
    public const string Activo = "ACT";
    public const string Inactivo = "INA";
    public const string PendienteAprobacion = "PEN";
    public const string Bloqueado = "BLO";
}
