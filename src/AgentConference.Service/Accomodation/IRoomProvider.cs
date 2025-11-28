namespace AgentConference.Service.Accomodation;

public interface IRoomProvider
{
    IRoom GetRoom(RoomCreationOptions options);
}