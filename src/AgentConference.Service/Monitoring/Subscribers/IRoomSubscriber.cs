using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Monitoring;

public interface IRoomSubscriber : IAsyncDisposable
{
    string Id { get; }
    ValueTask OnRoomEventAsync(RoomEvent roomEvent, CancellationToken cancellationToken);
    ValueTask OnCompletedAsync(CancellationToken cancellationToken);
    ValueTask OnErrorAsync(Exception exception, CancellationToken cancellationToken);
    IReadOnlyList<RoomEvent> FlushEvents();
}