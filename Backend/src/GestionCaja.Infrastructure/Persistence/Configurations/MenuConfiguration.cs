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
    }
}
