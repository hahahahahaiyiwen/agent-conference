using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Notes;

public interface IMeetingNotesArchive
{
    Task ArchiveAsync(MeetingNotes notes, CancellationToken cancellationToken);
}