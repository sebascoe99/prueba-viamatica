using GestionCaja.Domain.Constants;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class UserStatusConfiguration : IEntityTypeConfiguration<UserStatus>
{
    public void Configure(EntityTypeBuilder<UserStatus> builder)
    {
        builder.ToTable("userstatus");
        builder.HasKey(s => s.StatusId);

        builder.Property(s => s.StatusId).HasMaxLength(3);
        builder.Property(s => s.Description).HasMaxLength(50).IsRequired();

        builder.HasData(
            new UserStatus { StatusId = UserStatuses.Activo, Description = "Activo" },
            new UserStatus { StatusId = UserStatuses.Inactivo, Description = "Inactivo" },
            new UserStatus { StatusId = UserStatuses.PendienteAprobacion, Description = "Pendiente de aprobación" },
            new UserStatus { StatusId = UserStatuses.Bloqueado, Description = "Bloqueado" });
    }
}
