using GestionCaja.Domain.Constants;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("rol");
        builder.HasKey(r => r.RolId);

        builder.Property(r => r.RolName).HasMaxLength(50).IsRequired();
        builder.HasIndex(r => r.RolName).IsUnique();

        // Filtro global: las consultas ignoran los registros eliminados lógicamente
        builder.HasQueryFilter(r => !r.IsDeleted);

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new Rol { RolId = Roles.Administrador, RolName = "Administrador", CreatedAt = seedDate },
            new Rol { RolId = Roles.Gestor, RolName = "Gestor", CreatedAt = seedDate },
            new Rol { RolId = Roles.Cajero, RolName = "Cajero", CreatedAt = seedDate });
    }
}
