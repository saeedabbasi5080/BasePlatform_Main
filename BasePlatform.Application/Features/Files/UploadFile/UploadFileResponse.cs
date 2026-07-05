namespace BasePlatform.Application.Features.Files.UploadFile;

public sealed record UploadFileResponse(
    Guid Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string StorageProvider,
    DateTimeOffset CreatedAt);