using System.ComponentModel.DataAnnotations;
using GestionCaja.Application.Common.Exceptions;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionCaja.Application.Catalogs;

public class ServiceRequest
{
    [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
    public string ServiceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del servicio es obligatoria")]
    [StringLength(150, ErrorMessage = "La descripción no puede superar 150 caracteres")]
    public string ServiceDescription { get; set; } = string.Empty;

    [Range(1, 100000, ErrorMessage = "La velocidad debe ser mayor a cero")]
    public int SpeedMbps { get; set; }

    [Range(0, 100000, ErrorMessage = "El precio no puede ser negativo")]
    public decimal Price { get; set; }

    public bool Active { get; set; } = true;

    public List<string> Devices { get; set; } = new();
}

public class DeviceRequest
{
    [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar 50 caracteres")]
    public string DeviceName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El servicio es obligatorio")]
    public int ServiceId { get; set; }
}

public class DeviceResponse
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
}

public class MethodPaymentRequest
{
    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar 50 caracteres")]
    public string Description { get; set; } = string.Empty;
}

public class AttentionTypeCreateRequest
{
    [Required(ErrorMessage = "El código es obligatorio")]
    [RegularExpression("^[A-Z]{2}$", ErrorMessage = "El código debe tener 2 letras mayúsculas")]
    public string AttentionTypeId { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(100, ErrorMessage = "La descripción no puede superar 100 caracteres")]
    public string Description { get; set; } = string.Empty;
}

public class AttentionTypeUpdateRequest
{
    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(100, ErrorMessage = "La descripción no puede superar 100 caracteres")]
    public string Description { get; set; } = string.Empty;
}

