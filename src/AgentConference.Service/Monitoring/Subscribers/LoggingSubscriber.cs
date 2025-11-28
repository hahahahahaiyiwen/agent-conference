using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AgentConference.Service.Monitoring;

internal class LoggingSubscriber : IRoomSubscriber
{
    private readonly string _id = Guid.NewGuid().ToString();

    private readonly ILogger<LoggingSubscriber> _logger;

    public LoggingSubscriber(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LoggingSubscriber>();
    }
    
    public string Id => _id;

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public IReadOnlyList<RoomEvent> FlushEvents()
    {
        return Array.Empty<RoomEvent>();
    }

    public ValueTask OnCompletedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Room monitoring completed.");
        return ValueTask.CompletedTask;
    }

    public ValueTask OnErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error occurred in room monitoring.");
        return ValueTask.CompletedTask;
    }

    public ValueTask OnRoomEventAsync(RoomEvent roomEvent, CancellationToken cancellationToken)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Timestamp: {roomEvent.Timestamp}, ");
        builder.Append($"Event: {roomEvent.EventName}, ");
        if (roomEvent.Properties != null)
        {
            foreach (var prop in roomEvent.Properties)
            {
                builder.Append($"{prop.Key}: {prop.Value}, ");
            }
        }
        _logger.LogInformation(builder.ToString());
        
        return ValueTask.CompletedTask;
    }
}