using AgentConference.Service.Accomodation;
using AgentConference.Service.Async;
using AgentConference.Service.Attendees;
using AgentConference.Service.Monitoring;
using AgentConference.Service.Notes;
using Microsoft.Extensions.DependencyInjection;

namespace AgentConference.Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentConferenceService(this IServiceCollection services)
    {
        services.AddSingleton<IConferenceService, AgentConferenceService>();
        services.AddSingleton<IMonitorProvider, MonitorProvider>();
        services.AddSingleton<ISubscriberProvider, TrackedSubscriberProvider>();
        services.AddSingleton<IRoomProvider, PooledRoomProvider>();
        services.AddSingleton<IMeetingNotesArchive, FileSystemMeetingNotesArchive>();
        services.AddSingleton<IAttendeeProvider, AgentAttendeeProvider>();
        services.AddSingleton<IAsyncOperationProvider, AsyncOperationProvider>();
        
        return services;
    }
}