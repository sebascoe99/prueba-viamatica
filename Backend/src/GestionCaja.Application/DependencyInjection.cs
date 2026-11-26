using GestionCaja.Application.Auth;
using GestionCaja.Application.Cashes;
using GestionCaja.Application.Catalogs;
using GestionCaja.Application.Clients;
using GestionCaja.Application.Contracts;
using GestionCaja.Application.Dashboard;
using GestionCaja.Application.Turns;
using GestionCaja.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCaja.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ICashService, CashService>();
        services.AddScoped<ITurnService, TurnService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
