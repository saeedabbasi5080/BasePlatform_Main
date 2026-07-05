using BasePlatform.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace BasePlatform.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ProfilePhotoUrl { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public Gender? Gender { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}