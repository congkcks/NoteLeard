using System.Threading.Channels;

namespace NoteLearn.Services.Background;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundTaskQueue(int capacity = 100)
    {
        _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem)
    {
        if (workItem is null) throw new ArgumentNullException(nameof(workItem));
        return _queue.Writer.WriteAsync(workItem);
    }

    public ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        => _queue.Reader.ReadAsync(cancellationToken);
}
