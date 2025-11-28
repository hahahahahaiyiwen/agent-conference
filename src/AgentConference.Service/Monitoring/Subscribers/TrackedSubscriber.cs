using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Monitoring;
internal sealed class TrackedSubscriber : IRoomSubscriber, IAsyncDisposable
{
    private readonly InMemorySubscriber _inner;
    private readonly TrackedSubscriberProvider _owner;
    private bool _disposed;

    public TrackedSubscriber(InMemorySubscriber inner, TrackedSubscriberProvider owner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public string Id => _inner.Id;

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await _owner.RemoveAsync(_inner.Id);
    }

    public ValueTask OnRoomEventAsync(RoomEvent roomEvent, CancellationToken cancellationToken) => _inner.OnRoomEventAsync(roomEvent, cancellationToken);

    public ValueTask OnCompletedAsync(CancellationToken cancellationToken) => _inner.OnCompletedAsync(cancellationToken);

    public ValueTask OnErrorAsync(Exception exception, CancellationToken cancellationToken) => _inner.OnErrorAsync(exception, cancellationToken);

    public IReadOnlyList<RoomEvent> FlushEvents() => _inner.FlushEvents();
    
    public bool IsCompleted => _inner.IsCompleted;

    public Exception Error => _inner.Error;
}