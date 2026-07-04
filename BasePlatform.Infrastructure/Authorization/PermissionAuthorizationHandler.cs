using Microsoft.AspNetCore.Authorization;

namespace BasePlatform.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private const string SuperAdminRole = "SuperAdmin";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // SuperAdmin implicitly holds every permission.
        if (context.User.IsInRole(SuperAdminRole))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var hasPermission = context.User.Claims.Any(c =>
            c.Type == "permission" &&
            string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}