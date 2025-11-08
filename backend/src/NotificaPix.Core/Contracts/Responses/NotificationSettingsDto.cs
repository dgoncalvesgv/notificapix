namespace NotificaPix.Core.Contracts.Responses;

public record NotificationSettingsDto(IReadOnlyCollection<string> Emails, string? WebhookUrl, string? WebhookSecret, bool Enabled);
