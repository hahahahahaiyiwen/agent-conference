using System;
using System.Threading;
using System.Threading.Tasks;
using AgentConference.Service.Async;
using AgentConference.WebApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AgentConference.WebApi;

[ApiController]
[Route("api/[controller]")]
public class AsyncOperationController
{
    private readonly IAsyncOperationProvider _asyncOperationProvider;

    public AsyncOperationController(
        IAsyncOperationProvider asyncOperationProvider)
    {
        _asyncOperationProvider = asyncOperationProvider ?? throw new ArgumentNullException(nameof(asyncOperationProvider));
    }

    [HttpGet("{operationId}")]
    public async Task<ActionResult<AsyncOperationResponse>> GetAsyncOperation(string operationId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(operationId))
        {
            return new BadRequestObjectResult("Operation ID cannot be null or empty.");
        }

        AsyncOperation operation = await _asyncOperationProvider.Get(operationId, cancellationToken);

        AsyncOperationResponse response = operation.ToResponse();   

        return new OkObjectResult(response);
    }
    
}