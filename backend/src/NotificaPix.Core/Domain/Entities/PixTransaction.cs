namespace NotificaPix.Core.Domain.Entities;

public class PixTransaction : EntityBase
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public string TxId { get; set; } = string.Empty;
    public string EndToEndId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime OccurredAt { get; set; }
    public string PayerName { get; set; } = string.Empty;
    public string PayerKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RawJson { get; set; } = string.Empty;
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
