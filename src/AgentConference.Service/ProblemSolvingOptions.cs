using System;
using System.Collections.Generic;
using AgentConference.Primitives;
using AgentConference.Service.Attendees;

namespace AgentConference.Service;

public class ProblemSolvingOptions
{
    public TimeSpan TimeLimit { get; set; } = TimeSpan.FromSeconds(60);

    public int MemoryLimitInMB { get; set; }

    public IEnumerable<AttendeeCreationOptions> AttendeeOptions { get; set; }
}