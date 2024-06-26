using Server.Models;

namespace Server.Interfaces.RoomsAndChats;

public interface IRoomServices
{
    Task ExistingRooms();
    Task SendMessageToRoom(string username, string message, string roomName);
    Task HandleCreateRoom(Client client, string message);
    Task HandleJoinRoom(Client client, string message);
    Task HandleDeleteRoom(string message, Client client);
    Task LeaveRoom(Client client);
    Task HandleInviteRoom(Client client, string message);
    Task PrintRooms(Client client);
}