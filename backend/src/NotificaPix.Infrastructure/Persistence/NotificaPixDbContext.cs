using Microsoft.EntityFrameworkCore;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Infrastructure.Persistence;

public class NotificaPixDbContext(DbContextOptions<NotificaPixDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<BankConnection> BankConnections => Set<BankConnection>();
    public DbSet<PixTransaction> PixTransactions => Set<PixTransaction>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<NotificationSettings> NotificationSettings => Set<NotificationSettings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Invite> Invites => Set<Invite>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<BankApiIntegration> BankApiIntegrations => Set<BankApiIntegration>();
    public DbSet<BankWebhookEvent> BankWebhookEvents => Set<BankWebhookEvent>();
    public DbSet<PixStaticQrCode> PixStaticQrCodes => Set<PixStaticQrCode>();
    public DbSet<PixKey> PixKeys => Set<PixKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Organization>(entity =>
        {
            entity.HasIndex(o => o.Slug).IsUnique();
            entity.Property(o => o.Plan).HasConversion<string>().HasMaxLength(32);
            entity.Property(o => o.BillingEmail).HasMaxLength(256);
            entity.HasOne(o => o.DefaultPixKey)
                .WithMany()
                .HasForeignKey(o => o.DefaultPixKeyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
        });

        builder.Entity<Membership>(entity =>
        {
            entity.HasIndex(x => new { x.OrganizationId, x.UserId }).IsUnique();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
        });

        builder.Entity<BankConnection>(entity =>
        {
            entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        });

        builder.Entity<Alert>(entity =>
        {
            entity.Property(x => x.Channel).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        });

        builder.Entity<Invite>(entity =>
        {
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => x.Token).IsUnique();
        });

        builder.Entity<ApiKey>(entity =>
        {
            entity.HasIndex(x => new { x.OrganizationId, x.Name });
        });

        builder.Entity<BankApiIntegration>(entity =>
        {
            entity.HasIndex(x => new { x.OrganizationId, x.Bank }).IsUnique();
            entity.Property(x => x.Bank).HasMaxLength(64);
            entity.Property(x => x.ServiceUrl).HasMaxLength(512);
            entity.Property(x => x.ApiKey).HasMaxLength(64);
            entity.Property(x => x.AccountIdentifier).HasMaxLength(64);
        });

        builder.Entity<BankWebhookEvent>(entity =>
        {
            entity.HasIndex(x => new { x.BankApiIntegrationId, x.EventId }).IsUnique();
            entity.Property(x => x.Bank).HasMaxLength(64);
            entity.Property(x => x.EventId).HasMaxLength(128);
            entity.Property(x => x.EventType).HasMaxLength(64);
            entity.Property(x => x.Signature).HasMaxLength(256);
        });

        builder.Entity<PixKey>(entity =>
        {
            entity.Property(x => x.Label).HasMaxLength(128);
            entity.Property(x => x.KeyType).HasMaxLength(32);
            entity.Property(x => x.KeyValue).HasMaxLength(128);
            entity.HasIndex(x => new { x.OrganizationId, x.KeyValue }).IsUnique();
            entity.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PixStaticQrCode>(entity =>
        {
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Payload).HasColumnType("text");
            entity.Property(x => x.TxId).HasMaxLength(64);
            entity.HasIndex(x => x.OrganizationId);
            entity.HasIndex(x => x.PixKeyId);
            entity.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.PixKey)
                .WithMany()
                .HasForeignKey(x => x.PixKeyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
