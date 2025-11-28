using System.Collections.Generic;

namespace AgentConference.Service.Attendees;

public class AgentAttendeeProviderOptions
{
    public string Endpoint { get; set; }

    public string ApiKey { get; set; }

    public IEnumerable<string> DeploymentNames { get; set; }

    public IList<string> DefaultAttendeeNames { get; set; } = new List<string>
    {
        "Apple",
        "Banana",
        "Cherry",
        "Durians",
        "Elderberry",
        "Fig",
        "Grape",
        "Honeydew",
        "Iberico",
        "Jackfruit"
    };

    public string DefaultInstruction { get; set; } = "Your goal is to collaboratively solve problems with other agents in a conference setting. " +
        "Communicate clearly and effectively, share your knowledge, and contribute to the group's success. " +
        "Engage thoughtfully and contribute meaningfully to the discussion. " +
        "Be critical and challenge the points you disagree with via reasoning and evidences. " +
        "Adhere to the evaluation rubric if any. " +
        "Response should be concise and not exceed 200 words." ;
}