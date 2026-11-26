using System.Text.RegularExpressions;
using GestionCaja.Application.Common;
using GestionCaja.Application.Common.Exceptions;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Domain.Constants;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionCaja.Application.Turns;

public interface ITurnService
{
    Task<List<TurnResponse>> ListAsync(int? cashId, DateOnly? date, int? statusId, CancellationToken cancellationToken = default);
    Task<TurnResponse> GetAsync(int turnId, CancellationToken cancellationToken = default);
    Task<TurnResponse> CreateAsync(TurnCreateRequest request, CancellationToken cancellationToken = default);
    Task<TurnResponse> UpdateAsync(int turnId, TurnUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int turnId, CancellationToken cancellationToken = default);
    Task<TurnResponse> AttendAsync(int turnId, int clientId, CancellationToken cancellationToken = default);
    Task<TurnResponse> FinishAsync(int turnId, CancellationToken cancellationToken = default);
}

public partial class TurnService : ITurnService
{
    private readonly IApplicationDbContext _db;
    private readonly ITurnDescriptionGenerator _descriptionGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<TurnService> _logger;

    public TurnService(
        IApplicationDbContext db,
        ITurnDescriptionGenerator descriptionGenerator,
        ICurrentUser currentUser,
        ILogger<TurnService> logger)
    {
        _db = db;
        _descriptionGenerator = descriptionGenerator;
        _currentUser = currentUser;
        _logger = logger;
    }

    // Regla del enunciado: 2 letras mayúsculas + 4 números
    [GeneratedRegex("^[A-Z]{2}[0-9]{4}$")]
    private static partial Regex DescriptionFormat();

    public async Task<List<TurnResponse>> ListAsync(int? cashId, DateOnly? date, int? statusId, CancellationToken cancellationToken = default)
    {
        var (fromUtc, toUtc) = EcuadorTime.RangeUtc(date ?? EcuadorTime.Today, date ?? EcuadorTime.Today);

        var query = WithRelations().AsNoTracking().Where(t => t.Date >= fromUtc && t.Date < toUtc);

        if (cashId.HasValue)
        {
            query = query.Where(t => t.CashId == cashId.Value);
        }

        // El gestor solo ve los turnos que él asignó
        if (_currentUser.RolId == Roles.Gestor)
        {
            query = query.Where(t => t.UserGestorId == _currentUser.UserId);
        }

        // El cajero solo ve los turnos de las cajas a las que está asignado
        if (_currentUser.RolId == Roles.Cajero)
        {
            query = query.Where(t => t.Cash.UserCashes.Any(uc => uc.UserId == _currentUser.UserId));
        }

        var turns = await query.OrderBy(t => t.Date).ToListAsync(cancellationToken);
        var responses = turns.Select(ToResponse);

        if (statusId.HasValue)
        {
            responses = responses.Where(t => t.StatusId == statusId.Value);
        }

        return responses.ToList();
    }

    public async Task<TurnResponse> GetAsync(int turnId, CancellationToken cancellationToken = default)
    {
        return ToResponse(await FindOrThrowAsync(turnId, cancellationToken));
    }

