using GestionCaja.Api.Extensions;
using GestionCaja.Api.Middlewares;
using GestionCaja.Api.Services;
using GestionCaja.Application;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Infrastructure;
using GestionCaja.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Serilog reemplaza al logger por defecto. Todo lo que el código escribe con ILogger sale por aquí:
//  - consola
//  - logs/gestioncaja-<fecha>.log : procesos de creación, actualización, eliminación e inicios de sesión
//  - logs/alertas-<fecha>.log     : solo advertencias y errores
builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gestioncaja-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.File("logs/alertas-.log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning));

// Registro de servicios: cada capa expone un método que registra lo suyo
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();

// Respuesta uniforme cuando un DTO no pasa sus validaciones: { message, errors }
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value!.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error => error.ErrorMessage))
            .ToList();

        return new BadRequestObjectResult(new { message = string.Join(". ", errors), errors });
    };
});

// CORS para el frontend de Angular
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Aplica migraciones pendientes y crea el administrador inicial
await DbInitializer.InitializeAsync(app.Services);

// Pipeline HTTP: el orden importa. Primero el manejo de errores, luego
// autenticación (¿quién eres?) y después autorización (¿qué puedes hacer?).
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
