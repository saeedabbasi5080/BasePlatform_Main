using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Files.GetFileById;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Files.GetFiles;

public sealed record GetFilesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IQuery<Result<PaginatedResult<StoredFilePublicDto>>>;

public sealed class GetFilesQueryHandler
    : IQueryHandler<GetFilesQuery, Result<PaginatedResult<StoredFilePublicDto>>>
{
    private readonly IStoredFileRepository _repository;

    public GetFilesQueryHandler(IStoredFileRepository repository) => _repository = repository;

    public async Task<Result<PaginatedResult<StoredFilePublicDto>>> HandleAsync(
        GetFilesQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = PaginationLimits.NormalizePage(query.Page);
        var pageSize = PaginationLimits.NormalizePageSize(query.PageSize);

        var result = await _repository.GetPagedAsync(page, pageSize, query.Search, cancellationToken);

        var items = result.Items.Select(f => new StoredFilePublicDto(
            f.Id,
            f.FileName,
            f.OriginalFileName,
            f.ContentType,
            f.FileSizeBytes,
            f.StorageProvider,
            f.UploadedByUserId,
            f.CreatedAt)).ToList();

        return Result<PaginatedResult<StoredFilePublicDto>>.Success(
            new PaginatedResult<StoredFilePublicDto>(items, result.PageNumber, result.PageSize, result.TotalCount));
    }
}
