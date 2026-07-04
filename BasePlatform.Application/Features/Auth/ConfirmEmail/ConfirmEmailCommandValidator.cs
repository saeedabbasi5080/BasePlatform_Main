using BasePlatform.Application.Common.Abstractions;
using FluentValidation;

namespace BasePlatform.Application.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator(IEmailVerificationPolicy policy)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(policy.CodeLength)
            .WithMessage($"Verification code must be {policy.CodeLength} digits.")
            .Matches("^[0-9]+$").WithMessage("Verification code must be numeric.");
    }
}
