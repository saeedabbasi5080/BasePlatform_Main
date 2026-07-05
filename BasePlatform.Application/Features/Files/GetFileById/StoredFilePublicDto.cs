namespace BasePlatform.Application.Features.Files.GetFileById;

/// <summary>
/// File metadata exposed via API — excludes internal storage paths.
/// </summary>
public sealed record StoredFilePublicDto(
    Guid Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string StorageProvider,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);
