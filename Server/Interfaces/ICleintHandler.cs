namespace Server.Interfaces;

public interface ICleintHandler
{
    Task HandleClient(IClient client);
    Task HandleLogout(IClient client, string username);
    Task SendClientList(IClient client);
    Task HandleComplexMessage(IClient client, string username, string message);
    Task HandleMessage(IClient client, string username, string message);
    Task UpdatedClientList(IClient client);
}