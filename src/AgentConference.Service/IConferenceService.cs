using AgentConference.Primitives;
using AgentConference.Service.Async;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service;

public interface IConferenceService
{
    Task<Deliverable<T>> Solve<T>(ProblemDefinition problem, ProblemSolvingOptions options, CancellationToken cancellationToken);

    Task<AsyncOperation> SolveAsync<T>(ProblemDefinition problem, ProblemSolvingOptions options, CancellationToken cancellationToken);
}
