using Server.Models;

namespace Server.Interfaces.RoomsAndChats;

public interface IPrivateChatHelper
{
    Task LeaveCurrentRoom(Client client);
    Task NotifyTargetUser(string targetUsername, string clientUsername);
    Task JoinRoom(Client client, Room room, string targetUsername);
}