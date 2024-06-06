using Server.Interfaces;
using Server.Models;

namespace Server.Services;

public class PrivateChatHandler : IPrivateChatHandler
{
    private readonly IChatServer _chatServer;
    private readonly IRoomServices _roomServices;
    private readonly IPrivateChatHandler _privateChatHandler;


    public PrivateChatHandler(IChatServer server,IRoomServices roomServices)
    {
        _chatServer = server;
        _roomServices = roomServices;
    }

    public async Task CreatePrivateChats(IClient newClient)
    {
        foreach (var existingClient in _chatServer.clients)
        {
            if (existingClient != newClient)
            {
                string chatName = $"|private| {existingClient.Username}-{newClient.Username}";
                IRoom privateChat = new Room(chatName);
                _chatServer.rooms.Add(privateChat);


                Console.WriteLine($"Private chat '{chatName}' created between {existingClient.Username} and {newClient.Username}");
            }
        }
    }


    public async Task HandleJoinPrivateRoom(IClient client, string message)
    {
            
        var parts = message.Split(' ');
        if (client.Username == parts[1])
        {
            await _chatServer.ServerPrivateMessage(client, $"Error Entering the room");
        }
        else
        {
                
            var room = _chatServer.rooms.FirstOrDefault(r => r.Name.Contains(parts[1])&&r.Name.Contains(client.Username));
            if (room != null)
            {
                if (parts.Length == 2)
                {
                    _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName).RemoveClientFromRoom(client);
                    Console.WriteLine($"Client {client.Username} has left room {client.RoomName}");
                    _chatServer.rooms.FirstOrDefault(r => r.Name.Contains(parts[1])&&r.Name.Contains(client.Username)).AddClientToRoom(client);
                    client.RoomName = _chatServer.rooms.FirstOrDefault(r => r.Name.Contains(parts[1])&&r.Name.Contains(client.Username)).Name;
                    Console.WriteLine($"Client {client.Username} has joined private room {client.RoomName}");
                    await _roomServices.SendMessageToRoom("Server", $"{client.Username} has joined the private room {client.RoomName}",client.RoomName);
                }
            }
            else
            {
                await _chatServer.ServerPrivateMessage(client, $"Room {parts[1]} doesn't exist");
            }
        }

    }

}