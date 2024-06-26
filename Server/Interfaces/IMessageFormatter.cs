using Server.Models;

namespace Server.Interfaces;

public interface IMessageFormatter
{
    string Response(string username, string message);
    string RoomWasDeleted(string roomName);
    string RoomWasCreated(string username, string roomName);

    string ClientJoinedRoom(string roomName, string clientName);

    string ClientLeftRoom(string roomName, string clientName);

    string InviteToRoom(string roomName, string clientName);

    string InviteToRoomMessege(string roomName, string clientName);

    string AllClientListMessage(List<string> onlineClients, List<string> offlineClients, string currentUser);

    string UpdatedClientListMessage(List<string> onlineClients, string newClient);
    string ClientListMessage(List<string> onlineClients, string currentUser);
    
    string GenerateRoomListMessage(List<Room> rooms, string currentUsername);
    
    string UserHasEnteredPrivateChat(string username);

    string UserJoinedPrivateRoom(string username, string roomName);
    string RoomDoesNotExist(string targetUsername);
}