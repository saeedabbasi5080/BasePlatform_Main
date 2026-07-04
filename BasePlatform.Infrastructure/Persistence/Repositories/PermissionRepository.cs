using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BasePlatform.Infrastructure.Persistence.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _context;

    public PermissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Permission>> GetByIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceRolePermissionsAsync(
        Guid roleId,
        List<Permission> permissions,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _context.RolePermissions.RemoveRange(existing);

        var newEntries = permissions.Select(p => new RolePermission
        {
            RoleId = roleId,
            PermissionId = p.Id
        });

        await _context.RolePermissions.AddRangeAsync(newEntries, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<string>> GetPermissionNamesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from ur in _context.UserRoles
            join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
            join p in _context.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId
            select p.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}