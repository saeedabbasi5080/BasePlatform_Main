using FluentValidation;

namespace BasePlatform.Application.Features.Roles.UpdateRole;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
