using System.ComponentModel.DataAnnotations;

namespace AgentConference.WebApi.Contracts;

public class EvaluationRequest
{
    [Required]
    public EvaluationProblemDto Problem { get; set; }

    public ProblemSolvingOptionsDto Options { get; set; }
}
