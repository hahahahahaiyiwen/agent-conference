namespace AgentConference.Service.Monitoring;

public interface IMonitorProvider
{
    IRoomMonitor GetMonitor();
}