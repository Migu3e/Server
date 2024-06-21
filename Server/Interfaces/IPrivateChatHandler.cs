namespace Server.Interfaces;

public interface IPrivateChatHandler
{
    Task HandleJoinPrivateRoom(IClient client, string message);
}