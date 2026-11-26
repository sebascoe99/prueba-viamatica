using GestionCaja.Application.Common;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace GestionCaja.Application.Dashboard;

public class PendingUserResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RolName { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
}

public class WelcomeResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RolName { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }

    public int TurnsAttendedToday { get; set; }
    public string TurnsAttendedScope { get; set; } = string.Empty;

    public List<PendingUserResponse> PendingUsers { get; set; } = new();
}

public class UserIndicators
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Blocked { get; set; }
    public int PendingApproval { get; set; }
    public int SessionActive { get; set; }
    public int SessionInactive { get; set; }
}

public class CashIndicator
{
    public int CashId { get; set; }
    public string CashDescription { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string? OperatedBy { get; set; }
    public int TurnsAssigned { get; set; }
    public int TurnsAttended { get; set; }
    public int TurnsPending { get; set; }
}

public class CashierIndicator
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TurnsAttended { get; set; }
    public int PaymentsCount { get; set; }
    public decimal PaymentsTotal { get; set; }
}

public class GestorIndicator
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TurnsAssigned { get; set; }
    public int UsersCreated { get; set; }
}

public class IndicatorsResponse
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public UserIndicators Users { get; set; } = new();
    public List<CashIndicator> Cashes { get; set; } = new();
    public List<CashierIndicator> Cashiers { get; set; } = new();
    public List<GestorIndicator> Gestores { get; set; } = new();
}

public interface IDashboardService
{
    Task<WelcomeResponse> WelcomeAsync(CancellationToken cancellationToken = default);
    Task<IndicatorsResponse> IndicatorsAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IEncryptionService _encryptionService;

    public DashboardService(IApplicationDbContext db, ICurrentUser currentUser, IEncryptionService encryptionService)
    {
        _db = db;
        _currentUser = currentUser;
        _encryptionService = encryptionService;
    }

    public async Task<WelcomeResponse> WelcomeAsync(CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking()
            .Include(u => u.Rol)
            .FirstAsync(u => u.UserId == _currentUser.UserId, cancellationToken);

        var (fromUtc, toUtc) = EcuadorTime.TodayRangeUtc();

        var attended = _db.Attentions.AsNoTracking().Where(a =>
            a.AttentionStatusId == AttentionStatuses.Atendido &&
            a.AttendedAt >= fromUtc && a.AttendedAt < toUtc);

        string scope;
        switch (_currentUser.RolId)
        {
            case Roles.Gestor:
                attended = attended.Where(a => a.Turn.UserGestorId == _currentUser.UserId);
                scope = "Turnos atendidos hoy que usted asignó";
                break;
            case Roles.Cajero:
                attended = attended.Where(a => a.UserCashierId == _currentUser.UserId);
                scope = "Turnos atendidos hoy por usted";
                break;
            default:
                scope = "Total de turnos atendidos hoy";
                break;
        }

        var response = new WelcomeResponse
        {
            UserId = user.UserId,
            UserName = user.UserName,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = _encryptionService.Decrypt(user.Email),
            RolName = user.Rol.RolName,
            LastLogin = user.LastLogin,
            TurnsAttendedToday = await attended.CountAsync(cancellationToken),
            TurnsAttendedScope = scope
        };

        if (_currentUser.RolId is Roles.Gestor or Roles.Administrador)
        {
            var pending = _db.Users.AsNoTracking().Where(u => u.UserStatusId == UserStatuses.PendienteAprobacion);

            if (_currentUser.RolId == Roles.Gestor)
            {
                pending = pending.Where(u => u.UserCreate == _currentUser.UserId);
            }

            response.PendingUsers = await pending
                .OrderBy(u => u.CreationDate)
                .Select(u => new PendingUserResponse
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    FullName = u.FirstName + " " + u.LastName,
                    RolName = u.Rol.RolName,
                    CreationDate = u.CreationDate
                })
                .ToListAsync(cancellationToken);
        }

