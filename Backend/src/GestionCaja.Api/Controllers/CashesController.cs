using GestionCaja.Application.Cashes;
using GestionCaja.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCaja.Api.Controllers;

[ApiController]
[Route("api/cashes")]
[Authorize]
public class CashesController : ControllerBase
{
    private readonly ICashService _cashService;

    public CashesController(ICashService cashService)
    {
        _cashService = cashService;
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.AdministradorOGestor)]
    public async Task<ActionResult<List<CashResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _cashService.ListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = RoleNames.AdministradorOGestor)]
    public async Task<ActionResult<CashResponse>> Get(int id, CancellationToken cancellationToken)
    {
        return Ok(await _cashService.GetAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.AdministradorOGestor)]
    public async Task<ActionResult<CashResponse>> Create([FromBody] CashRequest request, CancellationToken cancellationToken)
    {
        var cash = await _cashService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = cash.CashId }, cash);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.AdministradorOGestor)]
    public async Task<ActionResult<CashResponse>> Update(int id, [FromBody] CashRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _cashService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>Eliminación lógica.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.AdministradorOGestor)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _cashService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Cajeros activos que se pueden asignar a una caja.</summary>
    [HttpGet("available-cashiers")]
    [Authorize(Roles = RoleNames.AdministradorOGestor)]
    public async Task<ActionResult<List<CashierResponse>>> AvailableCashiers(CancellationToken cancellationToken)
    {
        return Ok(await _cashService.ListAvailableCashiersAsync(cancellationToken));
    }

    /// <summary>Asigna un cajero a la caja (máximo 2 por caja).</summary>
    [HttpPost("{id:int}/cashiers")]
    [Authorize(Roles = RoleNames.Gestor)]
    public async Task<ActionResult<CashResponse>> AssignCashier(int id, [FromBody] AssignCashierRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _cashService.AssignCashierAsync(id, request.UserId, cancellationToken));
    }

    [HttpDelete("{id:int}/cashiers/{userId:int}")]
    [Authorize(Roles = RoleNames.Gestor)]
    public async Task<ActionResult<CashResponse>> UnassignCashier(int id, int userId, CancellationToken cancellationToken)
    {
        return Ok(await _cashService.UnassignCashierAsync(id, userId, cancellationToken));
    }

    /// <summary>Cajas a las que está asignado el cajero autenticado.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = RoleNames.Cajero)]
    public async Task<ActionResult<List<CashResponse>>> Mine(CancellationToken cancellationToken)
    {
        return Ok(await _cashService.ListMineAsync(cancellationToken));
    }

    /// <summary>El cajero toma la caja. Falla si el otro cajero asignado la está operando.</summary>
    [HttpPost("{id:int}/open")]
    [Authorize(Roles = RoleNames.Cajero)]
    public async Task<ActionResult<CashResponse>> Open(int id, CancellationToken cancellationToken)
    {
        return Ok(await _cashService.OpenAsync(id, cancellationToken));
    }

    /// <summary>El cajero libera la caja.</summary>
    [HttpPost("{id:int}/close")]
    [Authorize(Roles = RoleNames.Cajero)]
    public async Task<ActionResult<CashResponse>> Close(int id, CancellationToken cancellationToken)
    {
        return Ok(await _cashService.CloseAsync(id, cancellationToken));
    }
}
