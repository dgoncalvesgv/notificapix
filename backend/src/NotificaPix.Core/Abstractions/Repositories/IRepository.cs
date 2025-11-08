using NotificaPix.Core.Domain.Entities;

namespace NotificaPix.Core.Abstractions.Repositories;

public interface IRepository<T> where T : EntityBase
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T?> FindAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<T> Query();
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
