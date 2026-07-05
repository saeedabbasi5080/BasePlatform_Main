using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Constants;
using BasePlatform.Infrastructure.Persistence.Dapper;
using Dapper;

namespace BasePlatform.Infrastructure.Files;

public sealed class FileAccessService : IFileAccessService
{
    private readonly ICurrentUser _currentUser;
    private readonly IDapperQueryConnection _db;

    public FileAccessService(ICurrentUser currentUser, IDapperQueryConnection db)
    {
        _currentUser = currentUser;
        _db = db;
    }

    public async Task<bool> CanReadAsync(
        Guid fileId,
        Guid uploadedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return false;

        if (_currentUser.HasPermission(Permissions.FilesList))
            return true;

        if (_currentUser.UserId.Value == uploadedByUserId)
            return true;

        // Profile photos are readable by any authenticated user (avatar display).
        var fileIdText = fileId.ToString("D");
        const string sql = """
            SELECT TOP 1 1
            FROM AspNetUsers
            WHERE ProfilePhotoUrl LIKE @Pattern;
            """;

        using var connection = _db.CreateConnection();
        var isProfilePhoto = await connection.ExecuteScalarAsync<int?>(
            sql,
            new { Pattern = $"%{fileIdText}%" });

        return isProfilePhoto == 1;
    }
}
