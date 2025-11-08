namespace NotificaPix.Core.Domain.Entities;

public class NotificationSettings : EntityBase
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public string EmailsCsv { get; set; } = string.Empty;
    public string? WebhookUrl { get; set; }
    public string? WebhookSecret { get; set; }
    public bool Enabled { get; set; } = true;
}
