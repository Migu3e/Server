using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using MongoDB.Driver;
using Server.Const;
using Server.Interfaces;
using Server.Models;
using Server.MongoDB;

namespace Server.Services;

public class RoomServices : IRoomServices
{
    private readonly IChatServer _chatServer;

    public RoomServices(IChatServer services)
    {
        _chatServer = services;
    }
    
    public async Task ExistingRooms()
    {
        var collection = MongoDBHelper.GetCollection<RoomDB>("chats");
        var rooms = await collection.Find(_ => true).ToListAsync();

        foreach (var room in rooms)
        {
            var roomInstance = new Room(room.RoomName);
            if (_chatServer.rooms.Any(p => p.Name == room.RoomName))
            {
                // Room already exists, skip adding
                continue;
            }
            else
            {
                _chatServer.rooms.Add(roomInstance);
            }
        }
    }

    public async Task SendMessageToRoom(string username, string message, string roomName)
    {
        var room = _chatServer.rooms.FirstOrDefault(r => r.Name == roomName);
        if (room != null)
        {
            if (roomName == "Main")
            {
                _chatServer.ServerPrivateMessage(_chatServer.clients.FirstOrDefault(p => username == p.Username),"Cannot Massage In Room Main");
            }
            else
            {
                foreach (var member in room.Members)
                {
                    if (member.Username != username)
                    {
                        var response = $"<{DateTime.Now} - {username}> {message}";

                        var responseByte = Encoding.UTF8.GetBytes(response);
                        await member.ClientSocket.SendAsync(responseByte, SocketFlags.None);
                    }
                    
                    var newResponse = $"<{DateTime.Now} - {username}> {message}\n";
                    room.Messages.Add(newResponse);

                    var collection = MongoDBHelper.GetCollection<RoomDB>("chats");
                    var filter = Builders<RoomDB>.Filter.Eq(r => r.RoomName, roomName);
                    var update = Builders<RoomDB>.Update.Push(r => r.MList, newResponse);

                    await collection.UpdateOneAsync(filter, update);
                    
                }
            }
                
        }
        else
        {
            Console.WriteLine($"Room '{roomName}' does not exist.");
        }
    }


    public async Task HandleCreateRoom(IClient client, string message)
        {
            var parts = message.Split(' ');
            if (ConstCheckCommands.CanCreateRoom(message,_chatServer.rooms) == ConstMasseges.RoomWasCreated)
            {
                string roomName = parts[1];
                var room = new Room(roomName);
                _chatServer.rooms.Add(room);
                Console.WriteLine($"Room {roomName} was created by {client.Username}");
                await _chatServer.PrintToAll(client,$"Room {roomName} was created by {client.Username}");
                
                var collection = MongoDBHelper.GetCollection<RoomDB>("chats");
                var data = new RoomDB
                {
                    RoomName = roomName,
                    MList = new List<string>()
                    
                };

                collection.InsertOne(data);

            }
            else
            {
                _chatServer.PrivateMessage(client, ConstCheckCommands.CanCreateRoom(message,_chatServer.rooms));
            }
        }
    public async Task HandleJoinRoom(IClient client, string message)
    {
        var parts = message.Split(' ');

        // Find the room in the in-memory collection
        var room = _chatServer.rooms.FirstOrDefault(r => r.Name == parts[1]);
        if (ConstCheckCommands.CanJoinRoom(message, room) == "true")
        {
            // Remove the client from the current room
            var currentRoom = _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName);
            currentRoom?.RemoveClientFromRoom(client);
            Console.WriteLine($"Client {client.Username} has left room {client.RoomName}");

            // Add the client to the new room
            var newRoom = _chatServer.rooms.FirstOrDefault(r => r.Name == parts[1]);
            newRoom?.AddClientToRoom(client);
            client.RoomName = parts[1];
            Console.WriteLine($"Client {client.Username} has joined room {client.RoomName}");

            // Fetch the room document from MongoDB to get the message list (MList)
            var collection = MongoDBHelper.GetCollection<RoomDB>("chats");
            var filter = Builders<RoomDB>.Filter.Eq(r => r.RoomName, parts[1]);
            var roomFromDb = await collection.Find(filter).FirstOrDefaultAsync();

            if (roomFromDb != null)
            {
                foreach (var messageforeach in roomFromDb.MList)
                {
                    await _chatServer.PrivateMessage(client, messageforeach);
                }
            }

            // Notify the room that the client has joined
            await SendMessageToRoom(ConstMasseges.ServerConst, $"{client.Username} has joined the room {parts[1]}", parts[1]);
        }
        else
        {
            await _chatServer.PrivateMessage(client, ConstCheckCommands.CanJoinRoom(message, room));
        }
    }



    public async Task LeaveRoom(IClient client)
        {
            _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName).RemoveClientFromRoom(client);
            await SendMessageToRoom(ConstMasseges.ServerConst, client.Username+ " has left the room " + client.RoomName, client.RoomName);
            _chatServer.rooms.FirstOrDefault(r => r.Name == "Main").AddClientToRoom(client);
            client.RoomName = "Main";
            await SendMessageToRoom(ConstMasseges.ServerConst, $"{client.Username} has joined the room {client.RoomName}", client.RoomName);
        }

    public async Task HandleInviteRoom(IClient client, string message)
        {
            var parts = message.Split(' ');
            if (ConstCheckCommands.CanInviteToRoom(message, client) == "true")
            {
                string roomName = parts[1];
                var invitedClient = _chatServer.clients.FirstOrDefault(c => c.Username == client.Username && c.RoomName == roomName);
                if (invitedClient != null)
                {
                    await _chatServer.ServerPrivateMessage(client, ConstMasseges.InvitationToRoomMassage + roomName);
                }
            }
        }

    public async Task PrintRooms(IClient client)
        {
            string message = "";
            foreach (var room in _chatServer.rooms)
            {
                if (room.Name.StartsWith("|private|"))
                {
                    
                }
                else
                {
                    message += $"\nRoom {room.Name}";
                    foreach (var member in room.Members)
                    {
                        message += $"\n| <{member.Username}>" + (member.Username == client.Username ? " (you)" : "");
                    }
                    message += "\n";
                }
            }
            await _chatServer.ServerPrivateMessage(client, message);
        }
        
        
        

}