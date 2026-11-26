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
        return WithRelations().FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return WithRelations().FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
    }

    public Task<bool> ExistsUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.UserName.ToLower() == userName.ToLower(), cancellationToken);
    }

    public Task<List<User>> ListAsync(int? rolId, string? statusId, string? search, int? createdBy, CancellationToken cancellationToken = default)
    {
        var query = WithRelations().AsNoTracking();

        if (rolId.HasValue)
        {
            query = query.Where(u => u.RolId == rolId.Value);
        }

        if (!string.IsNullOrWhiteSpace(statusId))
        {
            query = query.Where(u => u.UserStatusId == statusId);
        }

        if (createdBy.HasValue)
        {
            query = query.Where(u => u.UserCreate == createdBy.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.UserName, term) ||
                EF.Functions.ILike(u.FirstName, term) ||
                EF.Functions.ILike(u.LastName, term));
        }

        return query.OrderBy(u => u.UserName).ToListAsync(cancellationToken);
    }

    public void Add(User user) => _context.Users.Add(user);

    public void Remove(User user) => _context.Users.Remove(user);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<User> WithRelations()
    {
        return _context.Users
            .Include(u => u.Rol)
            .Include(u => u.UserStatus);
    }
}
