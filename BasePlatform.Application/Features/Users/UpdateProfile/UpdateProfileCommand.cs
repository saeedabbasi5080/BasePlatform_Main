using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Users.GetCurrentUser;
using BasePlatform.Domain.Enums;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Users.UpdateProfile;

public sealed record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string Username,
    string Bio,
    string ProfilePhotoUrl,
    string? PhoneNumber,
    string Address,
    DateOnly? BirthDate,
    Gender? Gender,
    string JobTitle) : ICommand<Result<UserProfileResponse>>;
