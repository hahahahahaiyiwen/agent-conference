using System.ComponentModel.DataAnnotations;

namespace AgentConference.WebApi.Contracts;

public class SolveProblemRequest
{
    [Required]
    public ProblemDefinitionDto Problem { get; set; }

    public ProblemSolvingOptionsDto Options { get; set; }
}
