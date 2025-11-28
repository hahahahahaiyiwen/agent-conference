using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentConference.Service.Attendees;

public interface IAttendeeProvider
{
    public Task<IEnumerable<Attendee>> GetAttendees(IEnumerable<AttendeeCreationOptions> options);
}