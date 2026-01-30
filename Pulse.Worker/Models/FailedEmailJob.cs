namespace Pulse.Worker.Models;

public record FailedEmailJob(
    EmailJob Original,
    string Error,
    DateTime FailedAtUtc
);
