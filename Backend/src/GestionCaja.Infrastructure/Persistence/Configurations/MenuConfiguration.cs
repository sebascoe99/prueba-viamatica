using GestionCaja.Domain.Constants;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("menu");
        builder.HasKey(m => m.MenuId);

        builder.Property(m => m.Name).HasMaxLength(50).IsRequired();
        builder.Property(m => m.Route).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Icon).HasMaxLength(50);

        builder.HasQueryFilter(m => !m.IsDeleted);

        // Opciones del menú del frontend. La ruta coincide con las rutas de Angular.
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new Menu { MenuId = 1, Name = "Inicio", Route = "/home", Icon = "home", DisplayOrder = 1, CreatedAt = seedDate },
            new Menu { MenuId = 2, Name = "Dashboard", Route = "/dashboard", Icon = "dashboard", DisplayOrder = 2, CreatedAt = seedDate },
            new Menu { MenuId = 3, Name = "Usuarios", Route = "/users", Icon = "group", DisplayOrder = 3, CreatedAt = seedDate },
            new Menu { MenuId = 4, Name = "Cajas", Route = "/cashes", Icon = "point_of_sale", DisplayOrder = 4, CreatedAt = seedDate },
            new Menu { MenuId = 5, Name = "Asignación de turnos", Route = "/turns", Icon = "confirmation_number", DisplayOrder = 5, CreatedAt = seedDate },
            new Menu { MenuId = 6, Name = "Procesos de caja", Route = "/cash-process", Icon = "payments", DisplayOrder = 6, CreatedAt = seedDate },
            new Menu { MenuId = 7, Name = "Clientes", Route = "/clients", Icon = "badge", DisplayOrder = 7, CreatedAt = seedDate });
    }
}

public class RolMenuConfiguration : IEntityTypeConfiguration<RolMenu>
{
    public void Configure(EntityTypeBuilder<RolMenu> builder)
    {
        builder.ToTable("rolmenu");
        // Clave primaria compuesta: un rol no puede tener dos veces la misma opción
        builder.HasKey(rm => new { rm.RolId, rm.MenuId });

        builder.HasOne(rm => rm.Rol)
            .WithMany(r => r.RolMenus)
            .HasForeignKey(rm => rm.RolId);

        builder.HasOne(rm => rm.Menu)
            .WithMany(m => m.RolMenus)
            .HasForeignKey(rm => rm.MenuId);

        // Mismo filtro que sus entidades padre para que no queden relaciones huérfanas en las consultas
        builder.HasQueryFilter(rm => !rm.Rol.IsDeleted && !rm.Menu.IsDeleted);

        // Qué opciones ve cada rol
        builder.HasData(
            // Administrador: inicio, dashboard, usuarios y cajas
            new RolMenu { RolId = Roles.Administrador, MenuId = 1 },
            new RolMenu { RolId = Roles.Administrador, MenuId = 2 },
            new RolMenu { RolId = Roles.Administrador, MenuId = 3 },
            new RolMenu { RolId = Roles.Administrador, MenuId = 4 },
            // Gestor: inicio, usuarios, cajas y asignación de turnos
            new RolMenu { RolId = Roles.Gestor, MenuId = 1 },
            new RolMenu { RolId = Roles.Gestor, MenuId = 3 },
            new RolMenu { RolId = Roles.Gestor, MenuId = 4 },
            new RolMenu { RolId = Roles.Gestor, MenuId = 5 },
            // Cajero: inicio, procesos de caja y clientes
            new RolMenu { RolId = Roles.Cajero, MenuId = 1 },
            new RolMenu { RolId = Roles.Cajero, MenuId = 6 },
            new RolMenu { RolId = Roles.Cajero, MenuId = 7 });
    }
}
