using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgentConference.Primitives;
using AgentConference.Service.Attendees;
using AgentConference.Service.Monitoring;
using AgentConference.Service.Notes;

namespace AgentConference.Service.Accomodation;

internal class PooledRoom : IRoom, IAsyncDisposable
{
    private readonly PooledRoomProvider _provider;
    private readonly Room _room;
    private readonly PoolKey _poolKey;
    private bool _disposed;

    public PooledRoom(PooledRoomProvider provider, Room room, PoolKey poolKey)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _room = room ?? throw new ArgumentNullException(nameof(room));
        _poolKey = poolKey;
    }

    public RoomStatus Status
    {
        get
        {
            ThrowIfDisposed();
            return _room.Status;
        }
    }

    public IReadOnlyCollection<Attendee> Attendees
    {
        get
        {
            ThrowIfDisposed();
            return _room.Attendees;
        }
    }

    public int Capacity
    {
        get
        {
            ThrowIfDisposed();
            return _room.Capacity;
        }
    }

    public IRoomMonitor Monitor
    {
        get
        {
            ThrowIfDisposed();
            return _room.Monitor;
        }
        set
        {
            ThrowIfDisposed();
            _room.Monitor = value;
        }
    }

    public ProblemDefinition Problem
    {
        get
        {
            ThrowIfDisposed();
            return _room.Problem;
        }
    }

    public Task Setup(ProblemDefinition problem, IEnumerable<Attendee> attendees, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _room.Setup(problem, attendees, cancellationToken);
    }

    public Task KickOff(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return _room.KickOff(cancellationToken);
    }

    public Task<(Deliverable<T>, MeetingNotes)> Close<T>(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return _room.Close<T>(cancellationToken);
    }

    public Task Reset(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return _room.Reset(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await _provider.ReturnRoom(_room, _poolKey);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(PooledRoom));
        }
    }
}