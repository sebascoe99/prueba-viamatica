using System.Security.Claims;
using System.Text;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Infrastructure;
using GestionCaja.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace GestionCaja.Api.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = DependencyInjection.GetJwtSettings(configuration);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Se conservan los nombres de claim tal como vienen en el token (uid, usr, rol)
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };

                options.Events = new JwtBearerEvents
                {
                    // Después de validar firma y vigencia, se descifran los claims y se arma
                    // la identidad con valores legibles. Así [Authorize(Roles = "...")] funciona normal.
                    OnTokenValidated = context =>
                    {
                        var encryption = context.HttpContext.RequestServices.GetRequiredService<IEncryptionService>();
                        var principal = context.Principal!;

                        try
                        {
                            var claims = new List<Claim>
                            {
                                new(ClaimTypes.NameIdentifier, encryption.Decrypt(principal.FindFirstValue(JwtClaimNames.UserId)!)),
                                new(ClaimTypes.Name, encryption.Decrypt(principal.FindFirstValue(JwtClaimNames.UserName)!)),
                                new(JwtClaimNames.RolId, encryption.Decrypt(principal.FindFirstValue(JwtClaimNames.RolId)!)),
                                new(ClaimTypes.Role, encryption.Decrypt(principal.FindFirstValue(JwtClaimNames.RolName)!))
                            };

                            var identity = new ClaimsIdentity(
                                claims,
                                JwtBearerDefaults.AuthenticationScheme,
                                ClaimTypes.Name,
                                ClaimTypes.Role);

                            context.Principal = new ClaimsPrincipal(identity);
                        }
                        catch (Exception)
                        {
                            context.Fail("No se pudieron descifrar los claims del token");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
