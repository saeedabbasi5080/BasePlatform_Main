namespace BasePlatform.Application.Common.Abstractions;

public interface ISmsService
{
    Task SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);
}
