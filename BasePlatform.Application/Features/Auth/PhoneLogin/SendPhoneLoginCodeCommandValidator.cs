using FluentValidation;

namespace BasePlatform.Application.Features.Auth.PhoneLogin;

public sealed class SendPhoneLoginCodeCommandValidator : AbstractValidator<SendPhoneLoginCodeCommand>
{
    public SendPhoneLoginCodeCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty();
    }
}
