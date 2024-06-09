
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.MongoDB
{
    public class RoomDB
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Name")]
        public string RoomName { get; set; }
        
        [BsonElement("roommessages")] public List<string> MList { get; set; }
    }
}