    public async Task<TurnResponse> CreateAsync(TurnCreateRequest request, CancellationToken cancellationToken = default)
    {
        var attentionTypeId = request.AttentionTypeId.Trim().ToUpperInvariant();

        var typeExists = await _db.AttentionTypes.AnyAsync(a => a.AttentionTypeId == attentionTypeId, cancellationToken);
        if (!typeExists)
        {
            throw new AppException("El tipo de atención no existe");
        }

        await EnsureCashCanReceiveTurnsAsync(request.CashId, cancellationToken);

        // Transacción: la función toma un bloqueo por tipo de atención que se mantiene hasta el commit,
        // así dos gestores no pueden obtener el mismo número de turno al mismo tiempo.
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var description = await _descriptionGenerator.NextAsync(attentionTypeId, cancellationToken);
        if (!DescriptionFormat().IsMatch(description))
        {
            throw new AppException("La descripción del turno debe tener 2 letras mayúsculas y 4 números");
        }

        var turn = new Turn
        {
            Description = description,
            Date = DateTime.UtcNow,
            CashId = request.CashId,
            UserGestorId = _currentUser.UserId,
            AttentionTypeId = attentionTypeId
        };

        _db.Turns.Add(turn);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de turno {Turn} (id {TurnId}) en la caja {CashId} por {CurrentUser}",
            turn.Description, turn.TurnId, turn.CashId, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(turn.TurnId, cancellationToken));
    }

    public async Task<TurnResponse> UpdateAsync(int turnId, TurnUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var turn = await FindOrThrowAsync(turnId, cancellationToken);
        EnsurePending(turn, "reasignar");
        await EnsureCashCanReceiveTurnsAsync(request.CashId, cancellationToken);

        turn.CashId = request.CashId;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de turno {Turn} (id {TurnId}): reasignado a la caja {CashId} por {CurrentUser}",
            turn.Description, turn.TurnId, turn.CashId, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(turnId, cancellationToken));
    }

    public async Task DeleteAsync(int turnId, CancellationToken cancellationToken = default)
    {
        var turn = await FindOrThrowAsync(turnId, cancellationToken);
        EnsurePending(turn, "eliminar");

        _db.Turns.Remove(turn);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ELIMINACIÓN lógica de turno {Turn} (id {TurnId}) por {CurrentUser}",
            turn.Description, turn.TurnId, _currentUser.UserName);
    }

    public async Task<TurnResponse> AttendAsync(int turnId, int clientId, CancellationToken cancellationToken = default)
    {
        var turn = await FindOrThrowAsync(turnId, cancellationToken);
        EnsurePending(turn, "atender");

        // Solo el cajero que tiene la caja abierta puede atender sus turnos
        if (turn.Cash.CurrentUserId != _currentUser.UserId)
        {
            throw new AppException("Debe abrir la caja del turno antes de atenderlo", 403);
        }

        var hasAnotherInProgress = await _db.Attentions.AnyAsync(a =>
            a.UserCashierId == _currentUser.UserId &&
            a.AttentionStatusId == AttentionStatuses.EnAtencion, cancellationToken);

        if (hasAnotherInProgress)
        {
            throw new AppException("Ya tiene un turno en atención. Debe finalizarlo primero", 409);
        }

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken)
            ?? throw new AppException("El cliente no existe", 404);

        if (!client.Active)
        {
            throw new AppException("El cliente está inactivo");
        }

        _db.Attentions.Add(new Attention
        {
            TurnId = turn.TurnId,
            ClientId = clientId,
            AttentionTypeId = turn.AttentionTypeId,
            AttentionStatusId = AttentionStatuses.EnAtencion,
            UserCashierId = _currentUser.UserId
        });
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de atención: turno {Turn} para el cliente {Identification} por {CurrentUser}",
            turn.Description, client.Identification, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(turnId, cancellationToken));
    }

    public async Task<TurnResponse> FinishAsync(int turnId, CancellationToken cancellationToken = default)
    {
        var turn = await FindOrThrowAsync(turnId, cancellationToken);
        var attention = CurrentAttention(turn);

        if (attention is null || attention.AttentionStatusId != AttentionStatuses.EnAtencion)
        {
            throw new AppException("El turno no está en atención");
        }

        if (attention.UserCashierId != _currentUser.UserId)
        {
            throw new AppException("El turno está siendo atendido por otro cajero", 403);
        }

        attention.AttentionStatusId = AttentionStatuses.Atendido;
        attention.AttendedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de atención: turno {Turn} finalizado por {CurrentUser}", turn.Description, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(turnId, cancellationToken));
    }

    private IQueryable<Turn> WithRelations()
    {
        return _db.Turns
            .Include(t => t.Cash)
            .Include(t => t.AttentionType)
            .Include(t => t.UserGestor)
            .Include(t => t.Attentions).ThenInclude(a => a.Client)
            .Include(t => t.Attentions).ThenInclude(a => a.AttentionStatus)
            .Include(t => t.Attentions).ThenInclude(a => a.UserCashier);
    }

    private async Task<Turn> FindOrThrowAsync(int turnId, CancellationToken cancellationToken)
    {
        var turn = await WithRelations().FirstOrDefaultAsync(t => t.TurnId == turnId, cancellationToken);
        return turn ?? throw new AppException("El turno no existe", 404);
    }

    private async Task EnsureCashCanReceiveTurnsAsync(int cashId, CancellationToken cancellationToken)
    {
        var cash = await _db.Cashes.AsNoTracking().FirstOrDefaultAsync(c => c.CashId == cashId, cancellationToken)
            ?? throw new AppException("La caja no existe", 404);

        if (!cash.Active)
        {
            throw new AppException("La caja está inactiva");
        }
    }

    private static Attention? CurrentAttention(Turn turn)
    {
        return turn.Attentions.OrderByDescending(a => a.AttentionId).FirstOrDefault();
    }

    private static void EnsurePending(Turn turn, string action)
    {
        if (CurrentAttention(turn) is not null)
        {
            throw new AppException($"No se puede {action} un turno que ya fue atendido o está en atención");
        }
    }

    private static TurnResponse ToResponse(Turn turn)
    {
        var attention = CurrentAttention(turn);

        return new TurnResponse
        {
            TurnId = turn.TurnId,
            Description = turn.Description,
            Date = turn.Date,
            CashId = turn.CashId,
            CashDescription = turn.Cash.CashDescription,
            AttentionTypeId = turn.AttentionTypeId,
            AttentionTypeDescription = turn.AttentionType.Description,
            UserGestorId = turn.UserGestorId,
            GestorUserName = turn.UserGestor.UserName,
            StatusId = attention?.AttentionStatusId ?? AttentionStatuses.Pendiente,
            StatusDescription = attention?.AttentionStatus.Description ?? "Pendiente",
            AttentionId = attention?.AttentionId,
            ClientId = attention?.ClientId,
            ClientName = attention is null ? null : $"{attention.Client.Name} {attention.Client.LastName}",
            ClientIdentification = attention?.Client.Identification,
            CashierUserName = attention?.UserCashier?.UserName,
            AttendedAt = attention?.AttendedAt
        };
    }
}
