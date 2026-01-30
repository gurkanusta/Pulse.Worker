namespace Pulse.Worker.Models;
public record EmailJob(
    string To,
        string Subject,
        string Body,
        DateTime CreatedAtUtc,
        Guid CorrelationId
    );
    
      

