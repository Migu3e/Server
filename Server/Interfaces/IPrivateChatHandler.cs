namespace Server.Interfaces;

public interface IPrivateChatHandler
{
    Task CreatePrivateChats();
    Task HandleJoinPrivateRoom(IClient client, string message);
}