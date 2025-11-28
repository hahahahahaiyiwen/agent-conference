using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Monitoring;

public interface IRoomMonitor : IAsyncDisposable
{
    string Id { get; }

    ValueTask PublishRoomEvent(RoomEvent roomEvent, CancellationToken cancellationToken);

    ValueTask<IAsyncDisposable> SubscribeAsync(IRoomSubscriber subscriber, CancellationToken cancellationToken);
}