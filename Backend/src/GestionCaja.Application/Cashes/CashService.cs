using GestionCaja.Application.Common.Exceptions;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Domain.Constants;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionCaja.Application.Cashes;

public interface ICashService
{
    Task<List<CashResponse>> ListAsync(CancellationToken cancellationToken = default);
    Task<CashResponse> GetAsync(int cashId, CancellationToken cancellationToken = default);
    Task<CashResponse> CreateAsync(CashRequest request, CancellationToken cancellationToken = default);
    Task<CashResponse> UpdateAsync(int cashId, CashRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int cashId, CancellationToken cancellationToken = default);

    Task<List<CashierResponse>> ListAvailableCashiersAsync(CancellationToken cancellationToken = default);
    Task<CashResponse> AssignCashierAsync(int cashId, int userId, CancellationToken cancellationToken = default);
    Task<CashResponse> UnassignCashierAsync(int cashId, int userId, CancellationToken cancellationToken = default);

    Task<List<CashResponse>> ListMineAsync(CancellationToken cancellationToken = default);
    Task<CashResponse> OpenAsync(int cashId, CancellationToken cancellationToken = default);
    Task<CashResponse> CloseAsync(int cashId, CancellationToken cancellationToken = default);
}

public class CashService : ICashService
{
    // Regla del enunciado: cada caja es manejada por un máximo de 2 usuarios
    private const int MaxCashiersPerCash = 2;

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CashService> _logger;

    public CashService(IApplicationDbContext db, ICurrentUser currentUser, ILogger<CashService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<CashResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var cashes = await WithRelations().AsNoTracking().OrderBy(c => c.CashDescription).ToListAsync(cancellationToken);
        return cashes.Select(ToResponse).ToList();
    }

    public async Task<CashResponse> GetAsync(int cashId, CancellationToken cancellationToken = default)
    {
        return ToResponse(await FindOrThrowAsync(cashId, cancellationToken));
    }

    public async Task<CashResponse> CreateAsync(CashRequest request, CancellationToken cancellationToken = default)
    {
        var cash = new Cash
        {
            CashDescription = request.CashDescription.Trim(),
            Active = request.Active
        };

        _db.Cashes.Add(cash);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de caja {Cash} (id {CashId}) por {CurrentUser}", cash.CashDescription, cash.CashId, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(cash.CashId, cancellationToken));
    }

    public async Task<CashResponse> UpdateAsync(int cashId, CashRequest request, CancellationToken cancellationToken = default)
    {
        var cash = await FindOrThrowAsync(cashId, cancellationToken);

        if (!request.Active && cash.CurrentUserId.HasValue)
        {
            throw new AppException("No se puede inactivar una caja que está siendo operada por un cajero");
        }

        cash.CashDescription = request.CashDescription.Trim();
        cash.Active = request.Active;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de caja {Cash} (id {CashId}) por {CurrentUser}", cash.CashDescription, cash.CashId, _currentUser.UserName);

        return ToResponse(cash);
    }

    public async Task DeleteAsync(int cashId, CancellationToken cancellationToken = default)
    {
        var cash = await FindOrThrowAsync(cashId, cancellationToken);

        if (cash.CurrentUserId.HasValue)
        {
            throw new AppException("No se puede eliminar una caja que está siendo operada por un cajero");
        }

        _db.UserCashes.RemoveRange(cash.UserCashes);
        _db.Cashes.Remove(cash);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ELIMINACIÓN lógica de caja {Cash} (id {CashId}) por {CurrentUser}", cash.CashDescription, cash.CashId, _currentUser.UserName);
    }

    public async Task<List<CashierResponse>> ListAvailableCashiersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Users.AsNoTracking()
            .Where(u => u.RolId == Roles.Cajero && u.UserStatusId == UserStatuses.Activo)
            .OrderBy(u => u.UserName)
            .Select(u => new CashierResponse
            {
                UserId = u.UserId,
                UserName = u.UserName,
                FullName = u.FirstName + " " + u.LastName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CashResponse> AssignCashierAsync(int cashId, int userId, CancellationToken cancellationToken = default)
    {
        var cash = await FindOrThrowAsync(cashId, cancellationToken);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken)
            ?? throw new AppException("El usuario no existe", 404);

        if (user.RolId != Roles.Cajero)
        {
            throw new AppException("Solo se pueden asignar usuarios con rol Cajero a una caja");
        }

        if (user.UserStatusId != UserStatuses.Activo)
        {
            throw new AppException("El cajero no está activo");
        }

        if (cash.UserCashes.Any(uc => uc.UserId == userId))
        {
            throw new AppException("El cajero ya está asignado a esta caja", 409);
        }

        if (cash.UserCashes.Count >= MaxCashiersPerCash)
        {
            throw new AppException($"La caja ya tiene el máximo de {MaxCashiersPerCash} cajeros asignados");
        }

        _db.UserCashes.Add(new UserCash
        {
            CashId = cashId,
            UserId = userId,
            AssignedBy = _currentUser.UserId
        });
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de asignación: cajero {Cashier} a la caja {Cash} por {CurrentUser}",
            user.UserName, cash.CashDescription, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(cashId, cancellationToken));
    }

