namespace BasePlatform.Application.Common.Abstractions;

/// <summary>Generates and verifies numeric OTP codes. Only hashes are persisted.</summary>
public interface IVerificationCodeService
{
    /// <summary>Generates a cryptographically-random numeric code.</summary>
    string Generate();

    /// <summary>Hashes a code, salted with the owning user id.</summary>
    string Hash(Guid userId, string code);

    /// <summary>Constant-time comparison of a candidate code against a stored hash.</summary>
    bool Verify(Guid userId, string code, string codeHash);
}
