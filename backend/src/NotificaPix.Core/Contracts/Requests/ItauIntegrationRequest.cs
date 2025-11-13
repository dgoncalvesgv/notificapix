namespace NotificaPix.Core.Contracts.Requests;

public record ItauIntegrationRequest(
    string SandboxClientId,
    string SandboxClientSecret,
    string ProductionClientId,
    string ProductionClientSecret,
    string CertificatePassword,
    string CertificateFileName,
    string CertificateBase64,
    bool ProductionEnabled,
    string ServiceUrl,
    string ApiKey,
    string AccountIdentifier);
