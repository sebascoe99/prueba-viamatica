using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("client");
        builder.HasKey(c => c.ClientId);

        builder.Property(c => c.Name).HasMaxLength(50).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Identification).HasMaxLength(13).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(120).IsRequired();
        builder.Property(c => c.PhoneNumber).HasMaxLength(13).IsRequired();
        builder.Property(c => c.Address).HasMaxLength(100).IsRequired();
        builder.Property(c => c.ReferenceAddress).HasMaxLength(100).IsRequired();

        builder.HasIndex(c => c.Identification).IsUnique();

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
