using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Users.GetCurrentUser;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Users.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Username,
    string? PhoneNumber,
    string RoleName) : ICommand<Result<UserProfileResponse>>;
