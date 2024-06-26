namespace Server.Interfaces.RoomsAndChats;

public interface IRoomServices
{
    Task ExistingRooms();
    Task SendMessageToRoom(string username, string message, string roomName);
    Task HandleCreateRoom(IClient client, string message);
    Task HandleJoinRoom(IClient client, string message);
    Task HandleDeleteRoom(string message, IClient client);
    Task LeaveRoom(IClient client);
    Task HandleInviteRoom(IClient client, string message);
    Task PrintRooms(IClient client);
}