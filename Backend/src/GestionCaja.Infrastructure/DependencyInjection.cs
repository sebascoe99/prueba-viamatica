using GestionCaja.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCaja.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra en el contenedor todo lo que aporta la capa de infraestructura.
    /// La Api solo llama a este método y no conoce los detalles (EF Core, PostgreSQL).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                // Convierte UserId -> user_id, CreatedAt -> created_at (convención de PostgreSQL)
                .UseSnakeCaseNamingConvention());

        return services;
    }
}
