using System;

namespace AgentConference.Service.Monitoring;

internal class MonitorProvider : IMonitorProvider
{
    public IRoomMonitor GetMonitor()
    {
        return new ChannelRoomMonitor();
    }
}