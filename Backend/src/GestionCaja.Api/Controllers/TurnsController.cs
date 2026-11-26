using GestionCaja.Application.Turns;
using GestionCaja.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCaja.Api.Controllers;

[ApiController]
[Route("api/turns")]
[Authorize]
public class TurnsController : ControllerBase
{
    private readonly ITurnService _turnService;

    public TurnsController(ITurnService turnService)
    {
        _turnService = turnService;
    }

    /// <summary>
    /// Turnos de un día (por defecto hoy). El gestor ve los que asignó; el cajero, los de sus cajas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TurnResponse>>> List(
        [FromQuery] int? cashId,
        [FromQuery] DateOnly? date,
        [FromQuery] int? statusId,
        CancellationToken cancellationToken)
    {
        return Ok(await _turnService.ListAsync(cashId, date, statusId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TurnResponse>> Get(int id, CancellationToken cancellationToken)
    {
        return Ok(await _turnService.GetAsync(id, cancellationToken));
    }

    /// <summary>
    /// El gestor asigna un turno a una caja. La descripción (AC0001) la genera la función
    /// fn_next_turn_description de PostgreSQL según el tipo de atención.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = RoleNames.Gestor)]
    public async Task<ActionResult<TurnResponse>> Create([FromBody] TurnCreateRequest request, CancellationToken cancellationToken)
    {
        var turn = await _turnService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = turn.TurnId }, turn);
    }

    /// <summary>Reasigna un turno pendiente a otra caja.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Gestor)]
    public async Task<ActionResult<TurnResponse>> Update(int id, [FromBody] TurnUpdateRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _turnService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>Eliminación lógica de un turno pendiente.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Gestor)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _turnService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>El cajero inicia la atención del turno para un cliente.</summary>
    [HttpPost("{id:int}/attend")]
    [Authorize(Roles = RoleNames.Cajero)]
    public async Task<ActionResult<TurnResponse>> Attend(int id, [FromBody] TurnAttendRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _turnService.AttendAsync(id, request.ClientId, cancellationToken));
    }

    /// <summary>El cajero finaliza la atención: el turno queda como atendido.</summary>
    [HttpPost("{id:int}/finish")]
    [Authorize(Roles = RoleNames.Cajero)]
    public async Task<ActionResult<TurnResponse>> Finish(int id, CancellationToken cancellationToken)
    {
        return Ok(await _turnService.FinishAsync(id, cancellationToken));
    }
}
