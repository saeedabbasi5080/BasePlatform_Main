using BasePlatform.Api.Common;
using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Settings.GetSettingByKey;
using BasePlatform.Application.Features.Settings.GetSettings;
using BasePlatform.Application.Features.Settings.UpsertSetting;
using BasePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasePlatform.Api.Controllers;

[Route("api/settings")]
public sealed class SettingsController : ApiControllerBase
{
    private readonly IDispatcher _dispatcher;

    public SettingsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    // GET api/settings/public
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicSettings(
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.QueryAsync(
            new GetSettingsQuery(PublicOnly: true), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    // GET api/settings/{key}
    [HttpGet("{key}")]
    [Authorize]
    [Authorize(Policy = Permissions.SettingsView)]
    public async Task<IActionResult> GetSettingByKey(
        string key, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.QueryAsync(
            new GetSettingByKeyQuery(key), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    // PUT api/settings
    [HttpPut]
    [Authorize(Policy = Permissions.SettingsUpdate)]
    public async Task<IActionResult> UpsertSetting(
        [FromBody] UpsertSettingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(command, cancellationToken);

        return result.IsSuccess ? NoContent() : Problem(result);
    }
}