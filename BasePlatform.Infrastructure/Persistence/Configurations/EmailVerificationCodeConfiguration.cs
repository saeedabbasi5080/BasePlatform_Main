using BasePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasePlatform.Infrastructure.Persistence.Configurations;

public class EmailVerificationCodeConfiguration : IEntityTypeConfiguration<EmailVerificationCode>
{
    public void Configure(EntityTypeBuilder<EmailVerificationCode> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CodeHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.ExpiresAt).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.Purpose).IsRequired();

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.UserId, c.Purpose, c.ConsumedAt });
        builder.HasIndex(c => c.CreatedAt);
    }
}
