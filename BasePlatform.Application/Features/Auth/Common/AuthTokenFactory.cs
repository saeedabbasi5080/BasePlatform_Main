using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Login;
using BasePlatform.Domain.Entities;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.Common;

public sealed class AuthTokenFactory
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPermissionRepository _permissionRepository;

    public AuthTokenFactory(
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

    public async Task<Result<LoginResponse>> CreateLoginResponseAsync(
        AppUser user,
        CancellationToken cancellationToken = default)
    {
        if (!user.IsActive)
            return Result<LoginResponse>.Failure(
                Error.Unauthorized("Auth.Inactive", "Account is inactive."));

        var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionRepository.GetPermissionNamesForUserAsync(
            user.Id, cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email ?? user.PhoneNumber ?? user.UserName!,
            roles.ToList(),
            permissions);

        var rawRefreshToken = _refreshTokenService.GenerateToken();
        var tokenHash = _refreshTokenService.HashToken(rawRefreshToken);

        var refreshToken = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = _dateTimeProvider.UtcNow.AddDays(7),
            CreatedAt = _dateTimeProvider.UtcNow
        };

        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);

        return Result<LoginResponse>.Success(
            new LoginResponse(accessToken, rawRefreshToken, refreshToken.ExpiresAt, emailConfirmed));
    }
}
