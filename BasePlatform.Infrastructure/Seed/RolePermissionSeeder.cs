using BasePlatform.Domain.Constants;
using BasePlatform.Domain.Entities;
using BasePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BasePlatform.Infrastructure.Seed;

/// <summary>
/// Seeds the role → permission mappings. Without this the seeded SuperAdmin/Admin
/// roles have no permissions, which makes admin-panel login (admin.access) and every
/// permission-protected endpoint fail.
/// </summary>
public static class RolePermissionSeeder
{
    private static readonly IReadOnlyList<string> AdminPermissions =
    [
        Permissions.AdminAccess,
        Permissions.UsersView, Permissions.UsersCreate, Permissions.UsersEdit, Permissions.UsersDelete,
        Permissions.RolesView, Permissions.RolesAssign,
        Permissions.PermissionsView,
        Permissions.SettingsView, Permissions.SettingsUpdate,
        Permissions.FilesUpload, Permissions.FilesDelete, Permissions.FilesList,
        Permissions.AuditView
    ];

    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        var roles = await context.Roles
            .Where(r => r.Name == "SuperAdmin" || r.Name == "Admin")
            .ToListAsync();

        if (roles.Count == 0)
            return;

        var permissions = await context.Permissions.ToListAsync();
        var permissionsByName = permissions.ToDictionary(p => p.Name, p => p.Id);

        var existing = await context.RolePermissions
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync();
        var existingSet = existing
            .Select(e => (e.RoleId, e.PermissionId))
            .ToHashSet();

        foreach (var role in roles)
        {
            var grantedNames = role.Name switch
            {
                "SuperAdmin" => Permissions.All,
                _ => AdminPermissions
            };

            foreach (var permissionName in grantedNames)
            {
                if (!permissionsByName.TryGetValue(permissionName, out var permissionId))
                    continue;

                if (existingSet.Contains((role.Id, permissionId)))
                    continue;

                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });

                logger.LogInformation(
                    "Seeded permission '{Permission}' for role '{Role}'.",
                    permissionName, role.Name);
            }
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicateError(ex))
        {
            logger.LogWarning("Some role-permissions already seeded by another process. Skipping.");
            context.ChangeTracker.Clear();
        }
    }

    private static bool IsDuplicateError(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
        ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true ||
        ex.InnerException?.Message.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) == true;
}
