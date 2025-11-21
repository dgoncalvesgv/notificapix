namespace NotificaPix.Core.Domain.Entities;

public class PixStaticQrCode : EntityBase
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public Guid PixKeyId { get; set; }
    public PixKey? PixKey { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string TxId { get; set; } = string.Empty;
}
