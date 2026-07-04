using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Audit.GetAuditLogs;
using BasePlatform.Application.Features.Auth.ConfirmEmail;
using BasePlatform.Application.Features.Auth.ForgotPassword;
using BasePlatform.Application.Features.Auth.Login;
using BasePlatform.Application.Features.Auth.Logout;
using BasePlatform.Application.Features.Auth.Common;
using BasePlatform.Application.Features.Auth.RefreshToken;
using BasePlatform.Application.Features.Auth.Register;
using BasePlatform.Application.Features.Auth.ResendConfirmation;
using BasePlatform.Application.Features.Auth.PhoneLogin;
using BasePlatform.Application.Features.Auth.PhoneRegister;
using BasePlatform.Application.Features.Auth.ResetPassword;
using BasePlatform.Application.Features.Auth.VerifyResetCode;
using BasePlatform.Application.Features.Files.GetFiles;
using BasePlatform.Application.Features.Files.DeleteFile;
using BasePlatform.Application.Features.Files.GetFileById;
using BasePlatform.Application.Features.Files.UploadFile;
using BasePlatform.Application.Features.Permissions.AssignPermissionsToRole;
using BasePlatform.Application.Features.Permissions.GetAllPermissions;
using BasePlatform.Application.Features.Permissions.GetRolePermissions;
using BasePlatform.Application.Features.Roles.CreateRole;
using BasePlatform.Application.Features.Roles.DeleteRole;
using BasePlatform.Application.Features.Roles.GetAllRoles;
using BasePlatform.Application.Features.Roles.GetRoleById;
using BasePlatform.Application.Features.Roles.UpdateRole;
using BasePlatform.Application.Features.Settings.GetSettingByKey;
using BasePlatform.Application.Features.Settings.GetSettings;
using BasePlatform.Application.Features.Settings.UpsertSetting;
using BasePlatform.Application.Features.Users.AssignRole;
using BasePlatform.Application.Features.Users.ChangePassword;
using BasePlatform.Application.Features.Users.DeactivateUser;
using BasePlatform.Application.Features.Users.GetAllUsers;
using BasePlatform.Application.Features.Users.GetCurrentUser;
using BasePlatform.Application.Features.Users.GetUserById;
using BasePlatform.Application.Features.Users.UpdateProfile;
using BasePlatform.Infrastructure.Authentication;
using BasePlatform.Infrastructure.Authorization;
using BasePlatform.Infrastructure.Dispatcher;
using BasePlatform.Infrastructure.Email;
using BasePlatform.Infrastructure.Identity;
using BasePlatform.Infrastructure.Persistence;
using BasePlatform.Infrastructure.Persistence.Dapper;
using BasePlatform.Infrastructure.Persistence.Repositories;
using BasePlatform.Infrastructure.Queries.Audit;
using BasePlatform.Infrastructure.Queries.Files;
using BasePlatform.Infrastructure.Queries.Permissions;
using BasePlatform.Infrastructure.Queries.Roles;
using BasePlatform.Infrastructure.Queries.Settings;
using BasePlatform.Infrastructure.Queries.Users;
using BasePlatform.Infrastructure.Services;
using BasePlatform.Infrastructure.Sms;
using BasePlatform.Infrastructure.Storage;
using BasePlatform.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BasePlatform.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IDapperQueryConnection, DapperQueryConnection>();

        // Identity
        services.AddIdentityServices();
        services.AddScoped<IdentitySeeder>();

        // Authorization
        services.AddScoped<IUserClaimsPrincipalFactory<BasePlatform.Domain.Entities.AppUser>,
            PermissionClaimsPrincipalFactory>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Email
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<EmailVerificationOptions>(
            configuration.GetSection(EmailVerificationOptions.SectionName));
        services.Configure<SmsOptions>(configuration.GetSection(SmsOptions.SectionName));

        // JWT & Token
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // Services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ISmsOtpPolicy, SmsOtpPolicy>();
        services.AddScoped<KavenegarSmsService>();
        services.AddScoped<ConsoleSmsService>();
        services.AddScoped<ISmsService>(sp =>
        {
            var provider = sp.GetRequiredService<IOptions<SmsOptions>>().Value.Provider;
            return string.Equals(provider, "Kavenegar", StringComparison.OrdinalIgnoreCase)
                ? sp.GetRequiredService<KavenegarSmsService>()
                : sp.GetRequiredService<ConsoleSmsService>();
        });
        services.AddSingleton<IDevOtpNotifier, DevOtpNotifier>();
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddSingleton<IEmailVerificationPolicy, EmailVerificationPolicyProvider>();
        services.AddSingleton<IVerificationCodeService, VerificationCodeService>();

        // Async email delivery (background worker)
        services.AddSingleton<BackgroundEmailQueue>();
        services.AddSingleton<IEmailQueue>(sp => sp.GetRequiredService<BackgroundEmailQueue>());
        services.AddHostedService<EmailQueueProcessor>();

        // Async SMS delivery (background worker)
        services.AddSingleton<BackgroundSmsQueue>();
        services.AddSingleton<ISmsQueue>(sp => sp.GetRequiredService<BackgroundSmsQueue>());
        services.AddHostedService<SmsQueueProcessor>();

        // Email verification (OTP)
        services.AddScoped<OtpCodeIssuer>();
        services.AddScoped<PhoneOtpIssuer>();
        services.AddScoped<AuthTokenFactory>();

        // Repositories
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddScoped<IEmailVerificationCodeRepository, EmailVerificationCodeRepository>();

        // Dispatcher
        services.AddScoped<IDispatcher, BasePlatform.Infrastructure.Dispatcher.Dispatcher>();

        // Auth Handlers
        services.AddScoped<ICommandHandler<RegisterCommand, Result<RegisterResponse>>, RegisterCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, Result<LoginResponse>>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<LogoutCommand, Result>, LogoutCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>, RefreshTokenCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmEmailCommand, Result>, ConfirmEmailCommandHandler>();
        services.AddScoped<ICommandHandler<ResendConfirmationCommand, Result>, ResendConfirmationCommandHandler>();
        services.AddScoped<ICommandHandler<ForgotPasswordCommand, Result>, ForgotPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<ResetPasswordCommand, Result>, ResetPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<VerifyResetCodeCommand, Result>, VerifyResetCodeCommandHandler>();
        services.AddScoped<ICommandHandler<SendPhoneRegisterCodeCommand, Result<SendPhoneRegisterCodeResponse>>, SendPhoneRegisterCodeCommandHandler>();
        services.AddScoped<ICommandHandler<VerifyPhoneRegisterCommand, Result<LoginResponse>>, VerifyPhoneRegisterCommandHandler>();
        services.AddScoped<ICommandHandler<SendPhoneLoginCodeCommand, Result<SendPhoneLoginCodeResponse>>, SendPhoneLoginCodeCommandHandler>();
        services.AddScoped<ICommandHandler<VerifyPhoneLoginCommand, Result<LoginResponse>>, VerifyPhoneLoginCommandHandler>();

        // Users Handlers
        services.AddScoped<IQueryHandler<GetCurrentUserQuery, Result<UserProfileResponse>>, GetCurrentUserQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserByIdQuery, Result<UserProfileResponse>>, GetUserByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllUsersQuery, Result<PaginatedResult<UserSummaryDto>>>, GetAllUsersQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateProfileCommand, Result>, UpdateProfileCommandHandler>();
        services.AddScoped<ICommandHandler<ChangePasswordCommand, Result>, ChangePasswordCommandHandler>();
        services.AddScoped<ICommandHandler<DeactivateUserCommand, Result>, DeactivateUserCommandHandler>();
        services.AddScoped<ICommandHandler<AssignRoleCommand, Result>, AssignRoleCommandHandler>();

        // Roles Handlers
        services.AddScoped<IQueryHandler<GetAllRolesQuery, Result<List<RoleSummaryDto>>>, GetAllRolesQueryHandler>();
        services.AddScoped<IQueryHandler<GetRoleByIdQuery, Result<RoleDetailResponse>>, GetRoleByIdQueryHandler>();
        services.AddScoped<ICommandHandler<CreateRoleCommand, Result<Guid>>, CreateRoleCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateRoleCommand, Result>, UpdateRoleCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteRoleCommand, Result>, DeleteRoleCommandHandler>();

        // Permissions Handlers
        services.AddScoped<IQueryHandler<GetAllPermissionsQuery, Result<List<PermissionDto>>>, GetAllPermissionsQueryHandler>();
        services.AddScoped<IQueryHandler<GetRolePermissionsQuery, Result<List<PermissionDto>>>, GetRolePermissionsQueryHandler>();
        services.AddScoped<ICommandHandler<AssignPermissionsToRoleCommand, Result>, AssignPermissionsToRoleCommandHandler>();

        // Settings Handlers 
        services.AddScoped<IQueryHandler<GetSettingsQuery, Result<List<AppSettingDto>>>, GetSettingsQueryHandler>();
        services.AddScoped<IQueryHandler<GetSettingByKeyQuery, Result<AppSettingDto>>, GetSettingByKeyQueryHandler>();
        services.AddScoped<ICommandHandler<UpsertSettingCommand, Result>, UpsertSettingCommandHandler>();

        // Files Query Handlers
        services.AddScoped<IQueryHandler<GetFileByIdQuery, Result<StoredFileDto>>, GetFileByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetFilesQuery, Result<PaginatedResult<StoredFileDto>>>, GetFilesQueryHandler>();

        // Files Command Handlers
        services.AddScoped<ICommandHandler<UploadFileCommand, Result<UploadFileResponse>>, UploadFileCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteFileCommand, Result>, DeleteFileCommandHandler>();

        // Audit Query Handler
        services.AddScoped<IQueryHandler<GetAuditLogsQuery, Result<PaginatedResult<AuditLogDto>>>, GetAuditLogsQueryHandler>();

        // FluentValidation validators (executed by the Dispatcher before each command handler)
        services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<IValidator<ConfirmEmailCommand>, ConfirmEmailCommandValidator>();
        services.AddScoped<IValidator<ResendConfirmationCommand>, ResendConfirmationCommandValidator>();
        services.AddScoped<IValidator<ResetPasswordCommand>, ResetPasswordCommandValidator>();
        services.AddScoped<IValidator<VerifyResetCodeCommand>, VerifyResetCodeCommandValidator>();
        services.AddScoped<IValidator<ForgotPasswordCommand>, ForgotPasswordCommandValidator>();
        services.AddScoped<IValidator<SendPhoneRegisterCodeCommand>, SendPhoneRegisterCodeCommandValidator>();
        services.AddScoped<IValidator<VerifyPhoneRegisterCommand>, VerifyPhoneRegisterCommandValidator>();
        services.AddScoped<IValidator<SendPhoneLoginCodeCommand>, SendPhoneLoginCodeCommandValidator>();
        services.AddScoped<IValidator<VerifyPhoneLoginCommand>, VerifyPhoneLoginCommandValidator>();
        services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();
        services.AddScoped<IValidator<AssignRoleCommand>, AssignRoleCommandValidator>();
        services.AddScoped<IValidator<CreateRoleCommand>, CreateRoleCommandValidator>();
        services.AddScoped<IValidator<UpdateRoleCommand>, UpdateRoleCommandValidator>();
        services.AddScoped<IValidator<UpsertSettingCommand>, UpsertSettingCommandValidator>();

        return services;
    }
}
