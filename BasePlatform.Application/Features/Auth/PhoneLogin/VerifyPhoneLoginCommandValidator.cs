using FluentValidation;

namespace BasePlatform.Application.Features.Auth.PhoneLogin;

public sealed class VerifyPhoneLoginCommandValidator : AbstractValidator<VerifyPhoneLoginCommand>
{
    public VerifyPhoneLoginCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}