    public async Task<CashResponse> UnassignCashierAsync(int cashId, int userId, CancellationToken cancellationToken = default)
    {
        var cash = await FindOrThrowAsync(cashId, cancellationToken);

        var assignment = cash.UserCashes.FirstOrDefault(uc => uc.UserId == userId)
            ?? throw new AppException("El cajero no está asignado a esta caja", 404);

        if (cash.CurrentUserId == userId)
        {
            cash.CurrentUserId = null;
            cash.OpenedAt = null;
        }

        _db.UserCashes.Remove(assignment);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ELIMINACIÓN lógica de asignación: cajero {UserId} de la caja {Cash} por {CurrentUser}",
            userId, cash.CashDescription, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(cashId, cancellationToken));
    }

    public async Task<List<CashResponse>> ListMineAsync(CancellationToken cancellationToken = default)
    {
        var cashes = await WithRelations().AsNoTracking()
            .Where(c => c.Active && c.UserCashes.Any(uc => uc.UserId == _currentUser.UserId))
            .OrderBy(c => c.CashDescription)
            .ToListAsync(cancellationToken);

        return cashes.Select(ToResponse).ToList();
    }

    public async Task<CashResponse> OpenAsync(int cashId, CancellationToken cancellationToken = default)
    {
        var cash = await FindOrThrowAsync(cashId, cancellationToken);

        if (!cash.Active)
        {
            throw new AppException("La caja está inactiva");
        }

        if (cash.UserCashes.All(uc => uc.UserId != _currentUser.UserId))
        {
            throw new AppException("No está asignado a esta caja", 403);
        }

        if (cash.CurrentUserId == _currentUser.UserId)
        {
            return ToResponse(cash);
        }

        // Regla del enunciado: los 2 cajeros de una caja no pueden operarla al mismo tiempo
        if (cash.CurrentUserId.HasValue)
        {
            throw new AppException($"La caja está siendo operada por {cash.CurrentUser!.UserName}", 409);
        }

        var hasAnotherOpen = await _db.Cashes.AnyAsync(c => c.CurrentUserId == _currentUser.UserId, cancellationToken);
        if (hasAnotherOpen)
        {
            throw new AppException("Ya tiene otra caja abierta. Debe cerrarla primero", 409);
        }

        cash.CurrentUserId = _currentUser.UserId;
        cash.OpenedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new AppException("La caja acaba de ser tomada por otro cajero", 409);
        }

        _logger.LogInformation("ACTUALIZACIÓN: apertura de caja {Cash} por {CurrentUser}", cash.CashDescription, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(cashId, cancellationToken));
    }

    public async Task<CashResponse> CloseAsync(int cashId, CancellationToken cancellationToken = default)
    {
        var cash = await FindOrThrowAsync(cashId, cancellationToken);

        if (cash.CurrentUserId != _currentUser.UserId)
        {
            throw new AppException("La caja no está abierta por usted");
        }

        var hasAttentionInProgress = await _db.Attentions.AnyAsync(a =>
            a.Turn.CashId == cashId &&
            a.UserCashierId == _currentUser.UserId &&
            a.AttentionStatusId == AttentionStatuses.EnAtencion, cancellationToken);

        if (hasAttentionInProgress)
        {
            throw new AppException("Tiene un turno en atención. Debe finalizarlo antes de cerrar la caja");
        }

        cash.CurrentUserId = null;
        cash.OpenedAt = null;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN: cierre de caja {Cash} por {CurrentUser}", cash.CashDescription, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(cashId, cancellationToken));
    }

    private IQueryable<Cash> WithRelations()
    {
        return _db.Cashes
            .Include(c => c.CurrentUser)
            .Include(c => c.UserCashes).ThenInclude(uc => uc.User);
    }

    private async Task<Cash> FindOrThrowAsync(int cashId, CancellationToken cancellationToken)
    {
        var cash = await WithRelations().FirstOrDefaultAsync(c => c.CashId == cashId, cancellationToken);
        return cash ?? throw new AppException("La caja no existe", 404);
    }

    private static CashResponse ToResponse(Cash cash) => new()
    {
        CashId = cash.CashId,
        CashDescription = cash.CashDescription,
        Active = cash.Active,
        CurrentUserId = cash.CurrentUserId,
        CurrentUserName = cash.CurrentUser?.UserName,
        OpenedAt = cash.OpenedAt,
        Cashiers = cash.UserCashes
            .Select(uc => new CashierResponse
            {
                UserId = uc.UserId,
                UserName = uc.User.UserName,
                FullName = $"{uc.User.FirstName} {uc.User.LastName}"
            })
            .OrderBy(c => c.UserName)
            .ToList()
    };
}
