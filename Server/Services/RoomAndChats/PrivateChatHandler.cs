// File: Server/Services/PrivateChatHandler.cs

using Server.Interfaces;
using Server.Models;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Server.MongoDB;

namespace Server.Services
{
    public class PrivateChatHandler : IPrivateChatHandler
    {
        private readonly IChatServer _chatServer;
        private readonly IRoomServices _roomServices;

        public PrivateChatHandler(IChatServer server, IRoomServices roomServices)
        {
            _chatServer = server;
            _roomServices = roomServices;
        }

        public async Task CreatePrivateChats(IClient newClient)
        {
            var collection = MongoDBHelper.GetCollection<RoomDB>("chats");

            foreach (var existingClient in _chatServer.clients)
            {
                if (existingClient != newClient)
                {
                    string chatName = $"|private|.{existingClient.Username}.-.{newClient.Username}.";
                    string chatNameSecondWay = $"|private|.{newClient.Username}.-.{existingClient.Username}.";

                    // Check if the room already exists in the database
                    var filter = Builders<RoomDB>.Filter.Eq(r => r.RoomName, chatName);
                    var filterSecondWay = Builders<RoomDB>.Filter.Eq(r => r.RoomName, chatNameSecondWay);
                    var roomFromDb = await collection.Find(filter).FirstOrDefaultAsync();
                    var roomFromDbSecondWay = await collection.Find(filter).FirstOrDefaultAsync();


                    if (roomFromDb == null&& roomFromDbSecondWay == null)
                    {
                        IRoom privateChat = new Room(chatName);
                        _chatServer.rooms.Add(privateChat);

                        // Save the room to the database
                        var data = new RoomDB
                        {
                            RoomName = chatName,
                            MList = new List<string>()
                        };
                        await collection.InsertOneAsync(data);

                        Console.WriteLine($"Private chat '{chatName}' created between {existingClient.Username} and {newClient.Username}");
                    }
                }
            }
        }
        public async Task HandleJoinPrivateRoom(IClient client, string message)
        {
            var parts = message.Split(' ');
            if (parts.Length < 2)
            {
                await _chatServer.ServerPrivateMessage(client, "Invalid message format.");
                return;
            }

            string targetUsername = parts[1];
            if (client.Username == targetUsername)
            {
                await _chatServer.ServerPrivateMessage(client, "Error: Cannot join a private room with yourself.");
                return;
            }

            var room = _chatServer.rooms.FirstOrDefault(r => r.Name.Contains($".{targetUsername}.") && r.Name.Contains($".{client.Username}."));
            if (room != null)
            {
                var currentRoom = _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName);
                if (currentRoom != null)
                {
                    currentRoom.RemoveClientFromRoom(client);
                    Console.WriteLine($"Client {client.Username} has left room {client.RoomName}");
                }
                foreach (var newClient in _chatServer.clients.Where(c => c.Username == targetUsername))
                {
                    _chatServer.ServerPrivateMessage(newClient,
                        $"{client.Username} Has Enter The Private Chat With You");
                }

                room.AddClientToRoom(client);
                client.RoomName = room.Name;
                Console.WriteLine($"Client {client.Username} has joined private room {client.RoomName}");

                // Fetch messages from the MongoDB collection
                var collection = MongoDBHelper.GetCollection<RoomDB>("chats");
                var filter = Builders<RoomDB>.Filter.Eq(r => r.RoomName, room.Name);
                var roomFromDb = await collection.Find(filter).FirstOrDefaultAsync();

                if (roomFromDb != null)
                {
                    foreach (var existingMessage in roomFromDb.MList)
                    {
                        await _chatServer.PrivateMessage(client, existingMessage);
                    }
                }

                await _roomServices.SendMessageToRoom("Server", $"{client.Username} has joined the private room {client.RoomName}", client.RoomName);
            }
            else
            {
                await _chatServer.ServerPrivateMessage(client, $"Room with {targetUsername} doesn't exist.");
            }
        }

    }
}
