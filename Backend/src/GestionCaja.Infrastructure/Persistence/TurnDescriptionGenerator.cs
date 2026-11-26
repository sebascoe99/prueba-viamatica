using GestionCaja.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionCaja.Infrastructure.Persistence;

public class TurnDescriptionGenerator : ITurnDescriptionGenerator
{
    private readonly AppDbContext _context;

    public TurnDescriptionGenerator(AppDbContext context)
    {
        _context = context;
    }

    public Task<string> NextAsync(string attentionTypeId, CancellationToken cancellationToken = default)
    {
        return _context.Database
            .SqlQuery<string>($"SELECT fn_next_turn_description({attentionTypeId}) AS \"Value\"")
            .SingleAsync(cancellationToken);
    }
}