public interface ICatalogAdminService
{
    Task<List<ServiceResponse>> ListServicesAsync(CancellationToken cancellationToken = default);
    Task<ServiceResponse> GetServiceAsync(int serviceId, CancellationToken cancellationToken = default);
    Task<ServiceResponse> CreateServiceAsync(ServiceRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateServiceAsync(int serviceId, ServiceRequest request, CancellationToken cancellationToken = default);
    Task DeleteServiceAsync(int serviceId, CancellationToken cancellationToken = default);

    Task<List<DeviceResponse>> ListDevicesAsync(int? serviceId, CancellationToken cancellationToken = default);
    Task<DeviceResponse> CreateDeviceAsync(DeviceRequest request, CancellationToken cancellationToken = default);
    Task<DeviceResponse> UpdateDeviceAsync(int deviceId, DeviceRequest request, CancellationToken cancellationToken = default);
    Task DeleteDeviceAsync(int deviceId, CancellationToken cancellationToken = default);

    Task<CatalogItem> CreateMethodPaymentAsync(MethodPaymentRequest request, CancellationToken cancellationToken = default);
    Task<CatalogItem> UpdateMethodPaymentAsync(int methodPaymentId, MethodPaymentRequest request, CancellationToken cancellationToken = default);
    Task DeleteMethodPaymentAsync(int methodPaymentId, CancellationToken cancellationToken = default);

    Task<CatalogItem> CreateAttentionTypeAsync(AttentionTypeCreateRequest request, CancellationToken cancellationToken = default);
    Task<CatalogItem> UpdateAttentionTypeAsync(string attentionTypeId, AttentionTypeUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAttentionTypeAsync(string attentionTypeId, CancellationToken cancellationToken = default);
}

public class CatalogAdminService : ICatalogAdminService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CatalogAdminService> _logger;

    public CatalogAdminService(IApplicationDbContext db, ICurrentUser currentUser, ILogger<CatalogAdminService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<ServiceResponse>> ListServicesAsync(CancellationToken cancellationToken = default)
    {
        var services = await _db.Services.AsNoTracking()
            .Include(s => s.Devices)
            .OrderBy(s => s.SpeedMbps)
            .ToListAsync(cancellationToken);

        return services.Select(ToResponse).ToList();
    }

    public async Task<ServiceResponse> GetServiceAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        return ToResponse(await FindServiceAsync(serviceId, cancellationToken));
    }

    public async Task<ServiceResponse> CreateServiceAsync(ServiceRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureServiceNameIsFreeAsync(request.ServiceName.Trim(), null, cancellationToken);

        var service = new Service
        {
            ServiceName = request.ServiceName.Trim(),
            ServiceDescription = request.ServiceDescription.Trim(),
            SpeedMbps = request.SpeedMbps,
            Price = request.Price,
            Active = request.Active,
            Devices = CleanNames(request.Devices).Select(name => new Device { DeviceName = name }).ToList()
        };

        _db.Services.Add(service);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de servicio {Service} (id {ServiceId}) por {CurrentUser}", service.ServiceName, service.ServiceId, _currentUser.UserName);

        return ToResponse(service);
    }

    public async Task<ServiceResponse> UpdateServiceAsync(int serviceId, ServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = await FindServiceAsync(serviceId, cancellationToken);
        await EnsureServiceNameIsFreeAsync(request.ServiceName.Trim(), serviceId, cancellationToken);

        service.ServiceName = request.ServiceName.Trim();
        service.ServiceDescription = request.ServiceDescription.Trim();
        service.SpeedMbps = request.SpeedMbps;
        service.Price = request.Price;
        service.Active = request.Active;

        // Se sincronizan los equipos: se retiran los que ya no vienen y se agregan los nuevos
        var names = CleanNames(request.Devices);
        var removed = service.Devices.Where(d => !names.Contains(d.DeviceName, StringComparer.OrdinalIgnoreCase)).ToList();
        _db.Devices.RemoveRange(removed);

        var current = service.Devices.Except(removed).Select(d => d.DeviceName).ToList();
        foreach (var name in names.Where(n => !current.Contains(n, StringComparer.OrdinalIgnoreCase)))
        {
            _db.Devices.Add(new Device { DeviceName = name, ServiceId = serviceId });
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de servicio {Service} (id {ServiceId}) por {CurrentUser}", service.ServiceName, service.ServiceId, _currentUser.UserName);

        return await GetServiceAsync(serviceId, cancellationToken);
    }

    public async Task DeleteServiceAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        var service = await FindServiceAsync(serviceId, cancellationToken);

        var inUse = await _db.Contracts.AnyAsync(c => c.ServiceId == serviceId, cancellationToken);
        if (inUse)
        {
            throw new AppException("No se puede eliminar un servicio que tiene contratos. Puede inactivarlo", 409);
        }

        _db.Devices.RemoveRange(service.Devices);
        _db.Services.Remove(service);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ELIMINACIÓN lógica del servicio {Service} (id {ServiceId}) por {CurrentUser}", service.ServiceName, service.ServiceId, _currentUser.UserName);
    }

    public async Task<List<DeviceResponse>> ListDevicesAsync(int? serviceId, CancellationToken cancellationToken = default)
    {
        var query = _db.Devices.AsNoTracking();

        if (serviceId.HasValue)
        {
            query = query.Where(d => d.ServiceId == serviceId.Value);
        }

        return await query
            .OrderBy(d => d.ServiceId).ThenBy(d => d.DeviceName)
            .Select(d => new DeviceResponse
            {
                DeviceId = d.DeviceId,
                DeviceName = d.DeviceName,
                ServiceId = d.ServiceId,
                ServiceName = d.Service.ServiceName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DeviceResponse> CreateDeviceAsync(DeviceRequest request, CancellationToken cancellationToken = default)
    {
        await FindServiceAsync(request.ServiceId, cancellationToken);

        var device = new Device { DeviceName = request.DeviceName.Trim(), ServiceId = request.ServiceId };
        _db.Devices.Add(device);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de equipo {Device} (id {DeviceId}) por {CurrentUser}", device.DeviceName, device.DeviceId, _currentUser.UserName);

        return await GetDeviceAsync(device.DeviceId, cancellationToken);
    }

    public async Task<DeviceResponse> UpdateDeviceAsync(int deviceId, DeviceRequest request, CancellationToken cancellationToken = default)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken)
            ?? throw new AppException("El equipo no existe", 404);

        await FindServiceAsync(request.ServiceId, cancellationToken);

        device.DeviceName = request.DeviceName.Trim();
        device.ServiceId = request.ServiceId;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de equipo {Device} (id {DeviceId}) por {CurrentUser}", device.DeviceName, device.DeviceId, _currentUser.UserName);

        return await GetDeviceAsync(deviceId, cancellationToken);
    }

    public async Task DeleteDeviceAsync(int deviceId, CancellationToken cancellationToken = default)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken)
            ?? throw new AppException("El equipo no existe", 404);

        _db.Devices.Remove(device);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ELIMINACIÓN lógica del equipo {Device} (id {DeviceId}) por {CurrentUser}", device.DeviceName, device.DeviceId, _currentUser.UserName);
    }

    public async Task<CatalogItem> CreateMethodPaymentAsync(MethodPaymentRequest request, CancellationToken cancellationToken = default)
    {
        // El id no es autogenerado: se toma el siguiente, contando también los eliminados
        var lastId = await _db.MethodPayments.IgnoreQueryFilters().MaxAsync(m => (int?)m.MethodPaymentId, cancellationToken) ?? 0;

        var method = new MethodPayment { MethodPaymentId = lastId + 1, Description = request.Description.Trim() };
        _db.MethodPayments.Add(method);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de forma de pago {Method} (id {MethodId}) por {CurrentUser}", method.Description, method.MethodPaymentId, _currentUser.UserName);

        return new CatalogItem(method.MethodPaymentId.ToString(), method.Description);
    }

