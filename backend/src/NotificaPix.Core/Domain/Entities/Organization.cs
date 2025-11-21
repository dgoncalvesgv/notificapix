using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Domain.Entities;

public class Organization : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public PlanType Plan { get; set; } = PlanType.Starter;
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime UsageMonth { get; set; } = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
    public int UsageCount { get; set; }
    public string BillingEmail { get; set; } = string.Empty;
    public string? StripePriceId { get; set; }
    public Guid? DefaultPixKeyId { get; set; }
    public PixKey? DefaultPixKey { get; set; }
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<BankConnection> BankConnections { get; set; } = new List<BankConnection>();
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    public ICollection<PixTransaction> PixTransactions { get; set; } = new List<PixTransaction>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<Invite> Invites { get; set; } = new List<Invite>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public NotificationSettings? NotificationSettings { get; set; }
}
