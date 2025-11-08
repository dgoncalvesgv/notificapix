using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Domain.Entities;

public class Alert : EntityBase
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public Guid PixTransactionId { get; set; }
    public PixTransaction? PixTransaction { get; set; }
    public AlertChannel Channel { get; set; }
    public AlertStatus Status { get; set; } = AlertStatus.Pending;
    public int Attempts { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
