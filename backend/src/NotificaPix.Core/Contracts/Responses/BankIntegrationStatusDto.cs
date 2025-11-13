namespace NotificaPix.Core.Contracts.Responses;

public record BankIntegrationStatusDto(
    bool Configured,
    DateTime? UpdatedAt,
    Guid? IntegrationId,
    string? Bank,
    DateTime? CreatedAt,
    bool ProductionEnabled,
    DateTime? LastTestedAt,
    string? ServiceUrl,
    string? ApiKey,
    string? AccountIdentifier,
    string? SandboxClientId,
    string? SandboxClientSecret,
    string? ProductionClientId,
    string? ProductionClientSecret,
    string? CertificatePassword,
    string? CertificateFileName,
    bool HasCertificate);
