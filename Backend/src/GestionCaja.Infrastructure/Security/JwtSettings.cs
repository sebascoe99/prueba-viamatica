namespace GestionCaja.Infrastructure.Security;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

/// <summary>
/// Nombres de los claims dentro del token. Sus valores viajan cifrados.
/// </summary>
public static class JwtClaimNames
{
    public const string UserId = "uid";
    public const string UserName = "usr";
    public const string RolId = "rid";
    public const string RolName = "rol";
}
