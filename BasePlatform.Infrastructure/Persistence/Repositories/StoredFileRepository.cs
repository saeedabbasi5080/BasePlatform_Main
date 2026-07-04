using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Entities;
using BasePlatform.Shared;
using Microsoft.EntityFrameworkCore;

namespace BasePlatform.Infrastructure.Persistence.Repositories;

public sealed class StoredFileRepository : IStoredFileRepository
{
    private readonly AppDbContext _context;

    public StoredFileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StoredFile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<PaginatedResult<StoredFile>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _context.StoredFiles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(f =>
                f.OriginalFileName.Contains(search) ||
                f.FileName.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<StoredFile>(items, page, pageSize, totalCount);
    }

    public async Task AddAsync(
        StoredFile file,
        CancellationToken cancellationToken = default)
    {
        await _context.StoredFiles.AddAsync(file, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        StoredFile file,
        CancellationToken cancellationToken = default)
    {
        _context.StoredFiles.Remove(file);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
