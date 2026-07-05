using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Users.Common;
using BasePlatform.Application.Features.Users.GetCurrentUser;
using BasePlatform.Domain.Entities;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Users.UpdateProfile;

public sealed class UpdateProfileCommandHandler
    : ICommandHandler<UpdateProfileCommand, Result<UserProfileResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateProfileCommandHandler(
        UserManager<AppUser> userManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<UserProfileResponse>> HandleAsync(
        UpdateProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return Result<UserProfileResponse>.Failure(
                Error.Unauthorized("Users.Unauthenticated", "User is not authenticated."));

        var user = await _userManager.FindByIdAsync(_currentUser.UserId.Value.ToString());
        if (user is null || !user.IsActive)
            return Result<UserProfileResponse>.Failure(
                Error.NotFound("Users.NotFound", "User not found."));

        var normalizedUsername = command.Username.Trim();
        if (!string.Equals(user.UserName, normalizedUsername, StringComparison.OrdinalIgnoreCase))
        {
            var existingByUsername = await _userManager.FindByNameAsync(normalizedUsername);
            if (existingByUsername is not null && existingByUsername.Id != user.Id)
                return Result<UserProfileResponse>.Failure(
                    Error.Conflict("Users.UsernameTaken", "This username is already taken."));
        }

        if (!string.IsNullOrWhiteSpace(command.PhoneNumber)
            && !string.Equals(user.PhoneNumber, command.PhoneNumber, StringComparison.Ordinal))
        {
            var existingByPhone = _userManager.Users
                .FirstOrDefault(u => u.PhoneNumber == command.PhoneNumber && u.Id != user.Id);
            if (existingByPhone is not null)
                return Result<UserProfileResponse>.Failure(
                    Error.Conflict("Users.PhoneTaken", "This phone number is already registered."));
        }

        user.FirstName = command.FirstName.Trim();
        user.LastName = command.LastName.Trim();
        user.DisplayName = $"{user.FirstName} {user.LastName}".Trim();
        user.UserName = normalizedUsername;
        user.NormalizedUserName = normalizedUsername.ToUpperInvariant();
        user.Bio = command.Bio.Trim();
        // ProfilePhotoUrl is managed only via UploadProfilePhoto — ignore client value.
        user.PhoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber)
            ? null
            : command.PhoneNumber.Trim();
        user.Address = command.Address.Trim();
        user.BirthDate = command.BirthDate;
        user.Gender = command.Gender;
        user.JobTitle = command.JobTitle.Trim();
        user.UpdatedAt = _dateTimeProvider.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result<UserProfileResponse>.Failure(
                Error.Validation("Users.UpdateFailed", errorMessage));
        }

        var profile = await UserProfileMapper.MapAsync(user, _userManager);
        return Result<UserProfileResponse>.Success(profile);
    }
}
