using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class TurnConfiguration : IEntityTypeConfiguration<Turn>
{
    public void Configure(EntityTypeBuilder<Turn> builder)
    {
        builder.ToTable("turn", t =>
            // La base también valida el formato: 2 letras mayúsculas + 4 números
            t.HasCheckConstraint("ck_turn_description_format", "description ~ '^[A-Z]{2}[0-9]{4}$'"));

        builder.HasKey(t => t.TurnId);

        builder.Property(t => t.Description).HasMaxLength(6).IsRequired();
        builder.Property(t => t.AttentionTypeId).HasMaxLength(3).IsRequired();

        builder.HasOne(t => t.Cash)
            .WithMany(c => c.Turns)
            .HasForeignKey(t => t.CashId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.UserGestor)
            .WithMany()
            .HasForeignKey(t => t.UserGestorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AttentionType)
            .WithMany()
            .HasForeignKey(t => t.AttentionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Turnos de una caja en una fecha (pantalla del cajero)
        builder.HasIndex(t => new { t.CashId, t.Date });
        // Turnos asignados por un gestor en una fecha (pantalla de bienvenida del gestor)
        builder.HasIndex(t => new { t.UserGestorId, t.Date });
        // Para calcular el siguiente secuencial por tipo de atención
        builder.HasIndex(t => new { t.AttentionTypeId, t.Date });

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
