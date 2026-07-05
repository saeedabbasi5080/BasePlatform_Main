using BasePlatform.Domain.Enums;

namespace BasePlatform.Application.Features.Users.GetAllUsers;

public sealed record UserSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Username,
    string Email,
    string? PhoneNumber,
    string JobTitle,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
