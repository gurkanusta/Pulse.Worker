using Microsoft.Extensions.Options;
using Polly.Retry;
using Pulse.Worker.Email;
using Pulse.Worker.Models;
using Pulse.Worker.Options;
using Pulse.Worker.Queue;

namespace Pulse.Worker.Workers;

public sealed class EmailDispatcherWorker : BackgroundService
{
    private readonly ILogger<EmailDispatcherWorker> _logger;
    private readonly IOptions<EmailOptions> _emailOptions;
    private readonly IBackgroundQueue<EmailJob> _queue;
    private readonly IBackgroundQueue<FailedEmailJob> _deadLetterQueue;
    private readonly IEmailSender _emailSender;
    private readonly AsyncRetryPolicy _retryPolicy;

    public EmailDispatcherWorker(
        ILogger<EmailDispatcherWorker> logger,
        IOptions<EmailOptions> emailOptions,
        IBackgroundQueue<EmailJob> queue,
        IBackgroundQueue<FailedEmailJob> deadLetterQueue,
        IEmailSender emailSender,
        AsyncRetryPolicy retryPolicy)
    {
        _logger = logger;
        _emailOptions = emailOptions;
        _queue = queue;
        _deadLetterQueue = deadLetterQueue;
        _emailSender = emailSender;
        _retryPolicy = retryPolicy;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dispatcher started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            EmailJob job;

            try
            {
                job = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await ProcessJobAsync(job, stoppingToken);
        }

        _logger.LogInformation("Dispatcher stopped.");
    }

    private async Task ProcessJobAsync(EmailJob job, CancellationToken ct)
    {
        var from = _emailOptions.Value.From;

        _logger.LogInformation(
            "JOB_RECEIVED | CorrelationId:{CorrelationId} To:{To} Subject:{Subject} CreatedAtUtc:{CreatedAtUtc}",
            job.CorrelationId, job.To, job.Subject, job.CreatedAtUtc
        );

        try
        {
           
            await _retryPolicy.ExecuteAsync(async () =>
            {
                ct.ThrowIfCancellationRequested();

                _logger.LogInformation(
                    "EMAIL_SENDING | CorrelationId:{CorrelationId} To:{To} Subject:{Subject}",
                    job.CorrelationId, job.To, job.Subject
                );

                await _emailSender.SendAsync(
                    from: from,
                    to: job.To,
                    subject: job.Subject,
                    body: job.Body,
                    correlationId: job.CorrelationId,
                    ct: ct
                );
            });

            _logger.LogInformation("JOB_DONE | CorrelationId:{CorrelationId}", job.CorrelationId);
        }
        catch (OperationCanceledException)
        {
            
            _logger.LogWarning("JOB_CANCELED | CorrelationId:{CorrelationId}", job.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JOB_FAILED | CorrelationId:{CorrelationId}", job.CorrelationId);

            
            var failed = new FailedEmailJob(
                Original: job,
                Error: ex.Message,
                FailedAtUtc: DateTime.UtcNow
            );

            try
            {
                await _deadLetterQueue.EnqueueAsync(failed, ct);
                _logger.LogWarning("JOB_DEAD_LETTERED | CorrelationId:{CorrelationId}", job.CorrelationId);
            }
            catch (Exception dlqEx)
            {
                _logger.LogError(dlqEx, "DEAD_LETTER_ENQUEUE_FAILED | CorrelationId:{CorrelationId}", job.CorrelationId);
            }
        }
    }
}
