using FluentValidation;
using NotificaPix.Core.Contracts.Requests;

namespace NotificaPix.Api.Validation;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .NotEqual(x => x.CurrentPassword).WithMessage("A nova senha deve ser diferente da senha atual.");
    }
}
