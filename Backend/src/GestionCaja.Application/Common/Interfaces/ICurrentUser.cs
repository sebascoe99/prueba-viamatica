namespace GestionCaja.Application.Common.Interfaces;

/// <summary>
/// Datos del usuario que hace la petición, leídos de los claims del token.
/// La implementación está en la Api, que es la que conoce el contexto HTTP.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    int UserId { get; }
    string UserName { get; }
    int RolId { get; }
}
