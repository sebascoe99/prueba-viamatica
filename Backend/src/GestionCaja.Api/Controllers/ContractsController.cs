using GestionCaja.Application.Contracts;
using GestionCaja.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCaja.Api.Controllers;

/// <summary>Procesos de caja sobre contratos. Solo para el cajero.</summary>
[ApiController]
[Route("api/contracts")]
[Authorize(Roles = RoleNames.Cajero)]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ContractResponse>>> List([FromQuery] int? clientId, [FromQuery] string? statusId, CancellationToken cancellationToken)
    {
        return Ok(await _contractService.ListAsync(clientId, statusId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ContractResponse>> Get(int id, CancellationToken cancellationToken)
    {
        return Ok(await _contractService.GetAsync(id, cancellationToken));
    }

    /// <summary>Contratar un servicio de internet.</summary>
    [HttpPost]
    public async Task<ActionResult<ContractResponse>> Create([FromBody] ContractCreateRequest request, CancellationToken cancellationToken)
    {
        var contract = await _contractService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = contract.ContractId }, contract);
    }

    /// <summary>
    /// Cambio de servicio: el contrato actual queda sustituido (SUS) y se crea uno nuevo
    /// en renovación de servicio (REN) con la misma fecha final.
    /// </summary>
    [HttpPost("{id:int}/change-service")]
    public async Task<ActionResult<ContractResponse>> ChangeService(int id, [FromBody] ChangeServiceRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _contractService.ChangeServiceAsync(id, request.ServiceId, cancellationToken));
    }

    /// <summary>Cambio de forma de pago.</summary>
    [HttpPost("{id:int}/change-payment-method")]
    public async Task<ActionResult<ContractResponse>> ChangePaymentMethod(int id, [FromBody] ChangePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _contractService.ChangePaymentMethodAsync(id, request.MethodPaymentId, cancellationToken));
    }

    /// <summary>Cancelación del contrato: actualiza el estado (CAN) y la fecha fin.</summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ContractResponse>> Cancel(int id, CancellationToken cancellationToken)
    {
        return Ok(await _contractService.CancelAsync(id, cancellationToken));
    }

    /// <summary>Eliminación lógica.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _contractService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/payments")]
[Authorize(Roles = RoleNames.Cajero)]
public class PaymentsController : ControllerBase
{
    private readonly IContractService _contractService;

    public PaymentsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PaymentResponse>>> List([FromQuery] int? clientId, [FromQuery] int? contractId, CancellationToken cancellationToken)
    {
        return Ok(await _contractService.ListPaymentsAsync(clientId, contractId, cancellationToken));
    }

    /// <summary>Registra el pago de un contrato en curso.</summary>
    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Pay([FromBody] PaymentRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _contractService.PayAsync(request, cancellationToken));
    }
}
