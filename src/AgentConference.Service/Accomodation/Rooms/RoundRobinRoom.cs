using AgentConference.Primitives;
using AgentConference.Service.Attendees;
using AgentConference.Service.Monitoring;
using AgentConference.Service.Notes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Accomodation;

internal class RoundRobinRoom : Room
{
    private readonly Queue<Attendee> _speakingQueue = new Queue<Attendee>();

    public RoundRobinRoom(int capacity) : base(capacity)
    {
    }

    protected override async Task Open(ProblemDefinition problem, CancellationToken cancellationToken)
    {
        string openningStatement = RoomPrompts.OpeningStatement + problem.ToProblemString();
        
        Notes.Append(RoomRole.Facilitator.ToString(), openningStatement);

        if (Monitor != null)
        {
            await Monitor.PublishRoomEvent(
                new RoomEvent(
                    DateTimeOffset.UtcNow,
                    RoomEventType.KickOff,
                    new Dictionary<string, string>()
                    {
                        { RoomEventProperties.Actor, RoomRole.Facilitator.ToString() },
                        { RoomEventProperties.Message, openningStatement }
                    }),
                cancellationToken);
        }
    }

    protected override async Task<AttendeeResponse<DiscussionPoint>> GetNextResponse<DiscussionPoint>(CancellationToken cancellationToken)
    {
        if (_speakingQueue.Count == 0)
        {
            foreach (Attendee attendee in Attendees)
            {
                _speakingQueue.Enqueue(attendee);
            }
        }

        if (!_speakingQueue.TryDequeue(out Attendee speaker))
        {
            return null;
        }

        if (Monitor != null)
        {
            await Monitor.PublishRoomEvent(
                new RoomEvent(
                    DateTimeOffset.UtcNow,
                    RoomEventType.InDiscussion,
                    new Dictionary<string, string>()
                    {
                        { RoomEventProperties.Actor, RoomRole.Facilitator.ToString() },
                        { RoomEventProperties.Message, "Next speaker is " + speaker.Name }
                    }),
                cancellationToken);
        }

        return await speaker.ThinkAndSpeak<DiscussionPoint>(
            Extensions.ToString(Notes.ReadAll(speaker.LastSpokeAt)),
            cancellationToken);
    }

    protected override async Task<Deliverable<T>> Finalize<T>(ProblemDefinition problem, CancellationToken cancellationToken)
    {
        DateTimeOffset cutoffTime = DateTimeOffset.UtcNow;

        // Facilitator closing statement 
        Notes.Append(RoomRole.Facilitator.ToString(), RoomPrompts.ClosingStatement);

        if (Monitor != null)
        {
            await Monitor.PublishRoomEvent(
                new RoomEvent(
                    cutoffTime,
                    RoomEventType.Close,
                    new Dictionary<string, string>()
                    {
                        { RoomEventProperties.Actor, RoomRole.Facilitator.ToString() },
                        { RoomEventProperties.Message, RoomPrompts.ClosingStatement }
                    }),
                cancellationToken);
        }

        // Collect final responses from all attendees
        Deliverable<T> deliverable = new Deliverable<T>()
        {
            Items = new List<T>()
        };

        foreach (Attendee attendee in Attendees)
        {
            AttendeeResponse<T> response = await attendee.ThinkAndSpeak<T>(
                Extensions.ToString(Notes.ReadAll(cutoffTime)),
                cancellationToken);

            if (response == null)
            {
                // Attendee did not respond, skip
                continue;
            }

            Notes.Append(attendee.Name,  JsonSerializer.Serialize(response.Response));

            deliverable.Items.Add(response.Response);

            if (Monitor != null)
            {
                await Monitor.PublishRoomEvent(
                    new RoomEvent(
                        DateTimeOffset.UtcNow,
                        RoomEventType.Close,
                        new Dictionary<string, string>()
                        {
                            { RoomEventProperties.Actor, response.Attendee.ToString() },
                            { RoomEventProperties.Id, response.Attendee.Id.ToString() },
                            { RoomEventProperties.Message, JsonSerializer.Serialize(response.Response) }
                        }),
                    cancellationToken);
            }
        }

        return deliverable;
    }

    public override Task Reset(CancellationToken cancellationToken)
    {
        _speakingQueue.Clear();

        return base.Reset(cancellationToken);
    }
}