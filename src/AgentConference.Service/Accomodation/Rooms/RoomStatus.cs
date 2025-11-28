namespace AgentConference.Service.Accomodation;

public enum RoomStatus
{
    Empty, // Room is empty and ready for setup
    Active, // Room is active and ready for discussion
    InDiscussion, // Discussion is ongoing
    Closing, // Discussion is wrapping up
    Closed // Discussion has ended, equipment can be reset
}