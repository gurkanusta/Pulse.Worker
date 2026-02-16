using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Pulse.Worker.Email;
using Pulse.Worker.Models;
using Pulse.Worker.Queue;
using Pulse.Worker.Workers;

namespace Pulse.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPulseWorker(this IServiceCollection services)
    {
        
        services.AddSingleton(Channel.CreateUnbounded<EmailJob>());
        services.AddSingleton<IBackgroundQueue<EmailJob>, ChannelBackgroundQueue<EmailJob>>();

        
        services.AddSingleton(Channel.CreateUnbounded<FailedEmailJob>());
        services.AddSingleton<IBackgroundQueue<FailedEmailJob>, ChannelBackgroundQueue<FailedEmailJob>>();

        
        services.AddSingleton<IEmailSender, FakeEmailSender>();


        services.AddSingleton<Polly.Retry.AsyncRetryPolicy>(_ =>
    Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(attempt)
        )
);


        services.AddHostedService<JobProducerWorker>();
        services.AddHostedService<EmailDispatcherWorker>();
        services.AddHostedService<DeadLetterWorker>();

        return services;
    }
}