using BasePlatform.Application.Common.Abstractions;
using FluentValidation;

namespace BasePlatform.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator(IEmailVerificationPolicy policy)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(policy.CodeLength)
            .WithMessage($"Verification code must be {policy.CodeLength} digits.")
            .Matches("^[0-9]+$").WithMessage("Verification code must be numeric.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
