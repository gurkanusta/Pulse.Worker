using Polly;
using Polly.Retry;
using Serilog;
using System.Threading.Channels;
using Pulse.Worker.Email;
using Pulse.Worker.Models;
using Pulse.Worker.Options;
using Pulse.Worker.Queue;
using Pulse.Worker.Workers;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, cfg) =>
    {
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .WriteTo.Console();
    })
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<WorkerOptions>(ctx.Configuration.GetSection("Worker"));
        services.Configure<EmailOptions>(ctx.Configuration.GetSection("Email"));

        
        services.AddSingleton(Channel.CreateUnbounded<EmailJob>());




        services.AddSingleton<
    Pulse.Worker.Queue.IBackgroundQueue<EmailJob>,
    Pulse.Worker.Queue.ChannelBackgroundQueue<EmailJob>
>();


        services.AddSingleton<IEmailSender, FakeEmailSender>();


        services.AddSingleton(System.Threading.Channels.Channel.CreateUnbounded<FailedEmailJob>());
        services.AddSingleton<Pulse.Worker.Queue.IBackgroundQueue<FailedEmailJob>, Pulse.Worker.Queue.ChannelBackgroundQueue<FailedEmailJob>>();
        services.AddHostedService<DeadLetterWorker>();



        services.AddSingleton<AsyncRetryPolicy>(sp =>
    Polly.Policy.Handle<Exception>().WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(attempt),
        onRetry: (ex, ts, attempt, ctx) =>
        {
            
            Log.Warning(ex, "RETRY {Attempt} after {Delay}", attempt, ts);
        }
    )
);


        services.AddHostedService<JobProducerWorker>();
        services.AddHostedService<EmailDispatcherWorker>();
    })
    .Build();

await host.RunAsync();
