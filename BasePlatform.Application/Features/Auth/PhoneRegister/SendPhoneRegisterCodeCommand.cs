using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Auth.PhoneRegister;

public sealed record SendPhoneRegisterCodeCommand(
    string PhoneNumber,
    string FirstName,
    string LastName) : ICommand<Result<SendPhoneRegisterCodeResponse>>;

public sealed record SendPhoneRegisterCodeResponse(string Message);
