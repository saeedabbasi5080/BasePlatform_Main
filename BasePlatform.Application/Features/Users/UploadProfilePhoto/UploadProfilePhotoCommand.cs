using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Users.UploadProfilePhoto;

public sealed record UploadProfilePhotoCommand(
    Stream FileStream,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes) : ICommand<Result<UploadProfilePhotoResponse>>;

public sealed record UploadProfilePhotoResponse(string ProfilePhotoUrl);
