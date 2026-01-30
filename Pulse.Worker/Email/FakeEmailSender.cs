
using Microsoft.Extensions.Options;
using Pulse.Worker.Options;



namespace Pulse.Worker.Email;

public class FakeEmailSender(
    ILogger<FakeEmailSender> logger,
    IOptions<EmailOptions> emailOptions
    ) : IEmailSender
{
    public async Task SendAsync(string from, string to, string subject, string body, Guid correlationId, CancellationToken ct)
    {
        var delay = emailOptions.Value.SimulatedLatencyMs;
        await Task.Delay(delay, ct);

        logger.LogInformation(
             "EMAIL_SENT | CorrelationId:{CorrelationId} From:{From} To:{To} Subject:{Subject}",
            correlationId, from, to, subject
            );


        if (subject.Contains("#2"))
            throw new Exception("Simulated SMTP failure");

    }
}
