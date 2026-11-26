using GestionCaja.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionCaja.Application.Catalogs;

public record CatalogItem(string Id, string Description);

public class ServiceResponse
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public int SpeedMbps { get; set; }
    public decimal Price { get; set; }
    public List<string> Devices { get; set; } = new();
}

public class MenuResponse
{
    public int MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public interface ICatalogService
{
    Task<List<CatalogItem>> RolesAsync(CancellationToken cancellationToken = default);
    Task<List<CatalogItem>> UserStatusesAsync(CancellationToken cancellationToken = default);
    Task<List<CatalogItem>> AttentionTypesAsync(CancellationToken cancellationToken = default);
    Task<List<CatalogItem>> ContractStatusesAsync(CancellationToken cancellationToken = default);
    Task<List<CatalogItem>> MethodPaymentsAsync(CancellationToken cancellationToken = default);
    Task<List<ServiceResponse>> ServicesAsync(CancellationToken cancellationToken = default);
    Task<List<MenuResponse>> MenuAsync(CancellationToken cancellationToken = default);
}

public class CatalogService : ICatalogService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CatalogService(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<CatalogItem>> RolesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.Roles.AsNoTracking().OrderBy(r => r.RolId).ToListAsync(cancellationToken);
        return items.Select(r => new CatalogItem(r.RolId.ToString(), r.RolName)).ToList();
    }

    public async Task<List<CatalogItem>> UserStatusesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.UserStatuses.AsNoTracking().OrderBy(s => s.Description).ToListAsync(cancellationToken);
        return items.Select(s => new CatalogItem(s.StatusId, s.Description)).ToList();
    }

    public async Task<List<CatalogItem>> AttentionTypesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.AttentionTypes.AsNoTracking().OrderBy(a => a.Description).ToListAsync(cancellationToken);
        return items.Select(a => new CatalogItem(a.AttentionTypeId, a.Description)).ToList();
    }

    public async Task<List<CatalogItem>> ContractStatusesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.StatusContracts.AsNoTracking().OrderBy(s => s.Description).ToListAsync(cancellationToken);
        return items.Select(s => new CatalogItem(s.StatusId, s.Description)).ToList();
    }

    public async Task<List<CatalogItem>> MethodPaymentsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.MethodPayments.AsNoTracking().OrderBy(m => m.MethodPaymentId).ToListAsync(cancellationToken);
        return items.Select(m => new CatalogItem(m.MethodPaymentId.ToString(), m.Description)).ToList();
    }

    public async Task<List<ServiceResponse>> ServicesAsync(CancellationToken cancellationToken = default)
    {
        var services = await _db.Services.AsNoTracking()
            .Include(s => s.Devices)
            .Where(s => s.Active)
            .OrderBy(s => s.SpeedMbps)
            .ToListAsync(cancellationToken);

        return services.Select(s => new ServiceResponse
        {
            ServiceId = s.ServiceId,
            ServiceName = s.ServiceName,
            ServiceDescription = s.ServiceDescription,
            SpeedMbps = s.SpeedMbps,
            Price = s.Price,
            Devices = s.Devices.Select(d => d.DeviceName).ToList()
        }).ToList();
    }

    public async Task<List<MenuResponse>> MenuAsync(CancellationToken cancellationToken = default)
    {
        return await _db.RolMenus.AsNoTracking()
            .Where(rm => rm.RolId == _currentUser.RolId)
            .OrderBy(rm => rm.Menu.DisplayOrder)
            .Select(rm => new MenuResponse
            {
                MenuId = rm.Menu.MenuId,
                Name = rm.Menu.Name,
                Route = rm.Menu.Route,
                Icon = rm.Menu.Icon
            })
            .ToListAsync(cancellationToken);
    }
}
