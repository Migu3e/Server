using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.MongoDB
{
    public class ClientDB
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("UserName")]
        public string UserName { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }
    }
}