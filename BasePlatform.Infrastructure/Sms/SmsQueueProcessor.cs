using BasePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BasePlatform.Infrastructure.Sms;

public sealed class SmsQueueProcessor : BackgroundService
{
    private readonly BackgroundSmsQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SmsQueueProcessor> _logger;

    public SmsQueueProcessor(
        BackgroundSmsQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<SmsQueueProcessor> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();
                await smsService.SendAsync(message.PhoneNumber, message.Body, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver queued SMS to {Phone}.", message.PhoneNumber);
            }
        }
    }
}
