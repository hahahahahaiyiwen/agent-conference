using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Monitoring;

/// <summary>
/// Tracks long-lived room subscribers and removes them once disposed.
/// </summary>
internal sealed class TrackedSubscriberProvider : ISubscriberProvider
{
    private readonly ConcurrentDictionary<string, IRoomSubscriber> _subscribers = new();

    public IRoomSubscriber Get(string id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        _subscribers.TryGetValue(id, out IRoomSubscriber subscriber);
        return subscriber;
    }

    public IRoomSubscriber Create(string Id)
    {
        InMemorySubscriber subscriber = new(Id);

        if (!_subscribers.TryAdd(Id, subscriber))
        {
            throw new InvalidOperationException($"Subscriber with id '{Id}' already exists.");
        }

        return new TrackedSubscriber(subscriber, this);
    }

    internal ValueTask RemoveAsync(string id)
    {
        _subscribers.TryRemove(id, out _);

        return ValueTask.CompletedTask;
    }
}