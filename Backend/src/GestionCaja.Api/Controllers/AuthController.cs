using GestionCaja.Application.Auth;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCaja.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ICurrentUser _currentUser;

    public AuthController(IAuthService authService, IUserService userService, ICurrentUser currentUser)
    {
        _authService = authService;
        _userService = userService;
        _currentUser = currentUser;
    }

    /// <summary>Valida las credenciales y devuelve el token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _authService.LoginAsync(request, cancellationToken));
    }

    /// <summary>Cambia la contraseña validando usuario, identificación y correo.</summary>
    [HttpPost("recover-password")]
    [AllowAnonymous]
    public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordRequest request, CancellationToken cancellationToken)
    {
        await _userService.RecoverPasswordAsync(request, cancellationToken);
        return Ok(new { message = "La contraseña se actualizó correctamente" });
    }

    /// <summary>Marca la sesión del usuario como inactiva.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(_currentUser.UserId, cancellationToken);
        return NoContent();
    }

    /// <summary>Devuelve los datos del usuario autenticado, leídos de los claims ya descifrados.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = _currentUser.UserId,
            userName = _currentUser.UserName,
            rolId = _currentUser.RolId
        });
    }
}
