using AgentConference.Primitives;

namespace AgentConference.Service.Async;

public class AsyncOperation
{
    public string Id { get; init; }

    public string MonitorId { get; set; }

    public string Status { get; set; }

    public string Result { get; set; }
}