using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Domain.Entities;
using GestionCaja.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionCaja.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
