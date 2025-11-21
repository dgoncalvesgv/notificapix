namespace NotificaPix.Core.Domain.Entities;

public class PixKey : EntityBase
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public string Label { get; set; } = string.Empty;
    public string KeyType { get; set; } = string.Empty;
    public string KeyValue { get; set; } = string.Empty;
}
