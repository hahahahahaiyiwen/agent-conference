using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AgentConference.Service.Monitoring;

/// <summary>
/// Channel-based monitor that distributes room events to registered subscribers.
/// Each subscriber receives events on its own bounded channel so back-pressure from one consumer
/// does not block others.
/// </summary>
internal sealed class ChannelRoomMonitor : IRoomMonitor, IAsyncDisposable
{
    private readonly Channel<RoomEvent> _source;
    private readonly ConcurrentDictionary<IRoomSubscriber, CancellationTokenSource> _subscribers;
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Task _dispatcherTask;
    private bool _disposed;

    public ChannelRoomMonitor(int capacity = 100)
    {
        // Single producer (room) with multi-consumer support.
        BoundedChannelOptions options = new(capacity)
        {
            SingleWriter = false,
            SingleReader = false,
            FullMode = BoundedChannelFullMode.Wait
        };

        _source = Channel.CreateBounded<RoomEvent>(options);
        _subscribers = new ConcurrentDictionary<IRoomSubscriber, CancellationTokenSource>();
        _dispatcherTask = Task.Run(DispatchLoopAsync);
    }
    
    public string Id { get; } = Guid.NewGuid().ToString();
    
    public async ValueTask PublishRoomEvent(RoomEvent roomEvent, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        await _source.Writer.WriteAsync(roomEvent, cancellationToken);
    }

    public ValueTask<IAsyncDisposable> SubscribeAsync(IRoomSubscriber subscriber, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (subscriber == null)
        {
            throw new ArgumentNullException(nameof(subscriber));
        }

        CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdown.Token);

        if (!_subscribers.TryAdd(subscriber, linkedCts))
        {
            throw new InvalidOperationException("Subscriber already registered.");
        }

        return new ValueTask<IAsyncDisposable>(new AsyncActionDisposable(async () =>
        {
            if (_subscribers.TryRemove(subscriber, out CancellationTokenSource existing))
            {
                existing.Cancel();
                existing.Dispose();
                await subscriber.OnCompletedAsync(CancellationToken.None);
            }
        }));
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _source.Writer.TryComplete();
        _shutdown.Cancel();

        await _dispatcherTask.ConfigureAwait(false);

        List<Task> completionCalls = new();

        foreach (KeyValuePair<IRoomSubscriber, CancellationTokenSource> subscriber in _subscribers.ToArray())
        {
            if (_subscribers.TryRemove(subscriber.Key, out CancellationTokenSource existing))
            {
                existing.Cancel();
                existing.Dispose();
                completionCalls.Add(subscriber.Key.OnCompletedAsync(CancellationToken.None).AsTask());
            }
        }

        foreach (Task task in completionCalls)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            {
                // Ignore subscriber completion faults during disposal.
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ChannelRoomMonitor));
        }
    }

    private async Task DispatchLoopAsync()
    {
        try
        {
            while (await _source.Reader.WaitToReadAsync(_shutdown.Token).ConfigureAwait(false))
            {
                while (_source.Reader.TryRead(out RoomEvent roomEvent))
                {
                    await BroadcastAsync(roomEvent).ConfigureAwait(false);
                }
            }

            await NotifyCompletionAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            await NotifyCompletionAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await NotifyErrorAsync(ex).ConfigureAwait(false);
        }
    }

    private async Task BroadcastAsync(RoomEvent roomEvent)
    {
        List<Task> tasks = new();

        foreach (KeyValuePair<IRoomSubscriber, CancellationTokenSource> subscriber in _subscribers.ToArray())
        {
            CancellationToken token = subscriber.Value.Token;

            if (token.IsCancellationRequested)
            {
                continue;
            }

            tasks.Add(DispatchToSubscriberAsync(subscriber.Key, roomEvent, token));
        }

        foreach (Task task in tasks)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            {
                // Errors are handled within DispatchToSubscriberAsync.
            }
        }
    }

    private async Task DispatchToSubscriberAsync(IRoomSubscriber subscriber, RoomEvent roomEvent, CancellationToken token)
    {
        try
        {
            await subscriber.OnRoomEventAsync(roomEvent, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellations from the subscriber token.
        }
        catch (Exception ex)
        {
            await subscriber.OnErrorAsync(ex, token).ConfigureAwait(false);
        }
    }

    private async Task NotifyCompletionAsync()
    {
        foreach (KeyValuePair<IRoomSubscriber, CancellationTokenSource> subscriber in _subscribers.ToArray())
        {
            try
            {
                await subscriber.Key.OnCompletedAsync(subscriber.Value.Token).ConfigureAwait(false);
            }
            catch
            {
                // Ignore completion errors.
            }
        }
    }

    private async Task NotifyErrorAsync(Exception exception)
    {
        foreach (KeyValuePair<IRoomSubscriber, CancellationTokenSource> subscriber in _subscribers.ToArray())
        {
            try
            {
                await subscriber.Key.OnErrorAsync(exception, subscriber.Value.Token).ConfigureAwait(false);
            }
            catch
            {
                // Swallow subscriber errors triggered by OnError.
            }
        }
    }

    private sealed class AsyncActionDisposable : IAsyncDisposable
    {
        private readonly Func<ValueTask> _disposeAction;

        public AsyncActionDisposable(Func<ValueTask> disposeAction)
        {
            _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
        }

        public ValueTask DisposeAsync() => _disposeAction();
    }
}
