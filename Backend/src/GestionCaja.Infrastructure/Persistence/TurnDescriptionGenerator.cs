using GestionCaja.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionCaja.Infrastructure.Persistence;

/// <summary>
/// Llama a la función de PostgreSQL fn_next_turn_description, que devuelve el siguiente
/// código de turno del día para un tipo de atención (por ejemplo AC0001, AC0002...).
/// </summary>
public class TurnDescriptionGenerator : ITurnDescriptionGenerator
{
    private readonly AppDbContext _context;

    public TurnDescriptionGenerator(AppDbContext context)
    {
        _context = context;
    }

    public Task<string> NextAsync(string attentionTypeId, CancellationToken cancellationToken = default)
    {
        // La interpolación de SqlQuery se envía como parámetro, no se concatena (evita inyección SQL)
        return _context.Database
            .SqlQuery<string>($"SELECT fn_next_turn_description({attentionTypeId}) AS \"Value\"")
            .SingleAsync(cancellationToken);
    }
}
