using AgentConference.Primitives;
using AgentConference.Service.Attendees;
using AgentConference.Service.Monitoring;
using AgentConference.Service.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Accomodation;

internal abstract class Room : IRoom, IAsyncDisposable
{
    private readonly TimeSpan PacingInterval = TimeSpan.FromSeconds(1);
    private int _status;
    private List<Attendee> _attendees;
    private MeetingNotes _notes;
    private ProblemDefinition _problem;
    private CancellationTokenSource _cts;
    private string _id;

    public Room(int capacity)
    {
        Capacity = capacity;
        _id = Guid.NewGuid().ToString();
        _status = (int)RoomStatus.Empty;
    }

    public int Capacity { get; }

    public string Id => Volatile.Read(ref _id);
    
    public RoomStatus Status => (RoomStatus)Volatile.Read(ref _status);

    public IReadOnlyCollection<Attendee> Attendees
    {
        // Return a snapshot of attendees to avoid external modification
        get
        {
            return _attendees?.ToArray();
        }
    }

    public IRoomMonitor Monitor { get; set;}

    public ProblemDefinition Problem => Volatile.Read(ref _problem);

    protected MeetingNotes Notes => Volatile.Read(ref _notes);

    public async Task Setup(ProblemDefinition problem, IEnumerable<Attendee> attendees, CancellationToken cancellationToken = default)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem));
        }

        if (attendees == null)
        {
            throw new ArgumentNullException(nameof(attendees));
        }

        if (attendees.Count() > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(attendees), "Number of attendees exceeds room capacity.");
        }

        if (!TryTransition(RoomStatus.Empty, RoomStatus.Active))
        {
            throw new InvalidOperationException("Room status invalid. Cannot setup the room.");
        }

        if (Monitor != null)
        {
            await Monitor.PublishRoomEvent(
                new RoomEvent(
                    DateTimeOffset.UtcNow,
                    RoomEventType.Setup,
                    new Dictionary<string, string>()
                    {
                        { RoomEventProperties.Message, "Room setup initiated." },
                        { RoomEventProperties.AttendeeCount, attendees.Count().ToString() }
                    }),
                cancellationToken);

            foreach (Attendee attendee in attendees)
            {
                await Monitor.PublishRoomEvent(
                    new RoomEvent(
                        DateTimeOffset.UtcNow,
                        RoomEventType.Setup,
                        new Dictionary<string, string>()
                        {
                            { RoomEventProperties.Actor, attendee.ToString() },
                            { RoomEventProperties.Message, "Attendee has joined the room." }
                        }),
                    cancellationToken);
            }
        }

        CancellationTokenSource previous = Interlocked.Exchange(ref _cts, new CancellationTokenSource());

        previous?.Dispose();

        Interlocked.Exchange(ref _notes, new MeetingNotes());

        Interlocked.Exchange(ref _problem, problem);

        Interlocked.Exchange(ref _attendees, attendees.ToList());
    }

    public async Task KickOff(CancellationToken cancellationToken)
    {
        if (!TryTransition(RoomStatus.Active, RoomStatus.InDiscussion))
        {
            throw new InvalidOperationException("Room status invalid. Cannot kick off discussion.");
        }

        CancellationTokenSource roomCts = Volatile.Read(ref _cts) ?? throw new InvalidOperationException("Room has not been configured.");

        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, roomCts.Token);

        // Get the problem and notes
        MeetingNotes notes = Volatile.Read(ref _notes);

        // Opening statement
        await Open(Volatile.Read(ref _problem), linkedCts.Token);

        // Discussion loop
        while (!linkedCts.Token.IsCancellationRequested)
        {
            linkedCts.Token.ThrowIfCancellationRequested();

            AttendeeResponse<DiscussionPoint> response = await GetNextResponse<DiscussionPoint>(linkedCts.Token);

            if (response?.Response != null)
            {
                notes.Append(response.Attendee.Name, JsonSerializer.Serialize(response.Response));

                if (Monitor != null)
                {
                    await Monitor.PublishRoomEvent(
                        new RoomEvent(
                            DateTimeOffset.UtcNow,
                            RoomEventType.InDiscussion,
                            new Dictionary<string, string>()
                            {
                                { RoomEventProperties.Actor, response.Attendee.ToString() },
                                { RoomEventProperties.Id, response.Attendee.Id.ToString() },
                                { RoomEventProperties.Message, JsonSerializer.Serialize(response.Response) }
                            }),
                        cancellationToken);
                }

                response.Attendee.LastSpokeAt = DateTimeOffset.UtcNow;
            }

            await Task.Delay(PacingInterval, linkedCts.Token);
        }
    }

    public async Task<(Deliverable<T>, MeetingNotes)> Close<T>(CancellationToken cancellationToken)
    {
        if (!TryTransition(RoomStatus.InDiscussion, RoomStatus.Closing))
        {
            throw new InvalidOperationException("Room status invalid. Cannot close the room.");
        }

        CancellationTokenSource roomCts = Volatile.Read(ref _cts) ?? throw new InvalidOperationException("Room has not been configured.");

        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, roomCts.Token);

        Deliverable<T> deliverable = await Finalize<T>(Volatile.Read(ref _problem), linkedCts.Token);

        MeetingNotes notes = Interlocked.Exchange(ref _notes, null);

        return (deliverable, notes);
    }

    public virtual Task Reset(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CancellationTokenSource current = Interlocked.Exchange(ref _cts, null);

        current?.Cancel();

        // Mark the room as closed
        Interlocked.Exchange(ref _status, (int)RoomStatus.Closed);

        // Reset states
        Interlocked.Exchange(ref _attendees, null);

        Interlocked.Exchange(ref _notes, null);

        Interlocked.Exchange(ref _problem, null);

        current?.Dispose();

        // Return monitor to null
        Monitor = null;

        // Mark the room as empty
        Interlocked.Exchange(ref _status, (int)RoomStatus.Empty);

        Interlocked.Exchange(ref _id, Guid.NewGuid().ToString());

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        var cts = Interlocked.Exchange(ref _cts, null);
        cts?.Cancel();
        cts?.Dispose();
        return ValueTask.CompletedTask;
    }

    protected abstract Task Open(ProblemDefinition problem, CancellationToken cancellationToken);

    protected abstract Task<AttendeeResponse<DiscussionPoint>> GetNextResponse<DiscussionPoint>(CancellationToken cancellationToken);

    protected abstract Task<Deliverable<T>> Finalize<T>(ProblemDefinition problem, CancellationToken cancellationToken);

    private bool TryTransition(RoomStatus from, RoomStatus to)
    {
        if (from == to)
        {
            return false;
        }

        int original = Interlocked.CompareExchange(ref _status, (int)to, (int)from);

        return original == (int)from;
    }
}