using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Shared;

namespace BasePlatform.Application.Common;

internal static class AuthGuard
{
    public static Result<Guid> RequireUserId(ICurrentUser currentUser)
    {
        if (currentUser.UserId is null)
            return Result<Guid>.Failure(
                Error.Unauthorized("Auth.Unauthenticated", "User is not authenticated."));

        return Result<Guid>.Success(currentUser.UserId.Value);
    }
}
