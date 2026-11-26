using System.Security.Claims;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Infrastructure.Security;

namespace GestionCaja.Api.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public int UserId => int.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public string UserName => Principal?.FindFirstValue(ClaimTypes.Name) ?? "anónimo";

    public int RolId => int.TryParse(Principal?.FindFirstValue(JwtClaimNames.RolId), out var id) ? id : 0;
}
