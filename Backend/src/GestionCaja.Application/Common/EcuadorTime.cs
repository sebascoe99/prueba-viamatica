namespace GestionCaja.Application.Common;

/// <summary>
/// Las fechas se guardan en UTC. Para saber qué es "hoy" para el negocio se usa la hora
/// de Ecuador (UTC-5, sin horario de verano) y se convierte el día a un rango en UTC.
/// </summary>
public static class EcuadorTime
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(-5);

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow + Offset);

    /// <summary>Rango [desde, hasta) en UTC que cubre los días indicados, ambos incluidos.</summary>
    public static (DateTime FromUtc, DateTime ToUtc) RangeUtc(DateOnly from, DateOnly to)
    {
        var fromUtc = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue) - Offset, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(to.AddDays(1).ToDateTime(TimeOnly.MinValue) - Offset, DateTimeKind.Utc);
        return (fromUtc, toUtc);
    }

    public static (DateTime FromUtc, DateTime ToUtc) TodayRangeUtc() => RangeUtc(Today, Today);
}
