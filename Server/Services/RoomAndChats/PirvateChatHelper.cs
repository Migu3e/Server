using MongoDB.Driver;
using Server.Const;
using Server.Interfaces;
using Server.Interfaces.RoomsAndChats;
using Server.MongoDB;

namespace Server.Services.RoomAndChats
{
    public class PrivateChatHelper : IPrivateChatHelper
    {
        private readonly IChatServer _chatServer;
        private readonly IRoomServices _roomServices;
        private readonly IMessageFormatter _messageFormatter;

        public PrivateChatHelper(IChatServer chatServer, IRoomServices roomServices, IMessageFormatter messageFormatter)
        {
            _chatServer = chatServer;
            _roomServices = roomServices;
            _messageFormatter = messageFormatter;
        }

        public async Task LeaveCurrentRoom(IClient client)
        {
            var currentRoom = _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName);
            if (currentRoom != null)
            {
                currentRoom.RemoveClientFromRoom(client);
                Console.WriteLine($"Client {client.Username} has left room {client.RoomName}");
            }
        }

        public async Task NotifyTargetUser(string targetUsername, string clientUsername)
        {
            foreach (var newClient in _chatServer.clients.Where(c => c.Username == targetUsername))
            {
                await _chatServer.ServerPrivateMessage(newClient, _messageFormatter.UserHasEnteredPrivateChat(clientUsername));
            }
        }

        public async Task JoinRoom(IClient client, IRoom room, string targetUsername)
        {
            room.AddClientToRoom(client);
            client.RoomName = room.Name;
            Console.WriteLine($"Client {client.Username} has joined private room {client.RoomName}");

            var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
            var filter = Builders<RoomDB>.Filter.Eq(r => r.RoomName, room.Name);
            var roomFromDb = await collection.Find(filter).FirstOrDefaultAsync();
            if (roomFromDb != null)
            {
                foreach (var existingMessage in roomFromDb.MList)
                {
                    await _chatServer.PrivateMessage(client, existingMessage);
                }
            }

            await _roomServices.SendMessageToRoom(ConstMasseges.ServerConst, _messageFormatter.UserJoinedPrivateRoom(client.Username, client.RoomName), client.RoomName);
        }
    }
}
