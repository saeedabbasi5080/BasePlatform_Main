using BasePlatform.Application.Common;
using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Users.Common;
using BasePlatform.Application.Features.Users.GetCurrentUser;
using BasePlatform.Domain.Entities;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Users.CreateUser;

public sealed class CreateUserCommandHandler
    : ICommandHandler<CreateUserCommand, Result<UserProfileResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUser _currentUser;

    public CreateUserCommandHandler(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dateTimeProvider = dateTimeProvider;
        _currentUser = currentUser;
    }

    public async Task<Result<UserProfileResponse>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var privilegedCheck = RolePolicy.ValidatePrivilegedRoleAssignment(
            _currentUser, command.RoleName);
        if (privilegedCheck is not null)
            return Result<UserProfileResponse>.Failure(privilegedCheck.Error);
        var email = command.Email.Trim();
        if (await _userManager.FindByEmailAsync(email) is not null)
            return Result<UserProfileResponse>.Failure(
                Error.Conflict("Users.EmailTaken", "This email address is already registered."));

        var username = string.IsNullOrWhiteSpace(command.Username)
            ? email
            : command.Username.Trim();

        if (await _userManager.FindByNameAsync(username) is not null)
            return Result<UserProfileResponse>.Failure(
                Error.Conflict("Users.UsernameTaken", "This username is already taken."));

        if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            var phone = command.PhoneNumber.Trim();
            if (_userManager.Users.Any(u => u.PhoneNumber == phone))
                return Result<UserProfileResponse>.Failure(
                    Error.Conflict("Users.PhoneTaken", "This phone number is already registered."));
        }

        if (!await _roleManager.RoleExistsAsync(command.RoleName))
            return Result<UserProfileResponse>.Failure(
                Error.NotFound("Roles.NotFound", $"Role '{command.RoleName}' was not found."));

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            DisplayName = $"{command.FirstName.Trim()} {command.LastName.Trim()}",
            Email = email,
            UserName = username,
            PhoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber)
                ? null
                : command.PhoneNumber.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            var errorMessage = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result<UserProfileResponse>.Failure(
                Error.Validation("Users.CreateFailed", errorMessage));
        }

        await _userManager.AddToRoleAsync(user, command.RoleName);

        var profile = await UserProfileMapper.MapAsync(user, _userManager);
        return Result<UserProfileResponse>.Success(profile);
    }
}
