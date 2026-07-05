using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Common;
using BasePlatform.Domain.Entities;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Users.ChangePassword;

public sealed class ChangePasswordCommandHandler
    : ICommandHandler<ChangePasswordCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUser _currentUser;

    public ChangePasswordCommandHandler(
        UserManager<AppUser> userManager,
        ICurrentUser currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<Result> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return Result.Failure(
                Error.Unauthorized("Users.Unauthenticated", "User is not authenticated."));

        var user = await _userManager.FindByIdAsync(_currentUser.UserId.Value.ToString());
        if (user is null || !user.IsActive)
            return Result.Failure(
                Error.NotFound("Users.NotFound", "User not found."));

        var isPhoneUser = PhoneUserLookup.IsPhoneAuthUser(user);

        if (!isPhoneUser && string.IsNullOrWhiteSpace(command.CurrentPassword))
        {
            return Result.Failure(
                Error.Validation("Users.CurrentPasswordRequired", "Current password is required."));
        }

        IdentityResult result;
        if (isPhoneUser)
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            result = await _userManager.ResetPasswordAsync(user, resetToken, command.NewPassword);
        }
        else
        {
            result = await _userManager.ChangePasswordAsync(
                user, command.CurrentPassword, command.NewPassword);
        }

        if (!result.Succeeded)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation("Users.ChangePasswordFailed", errorMessage));
        }

        return Result.Success();
    }
}