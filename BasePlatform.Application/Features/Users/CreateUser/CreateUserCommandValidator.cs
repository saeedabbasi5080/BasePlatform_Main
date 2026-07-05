using FluentValidation;

namespace BasePlatform.Application.Features.Users.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Username).MaximumLength(256);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(256);
    }
}
