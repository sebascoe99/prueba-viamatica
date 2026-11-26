using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Infrastructure.Persistence;
using GestionCaja.Infrastructure.Repositories;
using GestionCaja.Infrastructure.Security;
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

        // Seguridad: cada interfaz de Application se conecta con su implementación
        var jwtSettings = GetJwtSettings(configuration);
        services.AddSingleton(jwtSettings);

        var encryptionKey = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Falta configurar Encryption:Key");
        services.AddSingleton<IEncryptionService>(new AesEncryptionService(encryptionKey));

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Acceso a datos: la misma instancia de AppDbContext atiende a la interfaz que usa Application
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITurnDescriptionGenerator, TurnDescriptionGenerator>();

        return services;
    }

    public static JwtSettings GetJwtSettings(IConfiguration configuration)
    {
        return new JwtSettings
        {
            Key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Falta configurar Jwt:Key"),
            Issuer = configuration["Jwt:Issuer"] ?? "GestionCaja.Api",
            Audience = configuration["Jwt:Audience"] ?? "GestionCaja.Frontend",
            ExpirationMinutes = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var minutes) ? minutes : 60
        };
    }
}
