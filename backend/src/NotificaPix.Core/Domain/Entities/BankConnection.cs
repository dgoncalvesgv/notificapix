using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Domain.Entities;

public class BankConnection : EntityBase
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public BankProvider Provider { get; set; } = BankProvider.Mock;
    public string ConsentId { get; set; } = string.Empty;
    public BankConnectionStatus Status { get; set; } = BankConnectionStatus.Pending;
    public DateTime? ConnectedAt { get; set; }
    public string? MetaJson { get; set; }
}
