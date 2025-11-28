using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Accomodation;

internal class PooledRoomProvider : IRoomProvider, IAsyncDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<PooledRoomProvider> _logger;
    private readonly ConcurrentDictionary<PoolKey, ConcurrentBag<Room>> _pools = new();

    public PooledRoomProvider(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = _loggerFactory.CreateLogger<PooledRoomProvider>();
    }

    public IRoom GetRoom(RoomCreationOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.Capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.Capacity), "Room capacity must be greater than zero.");
        }

        PoolKey poolKey = new PoolKey(options.RoomType, options.Capacity);

        Room room = GetRoomFromPool(poolKey);

        return new PooledRoom(this, room, poolKey);
    }

    public async Task ReturnRoom(Room room, PoolKey poolKey)
    {
        if (room == null)
        {
            throw new ArgumentNullException(nameof(room));
        }

        try
        {
            await room.Reset(CancellationToken.None);

            ConcurrentBag<Room> pool = _pools.GetOrAdd(poolKey, _ => new ConcurrentBag<Room>());

            pool.Add(room);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Room reset failed. Discarding instance from pool.");

            await room.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var pool in _pools.Values)
        {
            while (pool.TryTake(out Room room))
            {
                await room.DisposeAsync();
            }
        }
    }

    private Room GetRoomFromPool(PoolKey poolKey)
    {
        ConcurrentBag<Room> pool = _pools.GetOrAdd(poolKey, _ => new ConcurrentBag<Room>());

        if (pool.TryTake(out Room pooledRoom))
        {
            return pooledRoom;
        }

        return CreateRoom(poolKey);
    }

    private Room CreateRoom(PoolKey poolKey)
    {
        switch (poolKey.RoomType)
        {
            case RoomType.RoundRobin:
                return new RoundRobinRoom(poolKey.Capacity);
            case RoomType.FreeFloor:
            default:
                throw new NotSupportedException($"Room type {poolKey.RoomType} is not supported.");
        }
    }
}