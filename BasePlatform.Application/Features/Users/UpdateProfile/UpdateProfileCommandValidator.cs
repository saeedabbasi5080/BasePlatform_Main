using BasePlatform.Domain.Enums;
using FluentValidation;

namespace BasePlatform.Application.Features.Users.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(30)
            .Matches("^[a-zA-Z0-9][a-zA-Z0-9_.]*$")
            .WithMessage(
                "Username may only contain English letters, digits, underscores, and dots (3-30 characters).");
        RuleFor(x => x.Bio).MaximumLength(1000);
        RuleFor(x => x.ProfilePhotoUrl).MaximumLength(2048);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.JobTitle).MaximumLength(200);
        RuleFor(x => x.Gender)
            .Must(g => g is null || Enum.IsDefined(typeof(Gender), g.Value))
            .WithMessage("Invalid gender value.");
        RuleFor(x => x.BirthDate)
            .Must(d => d is null || d.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Birth date cannot be in the future.");
    }
}
