using System.Net.Sockets;
using System.Text;
using Client.MongoDB;
using MongoDB.Driver;
using Server.Const;
using Server.Interfaces;
using Server.MongoDB;


namespace Server.Services;

public class ClientHandler : ICleintHandler
{
    public ClientHandler(IChatServer chatServer, IRoomServices roomServices, IPrivateChatHandler privateChatHandler)
    {
        _chatServer = chatServer;
        _roomServices = roomServices;
        _privateChatHandler = privateChatHandler;
        _messageFormatter = new MessageFormatter();
        _clientHandlerHelper = new ClientHandlerHelper();
    }

    private IChatServer _chatServer;
    private IRoomServices _roomServices;
    private IPrivateChatHandler _privateChatHandler;
    private IMessageFormatter _messageFormatter;
    private IClientHandlerHelper _clientHandlerHelper;

    
    public async Task HandleClient(IClient client)
{
    Socket handler = client.ClientSocket;
    while (true)
    {
        var parts = await _clientHandlerHelper.GetNameAndMessageParts(handler);
        if (parts == null)
        {
            break;
        }
        
        var username = parts[0];
        var message = parts[1];
        if (!string.IsNullOrEmpty(message))
        {
            await HandleMessage(client, username, message);
        }
    }

} 

    public async Task HandleMessage(IClient client, string username, string message)
{
    if (ConstCheckOperations.IsCommand(message))
    {
        await (message.ToLower() switch
        {
   
            _ when ConstCheckOperations.IsList(message) => SendClientList(client),
            _ when ConstCheckOperations.IsHelp(message) => _chatServer.ServerPrivateMessage(client,ConstMasseges.HelpMessage),
            _ when ConstCheckOperations.IsLogout(message) => HandleLogout(client, client.Username),
            _ when ConstCheckOperations.IsCreateRoom(message) => _roomServices.HandleCreateRoom(client, message),
            _ when ConstCheckOperations.IsJoinRoom(message) => _roomServices.HandleJoinRoom(client, message),
            _ when ConstCheckOperations.IsInviteRoom(message) => _roomServices.HandleInviteRoom(client, message),
            _ when ConstCheckOperations.IsDeleteRoom(message) => _roomServices.HandleDeleteRoom(message, client),
            _ when ConstCheckOperations.IsLeave(message)=> _roomServices.LeaveRoom(client),
            _ when ConstCheckOperations.IsListRooms(message) => _roomServices.PrintRooms(client),
            _ when ConstCheckOperations.IsPrivate(message) => _privateChatHandler.HandleJoinPrivateRoom(client, message),
            _ when ConstCheckOperations.IsListAll(message) => SendAllClientList(client),

            _=> _roomServices.SendMessageToRoom(client.Username,message,client.RoomName)

        });
    }
    else
    {
        _roomServices.SendMessageToRoom(client.Username, message, client.RoomName);
    }
    
}




    public async Task HandleLogout(IClient client, string username)
    {
        await _chatServer.ServerPrivateMessage(client,ConstMasseges.DisconnectedMassege);
        await _roomServices.SendMessageToRoom(username, ConstMasseges.LeftChat, client.RoomName);
        Console.WriteLine($"{ConstMasseges.ServerConst} - {username} has disconnected");
        _chatServer.clients.Remove(client);
        await _roomServices.LeaveRoom(client);
        client.ClientSocket.Shutdown(SocketShutdown.Both);
        client.ClientSocket.Close();
    }


    public async Task SendClientList(IClient client)
    {
        var onlineClients = _chatServer.clients.Select(c => c.Username).ToList();
        string listOfOnlineClients = _messageFormatter.ClientListMessage(onlineClients, client.Username);
        await _chatServer.ServerPrivateMessage(client, listOfOnlineClients);
    }

    
    
    public async Task SendAllClientList(IClient client)
    {
        var clientsCollection = MongoDBRoomHelper.GetCollection<ClientDB>(ConstMasseges.CollectionDataClient);
        var allClients = await clientsCollection.Find(_ => true).ToListAsync();
        var onlineClients = _chatServer.clients.Select(c => c.Username).ToList();
        var allClientNames = allClients.Select(c => c.UserName).ToList();
        var offlineClients = allClientNames.Where(c => !onlineClients.Contains(c)).ToList();
        string listOfOnlineClients = _messageFormatter.AllClientListMessage(onlineClients, offlineClients, client.Username);
        await _chatServer.ServerPrivateMessage(client, listOfOnlineClients);
    }



    public async Task UpdatedClientList(IClient client)
    {
        var onlineClients = _chatServer.clients.Select(c => c.Username).ToList();
        string listOfOnlineClients = _messageFormatter.UpdatedClientListMessage(onlineClients, client.Username);
        await _chatServer.PrintToAll(client, listOfOnlineClients);
    }


}
