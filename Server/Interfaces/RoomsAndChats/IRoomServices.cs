namespace Server.Interfaces;

public interface IRoomServices
{
    Task HandleJoinRoom(IClient client, string message);
    Task HandleCreateRoom(IClient client, string message);
    Task SendMessageToRoom(string username, string message, string roomName);
    Task LeaveRoom(IClient client);
    Task HandleInviteRoom(IClient client, string message);
    Task ExistingRooms();
    Task HandleDeleteRoom(string message, IClient client);
    Task PrintRooms(IClient client);
}