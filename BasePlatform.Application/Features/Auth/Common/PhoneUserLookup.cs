using BasePlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BasePlatform.Application.Features.Auth.Common;

public static class PhoneUserLookup
{
    public static Task<AppUser?> FindByPhoneAsync(
        UserManager<AppUser> userManager,
        string normalizedPhone,
        CancellationToken cancellationToken = default)
        => userManager.Users.FirstOrDefaultAsync(
            u => u.PhoneNumber == normalizedPhone, cancellationToken);

    public static string BuildSyntheticEmail(string normalizedPhone)
        => $"{normalizedPhone[1..]}@phone.local";

    public static bool IsPhoneAuthUser(AppUser user)
        => user.Email?.EndsWith("@phone.local", StringComparison.OrdinalIgnoreCase) == true;
}
