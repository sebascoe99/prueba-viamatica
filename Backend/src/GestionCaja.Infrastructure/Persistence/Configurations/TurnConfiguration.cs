using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class TurnConfiguration : IEntityTypeConfiguration<Turn>
{
    public void Configure(EntityTypeBuilder<Turn> builder)
    {
        builder.ToTable("turn", t =>
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

        builder.HasIndex(t => new { t.CashId, t.Date });
        builder.HasIndex(t => new { t.UserGestorId, t.Date });
        builder.HasIndex(t => new { t.AttentionTypeId, t.Date });

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