    public async Task<CatalogItem> UpdateMethodPaymentAsync(int methodPaymentId, MethodPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var method = await _db.MethodPayments.FirstOrDefaultAsync(m => m.MethodPaymentId == methodPaymentId, cancellationToken)
            ?? throw new AppException("La forma de pago no existe", 404);

        method.Description = request.Description.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de forma de pago {Method} (id {MethodId}) por {CurrentUser}", method.Description, method.MethodPaymentId, _currentUser.UserName);

        return new CatalogItem(method.MethodPaymentId.ToString(), method.Description);
    }

    public async Task DeleteMethodPaymentAsync(int methodPaymentId, CancellationToken cancellationToken = default)
    {
        var method = await _db.MethodPayments.FirstOrDefaultAsync(m => m.MethodPaymentId == methodPaymentId, cancellationToken)
            ?? throw new AppException("La forma de pago no existe", 404);

        var inUse = await _db.Contracts.AnyAsync(c => c.MethodPaymentId == methodPaymentId, cancellationToken)
            || await _db.Payments.AnyAsync(p => p.MethodPaymentId == methodPaymentId, cancellationToken);

        if (inUse)
        {
            throw new AppException("No se puede eliminar una forma de pago usada en contratos o pagos", 409);
        }

        _db.MethodPayments.Remove(method);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ELIMINACIÓN lógica de forma de pago {Method} (id {MethodId}) por {CurrentUser}", method.Description, method.MethodPaymentId, _currentUser.UserName);
    }

    public async Task<CatalogItem> CreateAttentionTypeAsync(AttentionTypeCreateRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _db.AttentionTypes.IgnoreQueryFilters().AnyAsync(a => a.AttentionTypeId == request.AttentionTypeId, cancellationToken);
        if (exists)
        {
            throw new AppException($"El código {request.AttentionTypeId} ya está registrado", 409);
        }

        var type = new AttentionType { AttentionTypeId = request.AttentionTypeId, Description = request.Description.Trim() };
        _db.AttentionTypes.Add(type);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de tipo de atención {Type} por {CurrentUser}", type.AttentionTypeId, _currentUser.UserName);

        return new CatalogItem(type.AttentionTypeId, type.Description);
    }

    public async Task<CatalogItem> UpdateAttentionTypeAsync(string attentionTypeId, AttentionTypeUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var type = await _db.AttentionTypes.FirstOrDefaultAsync(a => a.AttentionTypeId == attentionTypeId, cancellationToken)
            ?? throw new AppException("El tipo de atención no existe", 404);

        type.Description = request.Description.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de tipo de atención {Type} por {CurrentUser}", type.AttentionTypeId, _currentUser.UserName);

        return new CatalogItem(type.AttentionTypeId, type.Description);
    }

    public async Task DeleteAttentionTypeAsync(string attentionTypeId, CancellationToken cancellationToken = default)
    {
        var type = await _db.AttentionTypes.FirstOrDefaultAsync(a => a.AttentionTypeId == attentionTypeId, cancellationToken)
            ?? throw new AppException("El tipo de atención no existe", 404);

        var inUse = await _db.Turns.AnyAsync(t => t.AttentionTypeId == attentionTypeId, cancellationToken);
        if (inUse)
        {
            throw new AppException("No se puede eliminar un tipo de atención que tiene turnos", 409);
        }

        _db.AttentionTypes.Remove(type);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ELIMINACIÓN lógica del tipo de atención {Type} por {CurrentUser}", type.AttentionTypeId, _currentUser.UserName);
    }

    private async Task<Service> FindServiceAsync(int serviceId, CancellationToken cancellationToken)
    {
        var service = await _db.Services.Include(s => s.Devices).FirstOrDefaultAsync(s => s.ServiceId == serviceId, cancellationToken);
        return service ?? throw new AppException("El servicio no existe", 404);
    }

    private async Task<DeviceResponse> GetDeviceAsync(int deviceId, CancellationToken cancellationToken)
    {
        var devices = await ListDevicesAsync(null, cancellationToken);
        return devices.First(d => d.DeviceId == deviceId);
    }

    private async Task EnsureServiceNameIsFreeAsync(string name, int? exceptServiceId, CancellationToken cancellationToken)
    {
        var exists = await _db.Services.IgnoreQueryFilters()
            .AnyAsync(s => s.ServiceName.ToLower() == name.ToLower() && s.ServiceId != exceptServiceId, cancellationToken);

        if (exists)
        {
            throw new AppException($"Ya existe un servicio con el nombre {name}", 409);
        }
    }

    private static List<string> CleanNames(IEnumerable<string>? names)
    {
        return (names ?? [])
            .Select(name => name.Trim())
            .Where(name => name.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static ServiceResponse ToResponse(Service service) => new()
    {
        ServiceId = service.ServiceId,
        ServiceName = service.ServiceName,
        ServiceDescription = service.ServiceDescription,
        SpeedMbps = service.SpeedMbps,
        Price = service.Price,
        Active = service.Active,
        // Se excluyen los equipos recién retirados, que siguen en memoria marcados como eliminados
        Devices = service.Devices.Where(d => !d.IsDeleted).Select(d => d.DeviceName).OrderBy(name => name).ToList()
    };
}
