namespace Pulse.Worker.Options
{
    public class EmailOptions
    {
        public string From { get; set; } = "noreply@local";
        public int SimulatedLatencyMs { get; set; } = 250;
    }
}
