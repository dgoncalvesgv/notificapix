namespace NotificaPix.Core.Domain.Entities;

public class BankApiIntegration : EntityBase
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public string Bank { get; set; } = string.Empty;
    public string? SandboxClientId { get; set; }
    public string? SandboxClientSecret { get; set; }
    public string? ProductionClientId { get; set; }
    public string? ProductionClientSecret { get; set; }
    public string? CertificateFileName { get; set; }
    public string? CertificatePassword { get; set; }
    public string? CertificateBase64 { get; set; }
    public bool ProductionEnabled { get; set; }
    public bool IsTested { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public string? ServiceUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? AccountIdentifier { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
