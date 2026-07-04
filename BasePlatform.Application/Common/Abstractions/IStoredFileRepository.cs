using BasePlatform.Domain.Entities;
using BasePlatform.Shared;

namespace BasePlatform.Application.Common.Abstractions;

public interface IStoredFileRepository
{
    Task<StoredFile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PaginatedResult<StoredFile>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StoredFile file,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        StoredFile file,
        CancellationToken cancellationToken = default);
}
