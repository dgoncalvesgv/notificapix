using NotificaPix.Core.Abstractions.Repositories;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Infrastructure.Repositories;

public class UnitOfWork(NotificaPixDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
