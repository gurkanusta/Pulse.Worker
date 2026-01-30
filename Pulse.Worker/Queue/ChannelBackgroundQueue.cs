using System.Threading.Channels;

namespace Pulse.Worker.Queue;

public class ChannelBackgroundQueue<T>(Channel<T> channel) : IBackgroundQueue<T>
{
    public ValueTask EnqueueAsync(T item, CancellationToken ct)
        => channel.Writer.WriteAsync(item, ct);

    public ValueTask<T> DequeueAsync(CancellationToken ct)
        => channel.Reader.ReadAsync(ct);
}
