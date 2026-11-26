namespace GestionCaja.Domain.Constants;

public static class Roles
{
    public const int Administrador = 1;
    public const int Gestor = 2;
    public const int Cajero = 3;
}

public static class UserStatuses
{
    public const string Activo = "ACT";
    public const string Inactivo = "INA";
    public const string PendienteAprobacion = "PEN";
    public const string Bloqueado = "BLO";
}
