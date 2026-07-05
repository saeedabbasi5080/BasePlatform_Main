using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Users.Common;
using BasePlatform.Application.Features.Users.GetCurrentUser;
using BasePlatform.Domain.Entities;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Users.GetUserById;

public sealed class GetUserByIdQueryHandler
    : IQueryHandler<GetUserByIdQuery, Result<UserProfileResponse>>
{
    private readonly UserManager<AppUser> _userManager;

    public GetUserByIdQueryHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserProfileResponse>> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(query.UserId.ToString());
        if (user is null)
            return Result<UserProfileResponse>.Failure(
                Error.NotFound("Users.NotFound", "User not found."));

        var profile = await UserProfileMapper.MapAsync(user, _userManager);
        return Result<UserProfileResponse>.Success(profile);
    }
}
