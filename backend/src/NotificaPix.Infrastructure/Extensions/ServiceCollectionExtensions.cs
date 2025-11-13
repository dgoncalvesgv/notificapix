using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificaPix.Core.Abstractions.Repositories;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Infrastructure.Mapping;
using NotificaPix.Infrastructure.Persistence;
using NotificaPix.Infrastructure.Repositories;
using NotificaPix.Infrastructure.Seed;
using NotificaPix.Infrastructure.Security;
using NotificaPix.Infrastructure.Services;
using NotificaPix.Infrastructure.Workers;

namespace NotificaPix.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificaPixInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificaPixDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString =
                    $"Server={configuration["DB_HOST"] ?? "localhost"};" +
                    $"Port={configuration["DB_PORT"] ?? "3306"};" +
                    $"Database={configuration["DB_NAME"] ?? "notificapix"};" +
                    $"User={configuration["DB_USER"] ?? "root"};" +
                    $"Password={configuration["DB_PASS"] ?? "changeme"};";
            }

            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });
        services.AddAutoMapper(typeof(DomainProfile));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IWebhookSigner, WebhookSigner>();
       services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IEmailSender, FakeEmailSender>();
        services.AddHttpClient("webhooks");
        services.AddHttpClient("bank-sync");
        services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();
        services.AddScoped<IUsageService, UsageService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IOpenFinanceProvider, OpenFinanceMockProvider>(); // TODO: allow swapping to real provider
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IDataSeeder, DataSeeder>();

        var disableWorkers = configuration.GetValue<bool?>("DISABLE_WORKERS") ?? false;
        if (!disableWorkers)
        {
            services.AddHostedService<PixPollingWorker>();
            services.AddHostedService<AlertDispatcherWorker>();
        }
        return services;
    }
}
