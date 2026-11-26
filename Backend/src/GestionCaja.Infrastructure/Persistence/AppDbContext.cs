using GestionCaja.Domain.Common;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCaja.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UserStatus> UserStatuses => Set<UserStatus>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<RolMenu> RolMenus => Set<RolMenu>();

    public DbSet<Cash> Cashes => Set<Cash>();
    public DbSet<UserCash> UserCashes => Set<UserCash>();
    public DbSet<Turn> Turns => Set<Turn>();
    public DbSet<AttentionType> AttentionTypes => Set<AttentionType>();
    public DbSet<AttentionStatus> AttentionStatuses => Set<AttentionStatus>();
    public DbSet<Attention> Attentions => Set<Attention>();
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas las clases IEntityTypeConfiguration de este proyecto (carpeta Configurations)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                // Eliminación lógica: un Remove() no borra la fila, solo la marca como eliminada
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
