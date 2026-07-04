using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Auth.PhoneLogin;

public sealed record SendPhoneLoginCodeCommand(string PhoneNumber)
    : ICommand<Result<SendPhoneLoginCodeResponse>>;

public sealed record SendPhoneLoginCodeResponse(string Message);
