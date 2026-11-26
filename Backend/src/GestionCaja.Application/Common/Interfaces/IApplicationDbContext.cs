using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GestionCaja.Application.Common.Interfaces;

/// <summary>
/// Abstracción del acceso a datos para los servicios. Application conoce esta interfaz;
/// la implementación real (AppDbContext con PostgreSQL) está en Infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Rol> Roles { get; }
    DbSet<UserStatus> UserStatuses { get; }
    DbSet<User> Users { get; }
    DbSet<Menu> Menus { get; }
    DbSet<RolMenu> RolMenus { get; }
    DbSet<Cash> Cashes { get; }
    DbSet<UserCash> UserCashes { get; }
    DbSet<Turn> Turns { get; }
    DbSet<AttentionType> AttentionTypes { get; }
    DbSet<AttentionStatus> AttentionStatuses { get; }
    DbSet<Attention> Attentions { get; }
    DbSet<Client> Clients { get; }
    DbSet<Service> Services { get; }
    DbSet<Device> Devices { get; }
    DbSet<StatusContract> StatusContracts { get; }
    DbSet<MethodPayment> MethodPayments { get; }
    DbSet<Contract> Contracts { get; }
    DbSet<Payment> Payments { get; }

    /// <summary>Permite abrir transacciones cuando una operación tiene varios pasos.</summary>
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Genera la descripción del turno (AC0001) usando la función de PostgreSQL fn_next_turn_description.
/// </summary>
public interface ITurnDescriptionGenerator
{
    Task<string> NextAsync(string attentionTypeId, CancellationToken cancellationToken = default);
}
