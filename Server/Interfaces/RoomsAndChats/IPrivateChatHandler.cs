namespace Server.Interfaces.RoomsAndChats;

public interface IPrivateChatHandler
{
    Task HandleJoinPrivateRoom(IClient client, string message);
}