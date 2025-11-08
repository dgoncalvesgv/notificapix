using FluentValidation;
using NotificaPix.Core.Contracts.Requests;

namespace NotificaPix.Api.Validation;

public class TeamInviteRequestValidator : AbstractValidator<TeamInviteRequest>
{
    public TeamInviteRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
