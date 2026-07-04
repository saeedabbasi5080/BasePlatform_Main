using System.Threading.Channels;
using BasePlatform.Application.Common.Abstractions;

namespace BasePlatform.Infrastructure.Sms;

public sealed class BackgroundSmsQueue : ISmsQueue
{
    private readonly Channel<SmsMessage> _channel =
        Channel.CreateUnbounded<SmsMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ChannelReader<SmsMessage> Reader => _channel.Reader;

    public async ValueTask EnqueueAsync(SmsMessage message, CancellationToken cancellationToken = default)
        => await _channel.Writer.WriteAsync(message, cancellationToken);
}
