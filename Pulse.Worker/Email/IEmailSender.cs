namespace Pulse.Worker.Email;

    public interface IEmailSender
    {
    Task SendAsync(string from,string to, string subject, string body,Guid correlationId, CancellationToken ct);
}

