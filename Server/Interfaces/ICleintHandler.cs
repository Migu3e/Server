namespace Server.Interfaces;

public interface ICleintHandler
{
    Task HandleClient(IClient client);
    Task HandleLogout(IClient client, string username);
    Task SendClientList(IClient client);
    Task UpdatedClientList(IClient client);
}