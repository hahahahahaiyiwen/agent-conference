using System;
using System.ComponentModel.DataAnnotations;

namespace AgentConference.WebApi.Contracts;

public class ProblemSolvingOptionsDto
{
    [Range(1, int.MaxValue)]
    public int? NumberOfAttendees { get; set; }

    [Range(1, int.MaxValue)]
    public int? TimeLimitSeconds { get; set; }

    [Range(0, int.MaxValue)]
    public int? MemoryLimitInMB { get; set; }

    public AttendeeOptionsDto[] AttendeeOptions { get; set; }
}
