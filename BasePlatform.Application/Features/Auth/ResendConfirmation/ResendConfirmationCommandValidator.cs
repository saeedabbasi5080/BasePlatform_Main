using FluentValidation;

namespace BasePlatform.Application.Features.Auth.ResendConfirmation;

public sealed class ResendConfirmationCommandValidator : AbstractValidator<ResendConfirmationCommand>
{
    public ResendConfirmationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
