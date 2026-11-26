namespace GestionCaja.Application.Common;

public static class EcuadorTime
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(-5);

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow + Offset);

    public static (DateTime FromUtc, DateTime ToUtc) RangeUtc(DateOnly from, DateOnly to)
    {
        var fromUtc = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue) - Offset, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(to.AddDays(1).ToDateTime(TimeOnly.MinValue) - Offset, DateTimeKind.Utc);
        return (fromUtc, toUtc);
    }

    public static (DateTime FromUtc, DateTime ToUtc) TodayRangeUtc() => RangeUtc(Today, Today);
}
