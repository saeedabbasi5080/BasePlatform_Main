using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Shared;

namespace BasePlatform.Application.Features.Auth.ResendConfirmation;

public sealed record ResendConfirmationCommand(string Email) : ICommand<Result>;
