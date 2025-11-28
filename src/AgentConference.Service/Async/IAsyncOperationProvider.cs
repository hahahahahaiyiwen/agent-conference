using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Async;

public interface IAsyncOperationProvider
{
    Task<AsyncOperation> Create(string monitorId,CancellationToken cancellationToken);

    Task Update(AsyncOperation operation, CancellationToken cancellationToken);

    Task<AsyncOperation> Get(string id, CancellationToken cancellationToken);
}