using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Monitoring;

/// <summary>
/// In-memory subscriber that stores room events for later inspection.
/// </summary>
internal sealed class InMemorySubscriber : IRoomSubscriber
{
	private readonly string _id;
	private readonly object _lock = new();
	private readonly List<RoomEvent> _events = new();
	private bool _completed;
	private Exception _error;

	public InMemorySubscriber(string id)
	{
		_id = id ?? throw new ArgumentNullException(nameof(id));
	}

	public string Id => _id;

	public bool IsCompleted
	{
		get
		{
			lock (_lock)
            {
				return _completed;
            }
		}
	}

	public Exception Error
	{
		get
		{
			lock (_lock)
			{
				return _error;
			}
		}
	}

	/// <summary>
	/// Provides the events received since last flush as immutable copies.
	/// </summary>
	public IReadOnlyList<RoomEvent> FlushEvents()
	{
		lock (_lock)
		{
			IReadOnlyList<RoomEvent> events = CloneEvents();
			
			_events.Clear();
			
			return events;
		}
	}

	public ValueTask OnRoomEventAsync(RoomEvent roomEvent, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		lock (_lock)
		{
			_events.Add(CloneEvent(roomEvent));
			return ValueTask.CompletedTask;
		}
	}

	public ValueTask OnCompletedAsync(CancellationToken cancellationToken)
	{
		lock (_lock)
		{
			_completed = true;
			return ValueTask.CompletedTask;
		}
	}

	public ValueTask OnErrorAsync(Exception exception, CancellationToken cancellationToken)
	{
		lock (_lock)
		{
			_error = exception;
			_completed = true;
			return ValueTask.CompletedTask;
		}
	}

	private RoomEvent CloneEvent(RoomEvent roomEvent)
	{
		IDictionary<string, string> properties = roomEvent.Properties != null
			? new Dictionary<string, string>(roomEvent.Properties)
			: null;

		return new RoomEvent(roomEvent.Timestamp, roomEvent.EventName, properties);
	}

	private IReadOnlyList<RoomEvent> CloneEvents()
	{
		RoomEvent[] snapshot = new RoomEvent[_events.Count];

		for (int i = 0; i < _events.Count; i++)
		{
			snapshot[i] = CloneEvent(_events[i]);
		}

		return snapshot;
	}

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
