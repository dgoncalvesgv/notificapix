using FluentValidation;
using NotificaPix.Core.Contracts.Requests;

namespace NotificaPix.Api.Validation;

public class NotificationSettingsRequestValidator : AbstractValidator<NotificationSettingsRequest>
{
    public NotificationSettingsRequestValidator()
    {
        RuleForEach(x => x.Emails).EmailAddress();
        RuleFor(x => x.WebhookUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Webhook URL inválida");
        RuleFor(x => x.WebhookSecret)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.WebhookUrl));
    }
}
