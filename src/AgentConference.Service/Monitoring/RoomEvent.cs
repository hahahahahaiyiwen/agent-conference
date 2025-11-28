using System;
using System.Collections.Generic;
using AgentConference.Service.Accomodation;

namespace AgentConference.Service.Monitoring;

public readonly struct RoomEvent
{
    public RoomEvent(DateTimeOffset timestamp, RoomEventType eventName, IDictionary<string, string> properties)
    {
        Timestamp = timestamp;
        EventName = eventName;
        Properties = properties;
    }

    public DateTimeOffset Timestamp { get; }

    public RoomEventType EventName { get; }

    public IDictionary<string, string> Properties { get; }
}