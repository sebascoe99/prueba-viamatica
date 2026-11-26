using GestionCaja.Application.Catalogs;
using GestionCaja.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCaja.Api.Controllers;

/// <summary>Mantenimiento de los planes de internet. Solo administrador.</summary>
[ApiController]
[Route("api/services")]
[Authorize(Roles = RoleNames.Administrador)]
public class ServicesController : ControllerBase
{
    private readonly ICatalogAdminService _service;

    public ServicesController(ICatalogAdminService service)
    {
        _service = service;
    }

    /// <summary>Lista todos los servicios, incluidos los inactivos.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ServiceResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await _service.ListServicesAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResponse>> Get(int id, CancellationToken cancellationToken) =>
        Ok(await _service.GetServiceAsync(id, cancellationToken));

    /// <summary>Crea un servicio con sus equipos.</summary>
    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create([FromBody] ServiceRequest request, CancellationToken cancellationToken)
    {
        var created = await _service.CreateServiceAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.ServiceId }, created);
    }

    /// <summary>Actualiza el servicio y sincroniza su lista de equipos.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceResponse>> Update(int id, [FromBody] ServiceRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateServiceAsync(id, request, cancellationToken));

    /// <summary>Eliminación lógica. No se permite si el servicio tiene contratos.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteServiceAsync(id, cancellationToken);
        return NoContent();
    }
}

/// <summary>Mantenimiento de los equipos de cada servicio. Solo administrador.</summary>
[ApiController]
[Route("api/devices")]
[Authorize(Roles = RoleNames.Administrador)]
public class DevicesController : ControllerBase
{
    private readonly ICatalogAdminService _service;

    public DevicesController(ICatalogAdminService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<DeviceResponse>>> List([FromQuery] int? serviceId, CancellationToken cancellationToken) =>
        Ok(await _service.ListDevicesAsync(serviceId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<DeviceResponse>> Create([FromBody] DeviceRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateDeviceAsync(request, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DeviceResponse>> Update(int id, [FromBody] DeviceRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateDeviceAsync(id, request, cancellationToken));

    /// <summary>Eliminación lógica.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteDeviceAsync(id, cancellationToken);
        return NoContent();
    }
}

/// <summary>Mantenimiento de formas de pago. La consulta está en /api/catalogs/method-payments.</summary>
[ApiController]
[Route("api/method-payments")]
[Authorize(Roles = RoleNames.Administrador)]
public class MethodPaymentsController : ControllerBase
{
    private readonly ICatalogAdminService _service;

    public MethodPaymentsController(ICatalogAdminService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<CatalogItem>> Create([FromBody] MethodPaymentRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateMethodPaymentAsync(request, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CatalogItem>> Update(int id, [FromBody] MethodPaymentRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateMethodPaymentAsync(id, request, cancellationToken));

    /// <summary>Eliminación lógica. No se permite si está usada en contratos o pagos.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteMethodPaymentAsync(id, cancellationToken);
        return NoContent();
    }
}

/// <summary>Mantenimiento de tipos de atención. La consulta está en /api/catalogs/attention-types.</summary>
[ApiController]
[Route("api/attention-types")]
[Authorize(Roles = RoleNames.Administrador)]
public class AttentionTypesController : ControllerBase
{
    private readonly ICatalogAdminService _service;

    public AttentionTypesController(ICatalogAdminService service)
    {
        _service = service;
    }

    /// <summary>El código son las 2 letras con las que iniciará la descripción del turno.</summary>
    [HttpPost]
    public async Task<ActionResult<CatalogItem>> Create([FromBody] AttentionTypeCreateRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateAttentionTypeAsync(request, cancellationToken));

    [HttpPut("{id}")]
    public async Task<ActionResult<CatalogItem>> Update(string id, [FromBody] AttentionTypeUpdateRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAttentionTypeAsync(id, request, cancellationToken));

    /// <summary>Eliminación lógica. No se permite si tiene turnos.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _service.DeleteAttentionTypeAsync(id, cancellationToken);
        return NoContent();
    }
}
