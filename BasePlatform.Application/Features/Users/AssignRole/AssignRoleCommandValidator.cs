using FluentValidation;

namespace BasePlatform.Application.Features.Users.AssignRole;

public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required.");
    }
}
