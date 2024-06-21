// File: Server/Services/PrivateChatHandler.cs

using Server.Interfaces;
using Server.Models;
using System.Linq;
using System.Threading.Tasks;
using Client.MongoDB;
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
        
        public async Task CreatePrivateChats()
        {
            try
            {
                var collection = MongoDBHelper.GetCollection<RoomDB>("chats");
                var clientsCollection = MongoDBClientHelper.GetCollection<ClientDB>("dataclient");

                // Get all clients from the database
                var existingClients = await clientsCollection.Find(_ => true).ToListAsync();

                // Log the number of clients fetched
                Console.WriteLine($"Number of clients fetched: {existingClients.Count}");

                // Get all existing room names from the database
                var existingRooms = await collection.Find(_ => true).ToListAsync();
                var existingRoomNames = existingRooms.Select(r => r.RoomName).ToList();

                // List to accumulate new rooms to be inserted
                var newRooms = new List<RoomDB>();

                foreach (var existingClient in existingClients)
                {
                    Console.WriteLine($"Processing client: {existingClient.UserName}");
                    foreach (var secondClient in existingClients)
                    {
                        if (existingClient.UserName != secondClient.UserName)
                        {
                            string chatName = $"|private|.{existingClient.UserName}.-.{secondClient.UserName}.";
                            string chatNameSecondWay = $"|private|.{secondClient.UserName}.-.{existingClient.UserName}.";

                            // Check if the room already exists in the existingRoomNames list
                            if (!existingRoomNames.Contains(chatName) && !existingRoomNames.Contains(chatNameSecondWay))
                            {
                                IRoom privateChat = new Room(chatName);
                                _chatServer.rooms.Add(privateChat);

                                // Add the new room to the list
                                newRooms.Add(new RoomDB
                                {
                                    RoomName = chatName,
                                    MList = new List<string>()
                                });

                                // Add the room names to the list to avoid duplicates
                                existingRoomNames.Add(chatName);
                                existingRoomNames.Add(chatNameSecondWay);
                            }
                        }
                    }
                }

                // Perform a batch insertion for all new rooms
                if (newRooms.Count > 0)
                {
                    await collection.InsertManyAsync(newRooms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
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
