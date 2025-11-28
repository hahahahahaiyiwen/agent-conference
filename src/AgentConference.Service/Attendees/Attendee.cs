using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Attendees;

public abstract class Attendee
{
    public Attendee()
    {
    }

    public Attendee(string name)
    {
        Name = name;
    }

    public string Id { get; set; } = System.Guid.NewGuid().ToString();

    public string Name { get; private set; } = "Unnamed Attendee";

    public DateTimeOffset? LastSpokeAt { get; set; }

    public abstract Task<AttendeeResponse<T>> ThinkAndSpeak<T>(string question, CancellationToken cancellationToken);

    public override abstract string ToString();
}