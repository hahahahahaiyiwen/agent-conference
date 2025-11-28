namespace AgentConference.Service.Accomodation;

public class RoomCreationOptions
{
    public int Capacity { get; set; } = 5;

    public RoomType RoomType { get; set; } = RoomType.RoundRobin;
}