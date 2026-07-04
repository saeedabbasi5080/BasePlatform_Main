using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Entities;
using BasePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BasePlatform.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationCodeRepository : IEmailVerificationCodeRepository
{
    private readonly AppDbContext _context;

    public EmailVerificationCodeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailVerificationCode code, CancellationToken cancellationToken = default)
        => await _context.EmailVerificationCodes.AddAsync(code, cancellationToken);

    public async Task<EmailVerificationCode?> GetLatestActiveAsync(
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
        => await _context.EmailVerificationCodes
            .Where(c => c.UserId == userId && c.Purpose == purpose && c.ConsumedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task InvalidateActiveAsync(
        Guid userId,
        OtpPurpose purpose,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var active = await _context.EmailVerificationCodes
            .Where(c => c.UserId == userId && c.Purpose == purpose && c.ConsumedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var code in active)
            code.ConsumedAt = now;
    }

    public async Task<DateTimeOffset?> GetLastCreatedAtAsync(
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        var codes = _context.EmailVerificationCodes
            .Where(c => c.UserId == userId && c.Purpose == purpose);

        if (!await codes.AnyAsync(cancellationToken))
            return null;

        return await codes.MaxAsync(c => c.CreatedAt, cancellationToken);
    }

    public async Task<int> CountCreatedSinceAsync(
        Guid userId,
        OtpPurpose purpose,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
        => await _context.EmailVerificationCodes
            .CountAsync(
                c => c.UserId == userId && c.Purpose == purpose && c.CreatedAt >= since,
                cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
