namespace GestionCaja.Domain.Constants;

public static class Roles
{
    public const int Administrador = 1;
    public const int Gestor = 2;
    public const int Cajero = 3;
}

public static class RoleNames
{
    public const string Administrador = "Administrador";
    public const string Gestor = "Gestor";
    public const string Cajero = "Cajero";
    public const string AdministradorOGestor = Administrador + "," + Gestor;
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

public static class ContractStatuses
{
    public const string Vigente = "VIG";
    public const string Sustituido = "SUS";
    public const string RenovacionServicio = "REN";
    public const string Cancelado = "CAN";
}

public static class UserStatuses
{
    public const string Activo = "ACT";
    public const string Inactivo = "INA";
    public const string PendienteAprobacion = "PEN";
    public const string Bloqueado = "BLO";
}
