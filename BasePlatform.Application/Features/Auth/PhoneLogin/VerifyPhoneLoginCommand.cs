using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Login;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Auth.PhoneLogin;

public sealed record VerifyPhoneLoginCommand(string PhoneNumber, string Code)
    : ICommand<Result<LoginResponse>>;
