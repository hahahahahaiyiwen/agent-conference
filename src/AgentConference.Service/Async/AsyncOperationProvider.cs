using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Async;

internal class AsyncOperationProvider : IAsyncOperationProvider
{
    ConcurrentDictionary<string, AsyncOperation> _operations = new();

    public Task<AsyncOperation> Create(string monitorId, CancellationToken cancellationToken)
    {
        var operation = new AsyncOperation
        {
            Id = Guid.NewGuid().ToString(),
            MonitorId = monitorId,
            Status = "Created"
        };

        _operations[operation.Id] = new AsyncOperation
        {
            Id = operation.Id,
            MonitorId = operation.MonitorId,
            Status = operation.Status
        };

        return Task.FromResult(operation);
    }

    public Task Update(AsyncOperation operation, CancellationToken cancellationToken)
    {
        if (_operations.ContainsKey(operation.Id))
        {
            _operations[operation.Id] = new AsyncOperation
            {
                Id = operation.Id,
                MonitorId = operation.MonitorId,
                Status = operation.Status,
                Result = operation.Result
            };
        }
        else
        {
            throw new InvalidOperationException("AsyncOperation not found");
        }

        return Task.CompletedTask;
    }

    public Task<AsyncOperation> Get(string id, CancellationToken cancellationToken)
    {
        if (!_operations.TryGetValue(id, out var operation))
        {
            return null;
        }

        return Task.FromResult(
            new AsyncOperation
            {
                Id = operation.Id,
                MonitorId = operation.MonitorId,
                Status = operation.Status,
                Result = operation.Result
            });
    }
}