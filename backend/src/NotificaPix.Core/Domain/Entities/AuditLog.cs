namespace NotificaPix.Core.Domain.Entities;

public class AuditLog : EntityBase
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string DataJson { get; set; } = string.Empty;
}
