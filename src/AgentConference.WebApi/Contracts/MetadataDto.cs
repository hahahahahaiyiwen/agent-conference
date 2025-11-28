using System.ComponentModel.DataAnnotations;

namespace AgentConference.WebApi.Contracts;

public class MetadataDto
{
    [Required]
    public string Key { get; set; }

    public string Value { get; set; }
}
