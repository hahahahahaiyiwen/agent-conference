using System;

namespace AgentConference.Service.Accomodation;

internal readonly struct PoolKey : IEquatable<PoolKey>
{
    public PoolKey(RoomType roomType, int capacity)
    {
        RoomType = roomType;
        Capacity = capacity;
    }

    public RoomType RoomType { get; }

    public int Capacity { get; }

    public bool Equals(PoolKey other) => RoomType == other.RoomType && Capacity == other.Capacity;

    public override bool Equals(object obj) => obj is PoolKey other && Equals(other);

    public override int GetHashCode() => HashCode.Combine((int)RoomType, Capacity);
}