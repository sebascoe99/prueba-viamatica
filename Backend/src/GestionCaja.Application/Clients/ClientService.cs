using GestionCaja.Application.Common;
using GestionCaja.Application.Common.Exceptions;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Application.Common.Validation;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionCaja.Application.Clients;

public interface IClientService
{
    Task<List<ClientResponse>> ListAsync(string? search, bool? active, CancellationToken cancellationToken = default);
    Task<ClientResponse> GetAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ClientResponse> GetByIdentificationAsync(string identification, CancellationToken cancellationToken = default);
    Task<ClientResponse> CreateAsync(ClientRequest request, CancellationToken cancellationToken = default);
    Task<BulkResult> BulkCreateAsync(List<ClientRequest> rows, CancellationToken cancellationToken = default);
    Task<ClientResponse> UpdateAsync(int clientId, ClientRequest request, CancellationToken cancellationToken = default);
    Task<ClientResponse> ChangeStatusAsync(int clientId, bool active, CancellationToken cancellationToken = default);
    Task DeleteAsync(int clientId, CancellationToken cancellationToken = default);
}

public class ClientService : IClientService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ClientService> _logger;

    public ClientService(IApplicationDbContext db, ICurrentUser currentUser, ILogger<ClientService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<ClientResponse>> ListAsync(string? search, bool? active, CancellationToken cancellationToken = default)
    {
        var query = _db.Clients.AsNoTracking();

        if (active.HasValue)
        {
            query = query.Where(c => c.Active == active.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.Identification.Contains(term) ||
                c.Name.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term));
        }

        var clients = await query.OrderBy(c => c.LastName).ThenBy(c => c.Name).ToListAsync(cancellationToken);
        return clients.Select(ToResponse).ToList();
    }

    public async Task<ClientResponse> GetAsync(int clientId, CancellationToken cancellationToken = default)
    {
        return ToResponse(await FindOrThrowAsync(clientId, cancellationToken));
    }

    public async Task<ClientResponse> GetByIdentificationAsync(string identification, CancellationToken cancellationToken = default)
    {
        var client = await _db.Clients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Identification == identification.Trim(), cancellationToken);

        if (client is null)
        {
            throw new AppException("No existe un cliente con esa identificación", 404);
        }

        return ToResponse(client);
    }

    public async Task<ClientResponse> CreateAsync(ClientRequest request, CancellationToken cancellationToken = default)
    {
        var identification = request.Identification.Trim();
        await EnsureIdentificationIsFreeAsync(identification, null, cancellationToken);

        var client = new Client();
        Map(request, client);

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de cliente {Identification} (id {ClientId}) por {CurrentUser}",
            client.Identification, client.ClientId, _currentUser.UserName);

        return ToResponse(client);
    }

    public async Task<BulkResult> BulkCreateAsync(List<ClientRequest> rows, CancellationToken cancellationToken = default)
    {
        var result = new BulkResult { Total = rows.Count };

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowResult = new BulkRowResult { Row = i + 1, Key = row.Identification };

            try
            {
                DtoValidator.ValidateOrThrow(row);
                await CreateAsync(row, cancellationToken);

                rowResult.Success = true;
                rowResult.Message = "Cliente creado";
                result.Created++;
            }
            catch (AppException ex)
            {
                rowResult.Message = ex.Message;
                result.Failed++;
            }

            result.Rows.Add(rowResult);
        }

        _logger.LogInformation("CARGA MASIVA de clientes por {CurrentUser}: {Created} creados, {Failed} rechazados de {Total}",
            _currentUser.UserName, result.Created, result.Failed, result.Total);

        return result;
    }

    public async Task<ClientResponse> UpdateAsync(int clientId, ClientRequest request, CancellationToken cancellationToken = default)
    {
        var client = await FindOrThrowAsync(clientId, cancellationToken);
        await EnsureIdentificationIsFreeAsync(request.Identification.Trim(), clientId, cancellationToken);

        Map(request, client);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de cliente {Identification} (id {ClientId}) por {CurrentUser}",
            client.Identification, client.ClientId, _currentUser.UserName);

        return ToResponse(client);
    }

    public async Task<ClientResponse> ChangeStatusAsync(int clientId, bool active, CancellationToken cancellationToken = default)
    {
        var client = await FindOrThrowAsync(clientId, cancellationToken);
        client.Active = active;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de estado del cliente {Identification} (id {ClientId}): activo = {Active} por {CurrentUser}",
            client.Identification, client.ClientId, active, _currentUser.UserName);

        return ToResponse(client);
    }

    public async Task DeleteAsync(int clientId, CancellationToken cancellationToken = default)
    {
        var client = await FindOrThrowAsync(clientId, cancellationToken);

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ELIMINACIÓN lógica del cliente {Identification} (id {ClientId}) por {CurrentUser}",
            client.Identification, client.ClientId, _currentUser.UserName);
    }

    private async Task<Client> FindOrThrowAsync(int clientId, CancellationToken cancellationToken)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);
        return client ?? throw new AppException("El cliente no existe", 404);
    }

    private async Task EnsureIdentificationIsFreeAsync(string identification, int? exceptClientId, CancellationToken cancellationToken)
    {
        var exists = await _db.Clients
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Identification == identification && c.ClientId != exceptClientId, cancellationToken);

        if (exists)
        {
            throw new AppException($"Ya existe un cliente registrado con la identificación {identification}", 409);
        }
    }

    private static void Map(ClientRequest request, Client client)
    {
        client.Name = request.Name.Trim();
        client.LastName = request.LastName.Trim();
        client.Identification = request.Identification.Trim();
        client.Email = request.Email.Trim().ToLowerInvariant();
        client.PhoneNumber = request.PhoneNumber.Trim();
        client.Address = request.Address.Trim();
        client.ReferenceAddress = request.ReferenceAddress.Trim();
    }

    private static ClientResponse ToResponse(Client client) => new()
    {
        ClientId = client.ClientId,
        Name = client.Name,
        LastName = client.LastName,
        Identification = client.Identification,
        Email = client.Email,
        PhoneNumber = client.PhoneNumber,
        Address = client.Address,
        ReferenceAddress = client.ReferenceAddress,
        Active = client.Active,
        CreatedAt = client.CreatedAt
    };
}
