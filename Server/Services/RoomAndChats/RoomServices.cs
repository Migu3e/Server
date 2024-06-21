using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Chat.Encryption;
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
        var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
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
            if (roomName == ConstMasseges.DefaultRoom)
            {
                _chatServer.ServerPrivateMessage(_chatServer.clients.FirstOrDefault(p => username == p.Username),ConstMasseges.CannotMessageInMain);
            }
            else
            {
                foreach (var member in room.Members)
                {
                    if (member.Username != username)
                    {
                        var response = ConstFunctions.Response(username, message);

                        var responseByte = Encoding.UTF8.GetBytes(response);
                        await member.ClientSocket.SendAsync(responseByte, SocketFlags.None);
                    }
                    
                    var newResponse = ConstFunctions.Response(username, message);;
                    room.Messages.Add(newResponse);

                    var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
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
    string validationMessage = ConstCheckCommands.CanCreateRoom(message, _chatServer.rooms);
    
    if (validationMessage == ConstMasseges.RoomWasCreated)
    {
        string roomName = parts[1];
        string password = parts[2];

        var room = new Room(roomName, password);
        _chatServer.rooms.Add(room);
        Console.WriteLine($"Room {roomName} was created by {client.Username}");
        await _chatServer.PrintToAll(client, ConstFunctions.RoomWasCreated(client.Username,roomName));
        Encrypt encrypt = new Encrypt();
        var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
        var data = new RoomDB
        {
            RoomName = roomName,
            Password = encrypt.encrypt(password),
            MList = new List<string>()
        };

        await collection.InsertOneAsync(data);
    }
    else
    {
        await _chatServer.PrivateMessage(client, validationMessage);
    }
}

public async Task HandleJoinRoom(IClient client, string message)
{
    var parts = message.Split(' ');
    
    if (parts.Length < 3)
    {
        await _chatServer.PrivateMessage(client, ConstMasseges.IncorrectNamePassword);
        return;
    }

    string roomName = parts[1];
    string password = parts[2];
    
    // Fetch the room document from MongoDB to get the password and message list (MList)
    var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
    var filter = Builders<RoomDB>.Filter.Eq(r => r.RoomName, roomName);
    var roomFromDb = await collection.Find(filter).FirstOrDefaultAsync();
    Decrypt decrypt = new Decrypt();

    // Validate the room and password
    if (roomFromDb == null || decrypt.decrypt(roomFromDb.Password) != password)
    {
        await _chatServer.PrivateMessage(client, ConstMasseges.IncorrectNamePassword);
        return;
    }

    // Find the room in the in-memory collection
    var room = _chatServer.rooms.FirstOrDefault(r => r.Name == roomName);
    string validationMessage = ConstCheckCommands.CanJoinRoom(message, room);

    if (validationMessage == "true")
    {
        // Remove the client from the current room
        var currentRoom = _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName);
        currentRoom?.RemoveClientFromRoom(client);
        Console.WriteLine($"Client {client.Username} has left room {client.RoomName}");

        // Add the client to the new room
        var newRoom = _chatServer.rooms.FirstOrDefault(r => r.Name == roomName);
        newRoom?.AddClientToRoom(client);
        client.RoomName = roomName;
        Console.WriteLine($"Client {client.Username} has joined room {client.RoomName}");

        foreach (var messageforeach in roomFromDb.MList)
        {
            await _chatServer.PrivateMessage(client, messageforeach);
        }

        // Notify the room that the client has joined
        await SendMessageToRoom(ConstMasseges.ServerConst, ConstFunctions.ClientJoinedRoom(roomName,client.Username), roomName);
    }
    else
    {
        await _chatServer.PrivateMessage(client, validationMessage);
    }
}


    public async Task HandleDeleteRoom(string message,IClient client)
    {
        var parts = message.Split(' ');
        var roomName = parts[1];

        var room = _chatServer.rooms.FirstOrDefault(r => r.Name == roomName);

        // Ensure the message has the correct format
        if (ConstCheckCommands.CanDeleteRoom(message,_chatServer.rooms) == ConstMasseges.DeletedRoom)
        {
        // Remove the room from the in-memory collection
            _chatServer.rooms.Remove(room);

            // Remove the room from the MongoDB collection
            var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
            var filter = Builders<RoomDB>.Filter.Eq(r => r.RoomName, roomName);
            await collection.DeleteOneAsync(filter);
            _chatServer.PrintToAll(client,ConstFunctions.RoomWasDeleted(roomName));

            Console.WriteLine($"Room '{roomName}' has been deleted.");            return;
        }
        else
        {
            _chatServer.ServerPrivateMessage(client,(ConstCheckCommands.CanDeleteRoom(message,_chatServer.rooms)));
        }


    }




    public async Task LeaveRoom(IClient client)
        {
            await SendMessageToRoom(ConstMasseges.ServerConst,ConstFunctions.ClientLeftRoom(client.RoomName,client.Username), client.RoomName);
            _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName).RemoveClientFromRoom(client);
            _chatServer.rooms.FirstOrDefault(r => r.Name == ConstMasseges.DefaultRoom).AddClientToRoom(client);
            client.RoomName = ConstMasseges.DefaultRoom;
            await SendMessageToRoom(ConstMasseges.ServerConst, ConstFunctions.ClientJoinedRoom(client.RoomName,client.Username), client.RoomName);
            
        }

    public async Task HandleInviteRoom(IClient client, string message)
    {
        var parts = message.Split(' ');
    
        if (parts.Length < 3)
        {
            await _chatServer.PrivateMessage(client, ConstMasseges.InvalidFormat);
            return;
        }

        string roomName = parts[1];
        string invitedUsername = parts[2];

        if (ConstCheckCommands.CanInviteToRoom(message, client) == "true")
        {
            var invitedClient = _chatServer.clients.FirstOrDefault(c => c.Username == invitedUsername);

            if (invitedClient != null)
            {
                // The invited client is online
                await _chatServer.ServerPrivateMessage(invitedClient, ConstFunctions.InviteToRoomMessege(roomName,client.Username));
                await _chatServer.PrivateMessage(client, ConstFunctions.InviteToRoom(roomName,invitedUsername));
            }
            else
            {
                // The invited client is not online
                await _chatServer.PrivateMessage(client, ConstMasseges.ClientIsOffline);
            }
        }
        else
        {
            await _chatServer.PrivateMessage(client, ConstCheckCommands.CanInviteToRoom(message, client));
        }
    }

    public async Task PrintRooms(IClient client)
        {
            string message = "";
            foreach (var room in _chatServer.rooms)
            {
                if (!ConstCheckCommands.IsPrivateRoom(room.Name))
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