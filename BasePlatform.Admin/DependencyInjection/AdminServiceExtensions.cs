using BasePlatform.Admin.Configuration;
using BasePlatform.Domain.Constants;
using BasePlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

namespace BasePlatform.Admin.DependencyInjection;

public static class AdminServiceExtensions
{
    private static readonly string[] DefaultFrontendOrigins =
    [
        "http://localhost:3000",
        "https://localhost:3000",
        "http://localhost:5173",
        "https://localhost:5173"
    ];

    public static IServiceCollection AddAdminServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddControllers();

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BasePlatform Admin API",
                Version = "v1",
                Description = "BasePlatform Admin Panel API (Cookie Auth)"
            });
        });

        // Cookie Authentication
        services
            .AddAuthentication(AdminCookieDefaults.AuthenticationScheme)
            .AddCookie(AdminCookieDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "BasePlatform.Admin";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                // Flutter web dev runs on random localhost ports (often http) while Admin is https.
                options.Cookie.SameSite = environment.IsDevelopment()
                    ? SameSiteMode.None
                    : SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                };
            });

        // Authorization Policies
        services.AddAuthorization(options =>
        {
            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        // CORS — cookie auth requires credentials, so origins MUST be an explicit allow-list
        // (reflecting any origin together with AllowCredentials is a security hole).
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? DefaultFrontendOrigins;

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.SetIsOriginAllowed(static origin =>
                    {
                        if (string.IsNullOrWhiteSpace(origin))
                            return false;

                        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                            return false;

                        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                               || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
                    });
                }
                else
                {
                    policy.WithOrigins(allowedOrigins);
                }

                policy.AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }
}