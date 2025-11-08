namespace NotificaPix.Core.Abstractions.Services;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}
