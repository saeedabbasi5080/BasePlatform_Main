using FluentValidation;

namespace BasePlatform.Application.Features.Auth.PhoneRegister;

public sealed class SendPhoneRegisterCodeCommandValidator : AbstractValidator<SendPhoneRegisterCodeCommand>
{
    public SendPhoneRegisterCodeCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
