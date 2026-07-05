using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Domain.Entities;
using BasePlatform.Shared;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Application.Features.Users.UploadProfilePhoto;

public sealed class UploadProfilePhotoCommandHandler
    : ICommandHandler<UploadProfilePhotoCommand, Result<UploadProfilePhotoResponse>>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private const long MaxFileSizeBytes = 5L * 1024 * 1024; // 5 MB

    private readonly IStorageService _storageService;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UploadProfilePhotoCommandHandler(
        IStorageService storageService,
        IStoredFileRepository storedFileRepository,
        UserManager<AppUser> userManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _storageService = storageService;
        _storedFileRepository = storedFileRepository;
        _userManager = userManager;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<UploadProfilePhotoResponse>> HandleAsync(
        UploadProfilePhotoCommand command,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return Result<UploadProfilePhotoResponse>.Failure(
                Error.Unauthorized("Users.Unauthenticated", "User is not authenticated."));

        if (command.FileStream is null || command.FileStream.Length == 0)
            return Result<UploadProfilePhotoResponse>.Failure(
                Error.Validation("ProfilePhoto.EmptyFile", "File stream is empty."));

        if (command.FileSizeBytes > MaxFileSizeBytes)
            return Result<UploadProfilePhotoResponse>.Failure(
                Error.Validation("ProfilePhoto.TooLarge", "Profile photo must be 5 MB or smaller."));

        var extension = Path.GetExtension(command.OriginalFileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            return Result<UploadProfilePhotoResponse>.Failure(
                Error.Validation("ProfilePhoto.InvalidType", "Only JPG, PNG, WEBP, and GIF images are allowed."));

        var user = await _userManager.FindByIdAsync(_currentUser.UserId.Value.ToString());
        if (user is null || !user.IsActive)
            return Result<UploadProfilePhotoResponse>.Failure(
                Error.NotFound("Users.NotFound", "User not found."));

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        string storagePath;
        try
        {
            storagePath = await _storageService.UploadAsync(
                command.FileStream,
                uniqueFileName,
                command.ContentType,
                cancellationToken);
        }
        catch (Exception)
        {
            return Result<UploadProfilePhotoResponse>.Failure(
                Error.Failure("ProfilePhoto.UploadFailed", "Profile photo upload failed."));
        }

        var storedFile = new StoredFile
        {
            Id = Guid.NewGuid(),
            FileName = uniqueFileName,
            OriginalFileName = command.OriginalFileName,
            ContentType = command.ContentType,
            FileSizeBytes = command.FileSizeBytes,
            StoragePath = storagePath,
            StorageProvider = "local",
            UploadedByUserId = user.Id,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        await _storedFileRepository.AddAsync(storedFile, cancellationToken);

        var profilePhotoUrl = $"/api/files/{storedFile.Id}/download";
        user.ProfilePhotoUrl = profilePhotoUrl;
        user.UpdatedAt = _dateTimeProvider.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errorMessage = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            return Result<UploadProfilePhotoResponse>.Failure(
                Error.Validation("ProfilePhoto.UpdateFailed", errorMessage));
        }

        return Result<UploadProfilePhotoResponse>.Success(
            new UploadProfilePhotoResponse(profilePhotoUrl));
    }
}
