using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCaja.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("service", t =>
        {
            t.HasCheckConstraint("ck_service_price", "price >= 0");
            t.HasCheckConstraint("ck_service_speed", "speed_mbps > 0");
        });

        builder.HasKey(s => s.ServiceId);

        builder.Property(s => s.ServiceName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.ServiceDescription).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Price).HasPrecision(10, 2);

        builder.HasIndex(s => s.ServiceName).IsUnique();

        builder.HasQueryFilter(s => !s.IsDeleted);

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new Service { ServiceId = 1, ServiceName = "Plan Hogar 100", ServiceDescription = "Internet fibra óptica para el hogar", SpeedMbps = 100, Price = 25.00m, CreatedAt = seedDate },
            new Service { ServiceId = 2, ServiceName = "Plan Hogar 300", ServiceDescription = "Internet fibra óptica para streaming y teletrabajo", SpeedMbps = 300, Price = 35.00m, CreatedAt = seedDate },
            new Service { ServiceId = 3, ServiceName = "Plan Pyme 600", ServiceDescription = "Internet fibra óptica para pequeños negocios", SpeedMbps = 600, Price = 60.00m, CreatedAt = seedDate });
    }
}

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("device");
        builder.HasKey(d => d.DeviceId);

        builder.Property(d => d.DeviceName).HasMaxLength(50).IsRequired();

        builder.HasOne(d => d.Service)
            .WithMany(s => s.Devices)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.ServiceId);

        builder.HasQueryFilter(d => !d.IsDeleted);

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new Device { DeviceId = 1, DeviceName = "Router WiFi 5", ServiceId = 1, CreatedAt = seedDate },
            new Device { DeviceId = 2, DeviceName = "ONT fibra óptica", ServiceId = 1, CreatedAt = seedDate },
            new Device { DeviceId = 3, DeviceName = "Router WiFi 6", ServiceId = 2, CreatedAt = seedDate },
            new Device { DeviceId = 4, DeviceName = "ONT fibra óptica", ServiceId = 2, CreatedAt = seedDate },
            new Device { DeviceId = 5, DeviceName = "Router WiFi 6 empresarial", ServiceId = 3, CreatedAt = seedDate },
            new Device { DeviceId = 6, DeviceName = "ONT fibra óptica", ServiceId = 3, CreatedAt = seedDate },
            new Device { DeviceId = 7, DeviceName = "Repetidor mesh", ServiceId = 3, CreatedAt = seedDate });
    }
}