        return response;
    }

    public async Task<IndicatorsResponse> IndicatorsAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var fromDate = from ?? EcuadorTime.Today;
        var toDate = to ?? fromDate;
        if (toDate < fromDate)
        {
            (fromDate, toDate) = (toDate, fromDate);
        }

        var (fromUtc, toUtc) = EcuadorTime.RangeUtc(fromDate, toDate);

        var users = await _db.Users.AsNoTracking()
            .Select(u => new { u.UserId, u.UserName, u.FirstName, u.LastName, u.RolId, u.UserStatusId, u.SessionActive, u.UserCreate, u.CreationDate })
            .ToListAsync(cancellationToken);

        var turns = await _db.Turns.AsNoTracking()
            .Where(t => t.Date >= fromUtc && t.Date < toUtc)
            .Select(t => new
            {
                t.CashId,
                t.UserGestorId,
                Attended = t.Attentions.Any(a => a.AttentionStatusId == AttentionStatuses.Atendido)
            })
            .ToListAsync(cancellationToken);

        var attentions = await _db.Attentions.AsNoTracking()
            .Where(a => a.AttentionStatusId == AttentionStatuses.Atendido && a.AttendedAt >= fromUtc && a.AttendedAt < toUtc)
            .Select(a => new { a.UserCashierId })
            .ToListAsync(cancellationToken);

        var payments = await _db.Payments.AsNoTracking()
            .Where(p => p.PaymentDate >= fromUtc && p.PaymentDate < toUtc)
            .Select(p => new { p.UserCashierId, p.Amount })
            .ToListAsync(cancellationToken);

        var cashes = await _db.Cashes.AsNoTracking()
            .OrderBy(c => c.CashDescription)
            .Select(c => new { c.CashId, c.CashDescription, c.Active, OperatedBy = c.CurrentUser != null ? c.CurrentUser.UserName : null })
            .ToListAsync(cancellationToken);

        return new IndicatorsResponse
        {
            From = fromDate,
            To = toDate,
            Users = new UserIndicators
            {
                Total = users.Count,
                Active = users.Count(u => u.UserStatusId == UserStatuses.Activo),
                Inactive = users.Count(u => u.UserStatusId == UserStatuses.Inactivo),
                Blocked = users.Count(u => u.UserStatusId == UserStatuses.Bloqueado),
                PendingApproval = users.Count(u => u.UserStatusId == UserStatuses.PendienteAprobacion),
                SessionActive = users.Count(u => u.SessionActive),
                SessionInactive = users.Count(u => !u.SessionActive)
            },
            Cashes = cashes.Select(c =>
            {
                var cashTurns = turns.Where(t => t.CashId == c.CashId).ToList();
                return new CashIndicator
                {
                    CashId = c.CashId,
                    CashDescription = c.CashDescription,
                    Active = c.Active,
                    OperatedBy = c.OperatedBy,
                    TurnsAssigned = cashTurns.Count,
                    TurnsAttended = cashTurns.Count(t => t.Attended),
                    TurnsPending = cashTurns.Count(t => !t.Attended)
                };
            }).ToList(),
            Cashiers = users.Where(u => u.RolId == Roles.Cajero).OrderBy(u => u.UserName).Select(u =>
            {
                var userPayments = payments.Where(p => p.UserCashierId == u.UserId).ToList();
                return new CashierIndicator
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    FullName = $"{u.FirstName} {u.LastName}",
                    TurnsAttended = attentions.Count(a => a.UserCashierId == u.UserId),
                    PaymentsCount = userPayments.Count,
                    PaymentsTotal = userPayments.Sum(p => p.Amount)
                };
            }).ToList(),
            Gestores = users.Where(u => u.RolId == Roles.Gestor).OrderBy(u => u.UserName).Select(u => new GestorIndicator
            {
                UserId = u.UserId,
                UserName = u.UserName,
                FullName = $"{u.FirstName} {u.LastName}",
                TurnsAssigned = turns.Count(t => t.UserGestorId == u.UserId),
                UsersCreated = users.Count(x => x.UserCreate == u.UserId && x.CreationDate >= fromUtc && x.CreationDate < toUtc)
            }).ToList()
        };
    }
}
