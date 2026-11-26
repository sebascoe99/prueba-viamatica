using GestionCaja.Application.Clients;
using GestionCaja.Application.Common;
using GestionCaja.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCaja.Api.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize(Roles = RoleNames.Cajero)]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClientResponse>>> List([FromQuery] string? search, [FromQuery] bool? active, CancellationToken cancellationToken)
    {
        return Ok(await _clientService.ListAsync(search, active, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientResponse>> Get(int id, CancellationToken cancellationToken)
    {
        return Ok(await _clientService.GetAsync(id, cancellationToken));
    }

    /// <summary>Busca un cliente ya registrado por su número de identificación.</summary>
    [HttpGet("by-identification/{identification}")]
    public async Task<ActionResult<ClientResponse>> GetByIdentification(string identification, CancellationToken cancellationToken)
    {
        return Ok(await _clientService.GetByIdentificationAsync(identification, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ClientResponse>> Create([FromBody] ClientRequest request, CancellationToken cancellationToken)
    {
        var client = await _clientService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = client.ClientId }, client);
    }

    /// <summary>Carga masiva. Devuelve el resultado fila por fila.</summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<BulkResult>> BulkCreate([FromBody] List<ClientBulkRow> rows, CancellationToken cancellationToken)
    {
        var requests = rows.Select(row => row.ToRequest()).ToList();
        return Ok(await _clientService.BulkCreateAsync(requests, cancellationToken));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClientResponse>> Update(int id, [FromBody] ClientRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _clientService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ClientResponse>> ChangeStatus(int id, [FromBody] ClientStatusRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _clientService.ChangeStatusAsync(id, request.Active, cancellationToken));
    }

    /// <summary>Eliminación lógica.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _clientService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
