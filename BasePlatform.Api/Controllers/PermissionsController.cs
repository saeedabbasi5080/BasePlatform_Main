using BasePlatform.Api.Common;
using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Permissions.AssignPermissionsToRole;
using BasePlatform.Application.Features.Permissions.GetAllPermissions;
using BasePlatform.Application.Features.Permissions.GetRolePermissions;
using BasePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasePlatform.Api.Controllers;

[Route("api/permissions")]
[Authorize]
public sealed class PermissionsController : ApiControllerBase
{
    private readonly IDispatcher _dispatcher;

    public PermissionsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    // GET api/permissions
    [HttpGet]
    [Authorize(Policy = Permissions.PermissionsView)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken)
    {
        var result = await _dispatcher.QueryAsync(
            new GetAllPermissionsQuery(), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    // GET api/permissions/roles/{roleId}
    [HttpGet("roles/{roleId:guid}")]
    [Authorize(Policy = Permissions.PermissionsView)]
    public async Task<IActionResult> GetRolePermissions(
        Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.QueryAsync(
            new GetRolePermissionsQuery(roleId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    // PUT api/permissions/roles/{roleId}
    [HttpPut("roles/{roleId:guid}")]
    [Authorize(Policy = Permissions.PermissionsManage)]
    public async Task<IActionResult> AssignPermissionsToRole(
        Guid roleId,
        [FromBody] AssignPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(
            new AssignPermissionsToRoleCommand(roleId, request.PermissionIds),
            cancellationToken);

        return result.IsSuccess ? NoContent() : Problem(result);
    }
}

public sealed record AssignPermissionsRequest(List<Guid> PermissionIds);