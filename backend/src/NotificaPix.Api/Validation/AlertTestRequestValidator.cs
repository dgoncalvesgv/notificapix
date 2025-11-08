using FluentValidation;
using NotificaPix.Core.Contracts.Requests;

namespace NotificaPix.Api.Validation;

public class AlertTestRequestValidator : AbstractValidator<AlertTestRequest>
{
    public AlertTestRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PayerName).NotEmpty();
    }
}
