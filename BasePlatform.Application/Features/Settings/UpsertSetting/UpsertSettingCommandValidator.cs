using FluentValidation;

namespace BasePlatform.Application.Features.Settings.UpsertSetting;

public sealed class UpsertSettingCommandValidator : AbstractValidator<UpsertSettingCommand>
{
    public UpsertSettingCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Setting key is required.")
            .MaximumLength(200);

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Setting value is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
