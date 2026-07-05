using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Shared;

namespace BasePlatform.Application.Common;

/// <summary>
/// Guards assignment of built-in privileged roles (SuperAdmin, Admin).
/// </summary>
public static class RolePolicy
{
    public const string SuperAdminRole = "SuperAdmin";
    public const string AdminRole = "Admin";

    private static readonly HashSet<string> PrivilegedRoles =
        new(StringComparer.OrdinalIgnoreCase) { SuperAdminRole, AdminRole };

    public static bool IsPrivilegedRole(string roleName) =>
        PrivilegedRoles.Contains(roleName);

    public static bool IsSuperAdmin(ICurrentUser user) =>
        user.Roles.Contains(SuperAdminRole, StringComparer.OrdinalIgnoreCase);

    /// <returns>null when allowed; otherwise a failure result.</returns>
    public static Result? ValidatePrivilegedRoleAssignment(
        ICurrentUser actor,
        string targetRoleName)
    {
        if (!IsPrivilegedRole(targetRoleName))
            return null;

        if (!IsSuperAdmin(actor))
        {
            return Result.Failure(
                Error.Forbidden(
                    "Roles.CannotAssignPrivileged",
                    "Only SuperAdmin can assign Admin or SuperAdmin roles."));
        }

        return null;
    }
}
