namespace Pulse.Worker.Queue;

public interface IBackgroundQueue<T>
{
    ValueTask EnqueueAsync(T item, CancellationToken ct);
    ValueTask<T> DequeueAsync(CancellationToken ct);
}
