using Server.Models;

namespace Server.Interfaces.ClientHandler;

public interface ICleintHandler
{
    Task HandleClient(Client client);
    Task HandleLogout(Client client, string username);
    Task SendClientList(Client client);
    Task HandleMessage(Client client, string username, string message);
    Task UpdatedClientList(Client client);
}