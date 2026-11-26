using GestionCaja.Application.Auth;
using GestionCaja.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCaja.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
