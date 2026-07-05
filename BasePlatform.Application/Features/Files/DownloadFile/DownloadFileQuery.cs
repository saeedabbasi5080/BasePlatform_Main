using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Files.GetFileById;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Files.DownloadFile;

/// <summary>
/// Internal query for streaming a file — includes storage path after access check.
/// </summary>
public sealed record DownloadFileQuery(Guid FileId)
    : IQuery<Result<StoredFileDto>>;
