using FluentValidation;

namespace BasePlatform.Application.Features.Auth.PhoneRegister;

public sealed class VerifyPhoneRegisterCommandValidator : AbstractValidator<VerifyPhoneRegisterCommand>
{
    public VerifyPhoneRegisterCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}
