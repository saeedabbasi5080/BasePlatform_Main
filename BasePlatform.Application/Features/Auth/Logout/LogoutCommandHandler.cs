using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenService _refreshTokenService;

    public LogoutCommandHandler(IRefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }

    public async Task<Result> HandleAsync(
        LogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
            return Result.Success();

        // Idempotent: revoke by refresh-token hash; no Bearer header required.
        var tokenHash = _refreshTokenService.HashToken(command.RefreshToken);
        await _refreshTokenService.RevokeByHashAsync(tokenHash, cancellationToken);
        return Result.Success();
    }
}
