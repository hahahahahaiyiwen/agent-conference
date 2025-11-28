using AgentConference.Service.Monitoring;
using AgentConference.WebApi.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.WebApi;

[ApiController]
[Route("api/[controller]")]
public class MonitoringController
{
    private readonly ISubscriberProvider _subscriberProvider;

    public MonitoringController(
        ISubscriberProvider subscriberProvider)
    {
        _subscriberProvider = subscriberProvider ?? throw new ArgumentNullException(nameof(subscriberProvider));
    }

    [HttpGet("{monitorId}")]
    public async Task<ActionResult<AsyncOperationResponse>> Monitor(string monitorId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(monitorId))
        {
            return new BadRequestObjectResult("Monitor ID cannot be null or empty.");
        }

        IRoomSubscriber subscriber = _subscriberProvider.Get(monitorId);

        if (subscriber == null)
        {
            return new NotFoundObjectResult($"Monitor with ID '{monitorId}' not found.");
        }

        return new OkObjectResult(
            subscriber
                .FlushEvents()
                .Select(x => x.ToResponse()));
    }
    
}