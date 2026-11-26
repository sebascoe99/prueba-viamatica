using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // "user" es palabra reservada en PostgreSQL, por eso la tabla se llama "users"
        builder.ToTable("users");
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserName).HasMaxLength(20).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(50).IsRequired();

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Identification).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Password).HasMaxLength(256).IsRequired();

        builder.Property(u => u.UserStatusId).HasMaxLength(3).IsRequired();

        builder.HasOne(u => u.Rol)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.UserStatus)
            .WithMany(s => s.Users)
            .HasForeignKey(u => u.UserStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasIndex(u => u.RolId);
        builder.HasIndex(u => u.UserStatusId);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
