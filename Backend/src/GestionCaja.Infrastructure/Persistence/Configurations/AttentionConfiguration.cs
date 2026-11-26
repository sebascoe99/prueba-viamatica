using GestionCaja.Domain.Constants;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class AttentionTypeConfiguration : IEntityTypeConfiguration<AttentionType>
{
    public void Configure(EntityTypeBuilder<AttentionType> builder)
    {
        builder.ToTable("attentiontype");
        builder.HasKey(a => a.AttentionTypeId);

        builder.Property(a => a.AttentionTypeId).HasMaxLength(3);
        builder.Property(a => a.Description).HasMaxLength(100).IsRequired();

        builder.HasData(
            new AttentionType { AttentionTypeId = AttentionTypes.AtencionCliente, Description = "Atención al cliente" },
            new AttentionType { AttentionTypeId = AttentionTypes.ContratacionServicio, Description = "Contratación de servicio" },
            new AttentionType { AttentionTypeId = AttentionTypes.PagoServicio, Description = "Pago de servicio" },
            new AttentionType { AttentionTypeId = AttentionTypes.CambioServicio, Description = "Cambio de servicio" },
            new AttentionType { AttentionTypeId = AttentionTypes.CambioFormaPago, Description = "Cambio de forma de pago" },
            new AttentionType { AttentionTypeId = AttentionTypes.CancelacionContrato, Description = "Cancelación de contrato" });
    }
}

public class AttentionStatusConfiguration : IEntityTypeConfiguration<AttentionStatus>
{
    public void Configure(EntityTypeBuilder<AttentionStatus> builder)
    {
        builder.ToTable("attentionstatus");
        builder.HasKey(a => a.StatusId);

        // Los ids son fijos del catálogo, no autogenerados
        builder.Property(a => a.StatusId).ValueGeneratedNever();
        builder.Property(a => a.Description).HasMaxLength(30).IsRequired();

        builder.HasData(
            new AttentionStatus { StatusId = AttentionStatuses.Pendiente, Description = "Pendiente" },
            new AttentionStatus { StatusId = AttentionStatuses.EnAtencion, Description = "En atención" },
            new AttentionStatus { StatusId = AttentionStatuses.Atendido, Description = "Atendido" },
            new AttentionStatus { StatusId = AttentionStatuses.Cancelado, Description = "Cancelado" });
    }
}

public class AttentionConfiguration : IEntityTypeConfiguration<Attention>
{
    public void Configure(EntityTypeBuilder<Attention> builder)
    {
        builder.ToTable("attention");
        builder.HasKey(a => a.AttentionId);

        builder.Property(a => a.AttentionTypeId).HasMaxLength(3).IsRequired();

        builder.HasOne(a => a.Turn)
            .WithMany(t => t.Attentions)
            .HasForeignKey(a => a.TurnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Client)
            .WithMany()
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AttentionType)
            .WithMany()
            .HasForeignKey(a => a.AttentionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AttentionStatus)
            .WithMany()
            .HasForeignKey(a => a.AttentionStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.UserCashier)
            .WithMany()
            .HasForeignKey(a => a.UserCashierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.TurnId);
        builder.HasIndex(a => a.ClientId);
        // Total de turnos atendidos por día y por cajero (bienvenida y dashboard)
        builder.HasIndex(a => new { a.AttentionStatusId, a.AttendedAt });
        builder.HasIndex(a => new { a.UserCashierId, a.AttendedAt });

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
