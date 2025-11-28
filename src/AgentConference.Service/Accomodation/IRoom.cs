using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgentConference.Primitives;
using AgentConference.Service.Accomodation;
using AgentConference.Service.Attendees;
using AgentConference.Service.Monitoring;
using AgentConference.Service.Notes;

public interface IRoom : IAsyncDisposable
{
    // Empty, Active, InDiscussion, Closing, Closed
    RoomStatus Status { get; }

    // Attendees currently in the room
    IReadOnlyCollection<Attendee> Attendees { get; }

    // Room Capacity
    int Capacity { get; }

    // Room monitor
    IRoomMonitor Monitor { get; set; }

    // Problem being discussed in the room
    ProblemDefinition Problem { get; }

    // Empty => Active
    Task Setup(ProblemDefinition problem, IEnumerable<Attendee> attendees, CancellationToken cancellationToken = default);

    // Active => InDiscussion
    Task KickOff(CancellationToken cancellationToken);

    // InDiscussion => Closing
    Task<(Deliverable<T>, MeetingNotes)> Close<T>(CancellationToken cancellationToken);

    // Any status => Empty
    Task Reset(CancellationToken cancellationToken);
}