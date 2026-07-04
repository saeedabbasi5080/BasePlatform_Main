using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Auth.VerifyResetCode;

public sealed record VerifyResetCodeCommand(
    string Email,
    string Code) : ICommand<Result>;
