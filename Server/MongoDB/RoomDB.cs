using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Server.MongoDB
{
    public class RoomDB
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Name")]
        public string RoomName { get; set; }
        
        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonElement("roommessages")]
        public List<string> MList { get; set; }
    }
}