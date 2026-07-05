using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Files.DownloadFile;
using BasePlatform.Application.Features.Files.GetFileById;
using BasePlatform.Infrastructure.Persistence.Dapper;
using BasePlatform.Shared;
using Dapper;

namespace BasePlatform.Infrastructure.Queries.Files;

public sealed class DownloadFileQueryHandler
    : IQueryHandler<DownloadFileQuery, Result<StoredFileDto>>
{
    private readonly IDapperQueryConnection _db;
    private readonly IFileAccessService _fileAccess;

    public DownloadFileQueryHandler(
        IDapperQueryConnection db,
        IFileAccessService fileAccess)
    {
        _db = db;
        _fileAccess = fileAccess;
    }

    public async Task<Result<StoredFileDto>> HandleAsync(
        DownloadFileQuery query,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                FileName,
                OriginalFileName,
                ContentType,
                FileSizeBytes,
                StoragePath,
                StorageProvider,
                UploadedByUserId,
                CreatedAt
            FROM StoredFiles
            WHERE Id = @FileId;
            """;

        using var connection = _db.CreateConnection();
        var file = await connection.QueryFirstOrDefaultAsync<StoredFileDto>(
            sql, new { FileId = query.FileId });

        if (file is null)
        {
            return Result<StoredFileDto>.Failure(
                Error.NotFound("Files.NotFound", "File not found."));
        }

        if (!await _fileAccess.CanReadAsync(
                file.Id, file.UploadedByUserId, cancellationToken))
        {
            return Result<StoredFileDto>.Failure(
                Error.Forbidden("Files.AccessDenied", "You do not have access to this file."));
        }

        return Result<StoredFileDto>.Success(file);
    }
}
