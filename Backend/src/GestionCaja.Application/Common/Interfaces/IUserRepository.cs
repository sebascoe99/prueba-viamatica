using GestionCaja.Domain.Entities;

namespace GestionCaja.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<List<User>> ListAsync(int? rolId, string? statusId, string? search, int? createdBy, CancellationToken cancellationToken = default);

    void Add(User user);
    void Remove(User user);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
