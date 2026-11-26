using Microsoft.OpenApi;

namespace GestionCaja.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Gestión de Caja API",
                Version = "v1",
                Description = "API REST para la gestión de caja de una empresa de servicios de internet"
            });

            // Botón "Authorize" de Swagger: permite pegar el token y lo envía en cada petición
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Pegue aquí el token devuelto por /api/auth/login"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });

            // Muestra en Swagger los comentarios /// de los controladores
            var xmlFile = Path.Combine(AppContext.BaseDirectory, "GestionCaja.Api.xml");
            if (File.Exists(xmlFile))
            {
                options.IncludeXmlComments(xmlFile);
            }
        });

        return services;
    }
}
