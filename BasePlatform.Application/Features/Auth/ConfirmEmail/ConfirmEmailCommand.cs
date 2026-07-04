using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Auth.ConfirmEmail;

public sealed record ConfirmEmailCommand(
    string Email,
    string Code) : ICommand<Result>;