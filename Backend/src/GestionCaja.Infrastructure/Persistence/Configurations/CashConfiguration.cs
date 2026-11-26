using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class CashConfiguration : IEntityTypeConfiguration<Cash>
{
    public void Configure(EntityTypeBuilder<Cash> builder)
    {
        builder.ToTable("cash");
        builder.HasKey(c => c.CashId);

        builder.Property(c => c.CashDescription).HasMaxLength(50).IsRequired();

        builder.HasOne(c => c.CurrentUser)
            .WithMany()
            .HasForeignKey(c => c.CurrentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un cajero solo puede estar operando una caja a la vez
        builder.HasIndex(c => c.CurrentUserId)
            .IsUnique()
            .HasFilter("current_user_id IS NOT NULL");

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class UserCashConfiguration : IEntityTypeConfiguration<UserCash>
{
    public void Configure(EntityTypeBuilder<UserCash> builder)
    {
        builder.ToTable("usercash");
        builder.HasKey(uc => uc.UserCashId);

        builder.HasOne(uc => uc.User)
            .WithMany()
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(uc => uc.Cash)
            .WithMany(c => c.UserCashes)
            .HasForeignKey(uc => uc.CashId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice único parcial: no se repite la asignación vigente cajero-caja,
        // pero sí se permite volver a asignar después de una eliminación lógica.
        builder.HasIndex(uc => new { uc.UserId, uc.CashId })
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(uc => uc.CashId);

        builder.HasQueryFilter(uc => !uc.IsDeleted);
    }
}
