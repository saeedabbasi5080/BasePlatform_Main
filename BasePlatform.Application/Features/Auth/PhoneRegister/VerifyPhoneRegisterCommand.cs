using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Application.Features.Auth.Login;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Auth.PhoneRegister;

public sealed record VerifyPhoneRegisterCommand(
    string PhoneNumber,
    string Code) : ICommand<Result<LoginResponse>>;
