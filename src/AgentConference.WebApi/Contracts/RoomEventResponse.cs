using System;

namespace AgentConference.WebApi.Contracts;

public class RoomEventResponse
{
    public DateTimeOffset Timestamp { get; set; }

    public string Name { get; set; }

    public string Message { get; set; }
}