using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Domain.Constants;
using GestionCaja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCaja.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var encryption = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

        await context.Database.MigrateAsync();

        var adminExists = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.UserName == "admin2026");

        if (adminExists)
        {
            return;
        }

        context.Users.Add(new User
        {
            UserName = "admin2026",
            FirstName = "Administrador",
            LastName = "General",
            Email = encryption.Encrypt("admin@gestioncaja.com"),
            Identification = encryption.Encrypt("0999999999"),
            Password = passwordHasher.Hash("Admin2026*"),
            RolId = Roles.Administrador,
            UserStatusId = UserStatuses.Activo,
            DateApproval = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }
}
