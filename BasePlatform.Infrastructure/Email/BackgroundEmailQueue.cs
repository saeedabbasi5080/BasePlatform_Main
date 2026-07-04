using System.Threading.Channels;
using BasePlatform.Application.Common.Abstractions;

namespace BasePlatform.Infrastructure.Email;

/// <summary>
/// In-process, unbounded email queue backed by a <see cref="Channel{T}"/>.
/// Registered as a singleton; <see cref="EmailQueueProcessor"/> drains it.
/// </summary>
public sealed class BackgroundEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel =
        Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ChannelReader<EmailMessage> Reader => _channel.Reader;

    public async ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default)
        => await _channel.Writer.WriteAsync(message, cancellationToken);
}
