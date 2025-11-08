using FluentValidation;
using NotificaPix.Core.Contracts.Requests;

namespace NotificaPix.Api.Validation;

public class CreateApiKeyRequestValidator : AbstractValidator<CreateApiKeyRequest>
{
    public CreateApiKeyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
