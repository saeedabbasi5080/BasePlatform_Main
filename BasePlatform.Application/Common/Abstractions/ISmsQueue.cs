namespace BasePlatform.Application.Common.Abstractions;

public sealed record SmsMessage(string PhoneNumber, string Body);

public interface ISmsQueue
{
    ValueTask EnqueueAsync(SmsMessage message, CancellationToken cancellationToken = default);
}
