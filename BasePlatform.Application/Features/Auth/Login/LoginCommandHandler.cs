using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Entities;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Auth.Login;

public sealed class LoginCommandHandler
    : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPermissionRepository _permissionRepository;

    public LoginCommandHandler(
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

    public async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null || !user.IsActive)
            return Result<LoginResponse>.Failure(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));

        // Honor the configured lockout policy (brute-force protection).
        if (await _userManager.IsLockedOutAsync(user))
            return Result<LoginResponse>.Failure(
                Error.Unauthorized("Auth.LockedOut", "Account is temporarily locked. Try again later."));

        if (!await _userManager.CheckPasswordAsync(user, command.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return Result<LoginResponse>.Failure(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        // Login is allowed without email confirmation; the flag is surfaced to the client.
        var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionRepository.GetPermissionNamesForUserAsync(user.Id, cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email!,
            roles.ToList(),
            permissions);

        var rawRefreshToken = _refreshTokenService.GenerateToken();
        var tokenHash = _refreshTokenService.HashToken(rawRefreshToken);

        
        var refreshToken = new BasePlatform.Domain.Entities.RefreshToken
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
            new LoginResponse(accessToken, rawRefreshToken, refreshToken.ExpiresAt, emailConfirmed));  // ← جدید
    }
}