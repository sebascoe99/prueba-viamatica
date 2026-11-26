using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GestionCaja.Application.Common.Interfaces;

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

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ITurnDescriptionGenerator
{
    Task<string> NextAsync(string attentionTypeId, CancellationToken cancellationToken = default);
}
