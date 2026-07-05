using BasePlatform.Api.Common;
using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Files.DeleteFile;
using BasePlatform.Application.Features.Files.DownloadFile;
using BasePlatform.Application.Features.Files.GetFileById;
using BasePlatform.Application.Features.Files.GetFiles;
using BasePlatform.Application.Features.Files.UploadFile;
using BasePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasePlatform.Api.Controllers;

[Route("api/files")]
[Authorize]
public sealed class FilesController : ApiControllerBase
{
    private readonly IDispatcher _dispatcher;

    public FilesController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    // GET api/files
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

    // POST api/files
    [HttpPost]
    [Authorize(Policy = Permissions.FilesUpload)]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<IActionResult> UploadFile(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ApiErrorResponse("No file provided.", "Files.EmptyFile"));

        await using var stream = file.OpenReadStream();

        var result = await _dispatcher.SendAsync(
            new UploadFileCommand(
                stream,
                file.FileName,
                file.ContentType,
                file.Length),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetFileById), new { id = result.Value!.Id }, result.Value)
            : Problem(result);
    }

    // GET api/files/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFileById(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.QueryAsync(
            new GetFileByIdQuery(id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result);
    }

    // GET api/files/{id}/download
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> DownloadFile(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.QueryAsync(
            new DownloadFileQuery(id), cancellationToken);

        if (!result.IsSuccess)
            return Problem(result);

        var file = result.Value!;

        if (!System.IO.File.Exists(file.StoragePath))
            return NotFound(new ApiErrorResponse("File not found on disk.", "Files.NotFound"));

        var stream = new FileStream(
            file.StoragePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return File(stream, file.ContentType, file.OriginalFileName);
    }

    // DELETE api/files/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.FilesDelete)]
    public async Task<IActionResult> DeleteFile(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(
            new DeleteFileCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : Problem(result);
    }
}