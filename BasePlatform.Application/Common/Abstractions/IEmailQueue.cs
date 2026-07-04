namespace BasePlatform.Application.Common.Abstractions;

/// <summary>A message to be delivered asynchronously by the background email worker.</summary>
public sealed record EmailMessage(string To, string Subject, string Body, bool IsHtml = true);

/// <summary>
/// Enqueues emails for asynchronous delivery so request handlers (e.g. register) don't
/// block on SMTP.
/// </summary>
public interface IEmailQueue
{
    ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
