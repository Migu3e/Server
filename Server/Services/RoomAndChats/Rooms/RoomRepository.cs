using MongoDB.Driver;
using Server.Const;
using Server.Interfaces.RoomsAndChats;
using Server.MongoDB;

namespace Server.Services.RoomAndChats.Rooms
{
    public class RoomRepository : IRoomRepository
    {
        public async Task<List<RoomDB>> GetAllRooms()
        {
            var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
            return await collection.Find(_ => true).ToListAsync();
        }

        public async Task InsertRoom(RoomDB room)
        {
            var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
            await collection.InsertOneAsync(room);
        }

        public async Task UpdateRoomMessages(string roomName, string message)
        {
            var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
            var filter = Builders<RoomDB>.Filter.Eq(r => r.RoomName, roomName);
            var update = Builders<RoomDB>.Update.Push(r => r.MList, message);
            await collection.UpdateOneAsync(filter, update);
        }

        public async Task DeleteRoom(string roomName)
        {
            var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
            var filter = Builders<RoomDB>.Filter.Eq(r => r.RoomName, roomName);
            await collection.DeleteOneAsync(filter);
        }

        public async Task<RoomDB> GetRoomByName(string roomName)
        {
            var collection = MongoDBRoomHelper.GetCollection<RoomDB>(ConstMasseges.CollectionChats);
            var filter = Builders<RoomDB>.Filter.Eq(r => r.RoomName, roomName);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }
    }


}