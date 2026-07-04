using BasePlatform.Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BasePlatform.Infrastructure.Email;

/// <summary>Background worker that delivers queued emails via <see cref="IEmailService"/>.</summary>
public sealed class EmailQueueProcessor : BackgroundService
{
    private readonly BackgroundEmailQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailQueueProcessor> _logger;

    public EmailQueueProcessor(
        BackgroundEmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailQueueProcessor> logger)
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
                // IEmailService is scoped; create a scope per message.
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SendAsync(
                    message.To, message.Subject, message.Body, message.IsHtml, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver queued email to {Recipient}.", message.To);
            }
        }
    }
}
