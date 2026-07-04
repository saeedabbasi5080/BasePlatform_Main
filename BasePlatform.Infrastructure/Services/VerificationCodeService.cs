using System.Security.Cryptography;
using System.Text;
using BasePlatform.Application.Common.Abstractions;

namespace BasePlatform.Infrastructure.Services;

public sealed class VerificationCodeService : IVerificationCodeService
{
    private readonly IEmailVerificationPolicy _policy;

    public VerificationCodeService(IEmailVerificationPolicy policy)
    {
        _policy = policy;
    }

    public string Generate()
    {
        var length = _policy.CodeLength;
        var max = (int)Math.Pow(10, length);
        var number = RandomNumberGenerator.GetInt32(0, max);
        return number.ToString().PadLeft(length, '0');
    }

    public string Hash(Guid userId, string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{userId:N}:{code}"));
        return Convert.ToBase64String(bytes);
    }

    public bool Verify(Guid userId, string code, string codeHash)
    {
        var expected = Hash(userId, code);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(codeHash));
    }
}
