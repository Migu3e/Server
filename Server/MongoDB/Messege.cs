// File: Server/MongoDB/Message.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.MongoDB
{
    public class Message
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("RoomName")]
        public string RoomName { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("Text")]
        public string Text { get; set; }

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
