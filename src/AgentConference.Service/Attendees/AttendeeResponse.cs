using System.Text.Json.Serialization;

namespace AgentConference.Service.Attendees;

public class AttendeeResponse<T>
{
    [JsonIgnore]
    public Attendee Attendee { get; set; }

    public T Response { get; set; }
}