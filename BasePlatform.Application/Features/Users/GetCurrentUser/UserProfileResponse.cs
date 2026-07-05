using BasePlatform.Domain.Enums;

namespace BasePlatform.Application.Features.Users.GetCurrentUser;

public sealed record UserProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Username,
    string Bio,
    string ProfilePhotoUrl,
    string? PhoneNumber,
    string Email,
    string Address,
    DateOnly? BirthDate,
    Gender? Gender,
    string JobTitle,
    bool IsActive,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
