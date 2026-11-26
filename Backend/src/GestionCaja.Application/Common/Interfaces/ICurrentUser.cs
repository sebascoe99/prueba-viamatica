namespace GestionCaja.Application.Common.Interfaces;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    int UserId { get; }
    string UserName { get; }
    int RolId { get; }
}
