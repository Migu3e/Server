namespace Server.Interfaces.RoomsAndChats;

public interface IPrivateChatHelper
{
    Task LeaveCurrentRoom(IClient client);
    Task NotifyTargetUser(string targetUsername, string clientUsername);
    Task JoinRoom(IClient client, IRoom room, string targetUsername);
}