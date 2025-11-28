using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AgentConference.WebApi.Contracts;

public class EvaluationProblemDto
{
    public string GroundTruth { get; set; }

    [Required]
    public string Query { get; set; }

    [Required]
    public string Response { get; set; }

    [Required]
    public string Criteria { get; set; }

    public IList<MetadataDto> Metadata { get; set; }
}
