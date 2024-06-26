using Server.Models;

namespace Server.Interfaces.RoomsAndChats;

public interface IPrivateChatHandler
{
    Task HandleJoinPrivateRoom(Client client, string message);
}