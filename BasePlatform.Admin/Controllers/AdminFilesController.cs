using BasePlatform.Admin.Configuration;
using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Files.DeleteFile;
using BasePlatform.Application.Features.Files.GetFileById;
using BasePlatform.Application.Features.Files.GetFiles;
using BasePlatform.Application.Features.Files.UploadFile;
using BasePlatform.Domain.Constants;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasePlatform.Admin.Controllers;

[ApiController]
[Route("admin/files")]
[Authorize(AuthenticationSchemes = AdminCookieDefaults.AuthenticationScheme)]
public sealed class AdminFilesController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public AdminFilesController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpGet]
    [Authorize(Policy = Permissions.FilesList)]
    public async Task<IActionResult> GetFiles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _dispatcher.QueryAsync(
            new GetFilesQuery(page, pageSize, search), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.FilesList)]
    public async Task<IActionResult> GetFileById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.QueryAsync(new GetFileByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.FilesUpload)]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { Code = "Files.EmptyFile", Description = "No file provided." });

        await using var stream = file.OpenReadStream();
        var result = await _dispatcher.SendAsync(
            new UploadFileCommand(stream, file.FileName, file.ContentType, file.Length),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetFileById), new { id = result.Value!.Id }, result.Value)
            : Problem(result);
    }

    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = Permissions.FilesList)]
    public async Task<IActionResult> DownloadFile(Guid id, CancellationToken cancellationToken)
    {
        var metaResult = await _dispatcher.QueryAsync(new GetFileByIdQuery(id), cancellationToken);
        if (!metaResult.IsSuccess)
            return Problem(metaResult);

        var file = metaResult.Value!;
        if (!System.IO.File.Exists(file.StoragePath))
            return NotFound(new { Code = "Files.NotFound", Description = "File not found on disk." });

        var stream = new FileStream(
            file.StoragePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return File(stream, file.ContentType, file.OriginalFileName);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.FilesDelete)]
    public async Task<IActionResult> DeleteFile(Guid id, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(new DeleteFileCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : Problem(result);
    }

    private IActionResult Problem(Result result) =>
        result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(new { result.Error.Code, result.Error.Description }),
            ErrorType.Unauthorized => Unauthorized(new { result.Error.Code, result.Error.Description }),
            ErrorType.Forbidden => StatusCode(403, new { result.Error.Code, result.Error.Description }),
            ErrorType.Validation => BadRequest(new { result.Error.Code, result.Error.Description }),
            ErrorType.Conflict => Conflict(new { result.Error.Code, result.Error.Description }),
            _ => StatusCode(500, new { result.Error.Code, result.Error.Description })
        };

    private IActionResult Problem<T>(Result<T> result) =>
        result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(new { result.Error.Code, result.Error.Description }),
            ErrorType.Unauthorized => Unauthorized(new { result.Error.Code, result.Error.Description }),
            ErrorType.Forbidden => StatusCode(403, new { result.Error.Code, result.Error.Description }),
            ErrorType.Validation => BadRequest(new { result.Error.Code, result.Error.Description }),
            ErrorType.Conflict => Conflict(new { result.Error.Code, result.Error.Description }),
            _ => StatusCode(500, new { result.Error.Code, result.Error.Description })
        };
}
