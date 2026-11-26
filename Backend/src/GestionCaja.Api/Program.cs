using GestionCaja.Api.Extensions;
using GestionCaja.Api.Middlewares;
using GestionCaja.Application;
using GestionCaja.Infrastructure;
using GestionCaja.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Registro de servicios: cada capa expone un método que registra lo suyo
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Aplica migraciones pendientes y crea el administrador inicial
await DbInitializer.InitializeAsync(app.Services);

// Pipeline HTTP: el orden importa. Primero el manejo de errores, luego
// autenticación (¿quién eres?) y después autorización (¿qué puedes hacer?).
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
