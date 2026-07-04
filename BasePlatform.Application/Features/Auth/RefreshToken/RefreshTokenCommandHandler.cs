using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Entities;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPermissionRepository _permissionRepository;

    public RefreshTokenCommandHandler(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IDateTimeProvider dateTimeProvider,
        IPermissionRepository permissionRepository)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _dateTimeProvider = dateTimeProvider;
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<RefreshTokenResponse>> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _jwtTokenService.GetUserIdFromExpiredToken(command.AccessToken);
        if (userId is null)
            return Result<RefreshTokenResponse>.Failure(
                Error.Unauthorized("Auth.InvalidToken", "Invalid access token."));

        var tokenHash = _refreshTokenService.HashToken(command.RefreshToken);

        var isValid = await _refreshTokenService.ValidateAsync(userId.Value, tokenHash, cancellationToken);
        if (!isValid)
            return Result<RefreshTokenResponse>.Failure(
                Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is invalid or expired."));

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null || !user.IsActive)
            return Result<RefreshTokenResponse>.Failure(
                Error.Unauthorized("Auth.InvalidToken", "User not found or inactive."));

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionRepository.GetPermissionNamesForUserAsync(user.Id, cancellationToken);

        var newAccessToken = _jwtTokenService.GenerateAccessToken(
            user.Id, user.Email!, roles.ToList(), permissions);

        var newRawRefreshToken = _refreshTokenService.GenerateToken();
        var newTokenHash = _refreshTokenService.HashToken(newRawRefreshToken);

        var newRefreshToken = new BasePlatform.Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newTokenHash,
            ExpiresAt = _dateTimeProvider.UtcNow.AddDays(7),
            CreatedAt = _dateTimeProvider.UtcNow
        };

        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);

        // Rotation: revoke the old token and link it to the replacement.
        await _refreshTokenService.RevokeAsync(
            userId.Value, tokenHash, newRefreshToken.Id, cancellationToken);

        return Result<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(newAccessToken, newRawRefreshToken, newRefreshToken.ExpiresAt));
    }
}