using Microsoft.Extensions.Options;
using Pulse.Worker.Models;
using Pulse.Worker.Options;
using Pulse.Worker.Queue;

namespace Pulse.Worker.Workers;

public class JobProducerWorker(
    ILogger<JobProducerWorker> logger,
    IOptions<WorkerOptions> options,
    IBackgroundQueue<EmailJob> queue
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, options.Value.ProducerIntervalSeconds));
        var i = 0;

        logger.LogInformation("Producer started. Interval: {Seconds}s", interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            i++;

            var job = new EmailJob(
                To: "test@user.com",
                Subject: $"Welcome #{i}",
                Body: "Your account is ready.",
                CreatedAtUtc: DateTime.UtcNow,
                CorrelationId: Guid.NewGuid()
            );

            await queue.EnqueueAsync(job, stoppingToken);
            logger.LogInformation("JOB_ENQUEUED | CorrelationId:{CorrelationId} To:{To}", job.CorrelationId, job.To);

            await Task.Delay(interval, stoppingToken);
        }
    }


}
