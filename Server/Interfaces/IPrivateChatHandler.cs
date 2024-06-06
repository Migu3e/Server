namespace Server.Interfaces;

public interface IPrivateChatHandler
{
    Task CreatePrivateChats(IClient newClient);
    Task HandleJoinPrivateRoom(IClient client, string message);
}