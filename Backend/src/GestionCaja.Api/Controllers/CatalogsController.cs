using GestionCaja.Application.Catalogs;
using GestionCaja.Application.Dashboard;
using GestionCaja.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCaja.Api.Controllers;

/// <summary>Catálogos de solo lectura para llenar las listas del frontend.</summary>
[ApiController]
[Route("api/catalogs")]
[Authorize]
public class CatalogsController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("roles")]
    public async Task<ActionResult<List<CatalogItem>>> Roles(CancellationToken cancellationToken) =>
        Ok(await _catalogService.RolesAsync(cancellationToken));

    [HttpGet("user-statuses")]
    public async Task<ActionResult<List<CatalogItem>>> UserStatuses(CancellationToken cancellationToken) =>
        Ok(await _catalogService.UserStatusesAsync(cancellationToken));

    [HttpGet("attention-types")]
    public async Task<ActionResult<List<CatalogItem>>> AttentionTypes(CancellationToken cancellationToken) =>
        Ok(await _catalogService.AttentionTypesAsync(cancellationToken));

    [HttpGet("contract-statuses")]
    public async Task<ActionResult<List<CatalogItem>>> ContractStatuses(CancellationToken cancellationToken) =>
        Ok(await _catalogService.ContractStatusesAsync(cancellationToken));

    [HttpGet("method-payments")]
    public async Task<ActionResult<List<CatalogItem>>> MethodPayments(CancellationToken cancellationToken) =>
        Ok(await _catalogService.MethodPaymentsAsync(cancellationToken));

    /// <summary>Planes de internet con sus equipos.</summary>
    [HttpGet("services")]
    public async Task<ActionResult<List<ServiceResponse>>> Services(CancellationToken cancellationToken) =>
        Ok(await _catalogService.ServicesAsync(cancellationToken));
}

[ApiController]
[Route("api/menu")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public MenuController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>Opciones de menú del rol del usuario autenticado, cargadas desde la base de datos.</summary>
    [HttpGet]
    public async Task<ActionResult<List<MenuResponse>>> Get(CancellationToken cancellationToken) =>
        Ok(await _catalogService.MenuAsync(cancellationToken));
}

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Pantalla de bienvenida: datos del usuario, turnos atendidos hoy según su rol
    /// y usuarios pendientes de aprobación.
    /// </summary>
    [HttpGet("welcome")]
    public async Task<ActionResult<WelcomeResponse>> Welcome(CancellationToken cancellationToken) =>
        Ok(await _dashboardService.WelcomeAsync(cancellationToken));

    /// <summary>Indicadores de usuarios, cajas, cajeros y gestores en un rango de fechas. Solo administrador.</summary>
    [HttpGet("indicators")]
    [Authorize(Roles = RoleNames.Administrador)]
    public async Task<ActionResult<IndicatorsResponse>> Indicators([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken) =>
        Ok(await _dashboardService.IndicatorsAsync(from, to, cancellationToken));
}
