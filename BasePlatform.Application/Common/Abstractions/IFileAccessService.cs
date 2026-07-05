namespace BasePlatform.Application.Common.Abstractions;

public interface IFileAccessService
{
    /// <summary>
    /// Returns true when the user may read/download the file (owner, files.list, or public profile photo).
    /// </summary>
    Task<bool> CanReadAsync(Guid fileId, Guid uploadedByUserId, CancellationToken cancellationToken = default);
}
