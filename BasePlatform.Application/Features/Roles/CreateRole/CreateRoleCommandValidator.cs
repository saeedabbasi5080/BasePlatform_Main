using FluentValidation;

namespace BasePlatform.Application.Features.Roles.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
