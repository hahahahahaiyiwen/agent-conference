using Microsoft.Extensions.Logging;

namespace AgentConference.Service.Accomodation;

internal static class LoggingExtensions
{
    public static void LogRoomActivity(this ILogger<Room> logger, Room room, string identifier, string activity)
    {
        logger.LogInformation("Room: {RoomId}, id: {Identifier}: activity: {Activity}",
            room.Id, identifier, activity);
    }
}