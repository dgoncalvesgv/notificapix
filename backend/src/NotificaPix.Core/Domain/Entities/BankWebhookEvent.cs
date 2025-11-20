namespace NotificaPix.Core.Domain.Entities;

public class BankWebhookEvent : EntityBase
{
    public Guid BankApiIntegrationId { get; set; }
    public BankApiIntegration? Integration { get; set; }
    public Guid OrganizationId { get; set; }
    public string Bank { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
