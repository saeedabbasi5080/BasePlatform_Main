using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Files.GetFileById;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Files.GetFiles;

public sealed record GetFilesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IQuery<Result<PaginatedResult<StoredFileDto>>>;

public sealed class GetFilesQueryHandler
    : IQueryHandler<GetFilesQuery, Result<PaginatedResult<StoredFileDto>>>
{
    private readonly IStoredFileRepository _repository;

    public GetFilesQueryHandler(IStoredFileRepository repository) => _repository = repository;

    public async Task<Result<PaginatedResult<StoredFileDto>>> HandleAsync(
        GetFilesQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var result = await _repository.GetPagedAsync(page, pageSize, query.Search, cancellationToken);

        var items = result.Items.Select(f => new StoredFileDto(
            f.Id,
            f.FileName,
            f.OriginalFileName,
            f.ContentType,
            f.FileSizeBytes,
            f.StoragePath,
            f.StorageProvider,
            f.UploadedByUserId,
            f.CreatedAt)).ToList();

        return Result<PaginatedResult<StoredFileDto>>.Success(
            new PaginatedResult<StoredFileDto>(items, result.PageNumber, result.PageSize, result.TotalCount));
    }
}
