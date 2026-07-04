using BasePlatform.Domain.Entities;

namespace BasePlatform.Application.Common.Abstractions;

public interface IPermissionRepository
{
    Task<List<Permission>> GetByIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default);

    Task ReplaceRolePermissionsAsync(
        Guid roleId,
        List<Permission> permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the distinct permission names granted to a user through their roles.
    /// </summary>
    Task<List<string>> GetPermissionNamesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}