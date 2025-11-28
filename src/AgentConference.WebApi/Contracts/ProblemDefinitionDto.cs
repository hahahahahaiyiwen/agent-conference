using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgentConference.WebApi.Contracts;

public class ProblemDefinitionDto
{
    public string Context { get; set; }

    [Required]
    public string Statement { get; set; }

    public IList<MetadataDto> Metadata { get; set; }
}
