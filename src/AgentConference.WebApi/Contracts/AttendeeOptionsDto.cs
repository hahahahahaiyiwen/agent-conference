using System.ComponentModel.DataAnnotations;

namespace AgentConference.WebApi.Contracts;

public class AttendeeOptionsDto
{
    [MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(50)]
    [AllowedValues("gpt-4.1-mini-global", "gpt-5-mini-global")]
    public string Model { get; set; }

    [MaxLength(200)]
    public string Instruction { get; set; }
}