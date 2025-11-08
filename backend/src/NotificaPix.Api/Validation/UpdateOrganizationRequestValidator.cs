using FluentValidation;
using NotificaPix.Core.Contracts.Requests;

namespace NotificaPix.Api.Validation;

public class UpdateOrganizationRequestValidator : AbstractValidator<UpdateOrganizationRequest>
{
    public UpdateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.BillingEmail).NotEmpty().EmailAddress();
    }
}
