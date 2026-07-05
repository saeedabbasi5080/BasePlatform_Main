using BasePlatform.Application.Features.Users.GetCurrentUser;
using BasePlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Users.Common;

internal static class UserProfileMapper
{
    public static async Task<UserProfileResponse> MapAsync(
        AppUser user,
        UserManager<AppUser> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);
        var permissions = claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        return new UserProfileResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.UserName ?? string.Empty,
            user.Bio,
            user.ProfilePhotoUrl,
            user.PhoneNumber,
            user.Email ?? string.Empty,
            user.Address,
            user.BirthDate,
            user.Gender,
            user.JobTitle,
            user.IsActive,
            roles.ToList(),
            permissions,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
