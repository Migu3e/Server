namespace Server.Interfaces.ClientHandler;

public interface ICleintHandler
{
    Task HandleClient(IClient client);
    Task HandleLogout(IClient client, string username);
    Task SendClientList(IClient client);
    Task HandleMessage(IClient client, string username, string message);
    Task UpdatedClientList(IClient client);
}