using GestionCaja.Application.Common;
using GestionCaja.Application.Users;
using GestionCaja.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCaja.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = RoleNames.AdministradorOGestor)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Lista usuarios con filtros opcionales. El gestor solo ve los que él creó.</summary>
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> List(
        [FromQuery] int? rolId,
        [FromQuery] string? statusId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        return Ok(await _userService.ListAsync(rolId, statusId, search, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> Get(int id, CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetAsync(id, cancellationToken));
    }

    /// <summary>
    /// Crea un usuario gestor o cajero. Si lo crea un administrador queda activo;
    /// si lo crea un gestor queda pendiente de aprobación.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = user.UserId }, user);
    }

    /// <summary>Carga masiva. Devuelve el resultado fila por fila.</summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<BulkResult>> BulkCreate([FromBody] List<UserBulkRow> rows, CancellationToken cancellationToken)
    {
        var requests = rows.Select(row => row.ToRequest()).ToList();
        return Ok(await _userService.BulkCreateAsync(requests, cancellationToken));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Administrador)]
    public async Task<ActionResult<UserResponse>> Update(int id, [FromBody] UserUpdateRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>Cambia el estado del usuario: ACT, INA o BLO.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = RoleNames.Administrador)]
    public async Task<ActionResult<UserResponse>> ChangeStatus(int id, [FromBody] UserStatusRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userService.ChangeStatusAsync(id, request.StatusId, cancellationToken));
    }

    /// <summary>Aprueba un usuario creado por un gestor.</summary>
    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = RoleNames.Administrador)]
    public async Task<ActionResult<UserResponse>> Approve(int id, CancellationToken cancellationToken)
    {
        return Ok(await _userService.ApproveAsync(id, cancellationToken));
    }

    /// <summary>Eliminación lógica.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Administrador)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
