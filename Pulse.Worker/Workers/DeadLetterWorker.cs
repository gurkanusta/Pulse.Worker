using Pulse.Worker.Models;
using Pulse.Worker.Queue;

namespace Pulse.Worker.Workers;

public sealed class DeadLetterWorker : BackgroundService
{
    private readonly ILogger<DeadLetterWorker> _logger;
    private readonly IBackgroundQueue<FailedEmailJob> _deadLetterQueue;

    public DeadLetterWorker(
        ILogger<DeadLetterWorker> logger,
        IBackgroundQueue<FailedEmailJob> deadLetterQueue)
    {
        _logger = logger;
        _deadLetterQueue = deadLetterQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogWarning("DeadLetterWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            FailedEmailJob failed;

            try
            {
                failed = await _deadLetterQueue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            _logger.LogWarning(
                "DEAD_LETTER | CorrelationId:{CorrelationId} To:{To} Subject:{Subject} FailedAtUtc:{FailedAtUtc} Error:{Error}",
                failed.Original.CorrelationId,
                failed.Original.To,
                failed.Original.Subject,
                failed.FailedAtUtc,
                failed.Error
            );
        }

        _logger.LogWarning("DeadLetterWorker stopped.");
    }
}
