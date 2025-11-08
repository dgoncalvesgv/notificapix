using Microsoft.EntityFrameworkCore;
using NotificaPix.Core.Abstractions.Repositories;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Infrastructure.Repositories;

public class Repository<T>(NotificaPixDbContext context) : IRepository<T> where T : EntityBase
{
    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        context.Set<T>().Add(entity);
        return Task.CompletedTask;
    }

    public Task<T?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public IQueryable<T> Query() => context.Set<T>().AsQueryable();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
